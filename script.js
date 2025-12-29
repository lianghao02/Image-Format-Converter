/**
 * 警務影像轉換器 - 核心邏輯 (Refactored)
 * 轉檔預設執行：縮放 (DPI) -> 自動銳化 -> 切片 (長截圖判定)
 */

const app = (function () {
  // --- DOM 元素快取 ---
  let fileInput, fileListContainer, dropArea;
  let btnConvert, btnReconvert, btnClear;
  let progressArea, progressBar, statusText, dpiBtns, chkSharpen;
  
  // --- 狀態變數 ---
  let selectedFiles = [];
  let processedImages = [];

  // --- 本機配置 ---
  // 使用 config.js 中的 CONFIG_MANAGER
  let config = { ...CONFIG_MANAGER.DEFAULT_CONFIG };

  /**
   * 初始化應用程式
   */
  function init() {
    console.log("應用程式初始化中...");
    
    // 檢查 CONFIG_MANAGER 是否存在
    if (typeof CONFIG_MANAGER === 'undefined') {
      console.error("錯誤：CONFIG_MANAGER 未定義，請檢查 config.js 是否正確載入。");
      return;
    }

    loadUserPreferences(); // 載入使用者習慣設定

    // 取得元件引用
    fileInput = document.getElementById("fileInput");
    fileListContainer = document.getElementById("fileList");
    dropArea = document.getElementById("dropArea");
    btnConvert = document.getElementById("btnConvert");
    btnReconvert = document.getElementById("btnReconvert");
    btnClear = document.getElementById("btnClear");

    // 驗證必要元件是否存在
    const requiredElements = { fileInput, dropArea, btnConvert, btnClear };
    for (const [name, el] of Object.entries(requiredElements)) {
      if (!el) console.error(`找不到必要元件: ${name}`);
    }

    progressArea = document.getElementById("progressArea");
    progressBar = document.getElementById("progressBar");
    statusText = document.getElementById('statusText');
    dpiBtns = document.querySelectorAll('.dpi-btn');
    chkSharpen = document.getElementById('chkSharpen');

    bindEvents();
    syncConfigToUI(); // 同步配置至介面
    console.log("應用程式初始化完成。");
  }

  /**
   * 從 localStorage 載入使用者偏好
   */
  function loadUserPreferences() {
    const saved = localStorage.getItem(CONFIG_MANAGER.STORAGE_KEY);
    if (saved) {
      try {
        config = { ...CONFIG_MANAGER.DEFAULT_CONFIG, ...JSON.parse(saved) };
      } catch (e) {
        console.error("無法剖析使用者設定:", e);
      }
    }
  }

  /**
   * 儲存設定至 localStorage
   */
  function saveUserPreferences() {
    localStorage.setItem(CONFIG_MANAGER.STORAGE_KEY, JSON.stringify(config));
  }

  /**
   * 事件綁定
   */
  function bindEvents() {
    // 檔案拖放處理
    ["dragenter", "dragover"].forEach((eventName) => {
      dropArea.addEventListener(eventName, (e) => {
        e.preventDefault();
        dropArea.classList.add("border-success", "bg-[#f0fff4]");
        dropArea.classList.remove("border-[#cbd5e0]", "bg-white");
      });
    });

    ["dragleave", "drop"].forEach((eventName) => {
      dropArea.addEventListener(eventName, (e) => {
        e.preventDefault();
        dropArea.classList.remove("border-success", "bg-[#f0fff4]");
        dropArea.classList.add("border-[#cbd5e0]", "bg-white");
      });
    });

    dropArea.addEventListener("drop", (e) => handleFilesUpdate(e.dataTransfer.files));
    dropArea.addEventListener('click', () => fileInput.click()); 
    fileInput.addEventListener("change", (e) => handleFilesUpdate(e.target.files));

    // 按鈕點擊事件
    btnConvert.addEventListener("click", () => startConversionProcess());

    btnReconvert.addEventListener("click", () => {
      if (confirm("確定要套用最新設定並重新轉換嗎？")) {
        startConversionProcess();
      }
    });

    btnClear.addEventListener("click", resetAll);

    // DPI 選項切換
    dpiBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        const dpiValue = parseInt(btn.dataset.value);
        config.dpi = dpiValue;
        saveUserPreferences();
        updateDpiButtonsUI(dpiValue);
      });
    });
  }

  /**
   * 更新 DPI 按鈕的視覺狀態
   * @param {number} activeDpi 
   */
  function updateDpiButtonsUI(activeDpi) {
    dpiBtns.forEach(btn => {
      const isSelected = parseInt(btn.dataset.value) === activeDpi;
      if (isSelected) {
        btn.setAttribute('data-active', 'true');
        btn.classList.add('bg-accent', 'text-white', 'shadow-sm', 'border-accent');
        btn.classList.remove('border-transparent', 'text-slate-600', 'hover:bg-slate-50');
      } else {
        btn.removeAttribute('data-active');
        btn.classList.remove('bg-accent', 'text-white', 'shadow-sm', 'border-accent');
        btn.classList.add('border-transparent', 'text-slate-600', 'hover:bg-slate-50');
      }
    });
  }

  /**
   * 重置所有狀態與介面
   */
  function resetAll() {
    selectedFiles = [];
    processedImages = [];
    fileInput.value = "";

    fileListContainer.classList.add("hidden");
    fileListContainer.innerHTML = "";
    progressArea.classList.add("hidden");
    statusText.innerText = "";
    progressBar.style.width = "0%";

    btnConvert.removeAttribute("data-active");
    btnClear.removeAttribute("data-active");

    btnReconvert.classList.add("hidden");
    btnConvert.classList.remove("hidden");
    btnConvert.innerHTML = '<span><i class="fa-solid fa-rocket"></i> 開始轉換</span>';

    // 移除動態生成的下載按鈕
    const dynamicButtons = ["btnExportExcel", "btnDownloadZip"];
    dynamicButtons.forEach(id => {
      const el = document.getElementById(id);
      if (el) el.remove();
    });
  }

  /**
   * 處理檔案選取更新
   * @param {FileList} files 
   */
  function handleFilesUpdate(files) {
    // 過濾支援的圖片格式
    const validFiles = Array.from(files).filter(file => {
      const extension = file.name.slice(file.name.lastIndexOf(".")).toLowerCase();
      return CONFIG_MANAGER.SUPPORTED_TYPES.includes(file.type) || 
             CONFIG_MANAGER.SUPPORTED_EXTENSIONS.includes(extension);
    });

    if (validFiles.length === 0) {
      alert("不支援的檔案格式！");
      return;
    }

    selectedFiles = validFiles;

    // 更新檔案清單介面
    fileListContainer.classList.remove("hidden");
    fileListContainer.innerHTML = selectedFiles
      .map(file => `
        <div class="file-item flex justify-between text-[13px] py-1.5 border-b border-[#f0f0f0] text-[#555] last:border-b-0">
            <span>${file.name}</span>
            <span>${(file.size / 1024).toFixed(1)} KB</span>
        </div>
      `).join("");

    btnConvert.setAttribute("data-active", "true");
    btnClear.setAttribute("data-active", "true");
    btnReconvert.classList.add("hidden");
    btnConvert.classList.remove("hidden");
    progressArea.classList.add("hidden");

    // 清除舊有的下載按鈕
    ["btnExportExcel", "btnDownloadZip"].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.remove();
    });
  }

  /**
   * 開始轉換流程
   */
  async function startConversionProcess() {
    if (selectedFiles.length === 0) return;

    const dpi = config.dpi;
    const isSharpenEnabled = chkSharpen.checked;

    // 計算目標寬度 (像素)： (20cm / 2.54) * DPI
    const targetWidthPx = Math.round((config.a4WidthCm / 2.54) * dpi);

    // UI 鎖定
    btnConvert.removeAttribute("data-active");
    btnReconvert.classList.add("hidden");
    btnClear.removeAttribute("data-active");
    progressArea.classList.remove("hidden");

    processedImages = [];
    let successCount = 0;

    for (let i = 0; i < selectedFiles.length; i++) {
      const file = selectedFiles[i];
      statusText.innerText = `正在處理 (${i + 1}/${selectedFiles.length})：${file.name}`;
      progressBar.style.width = `${((i) / selectedFiles.length) * 100}%`;

      try {
        const results = await processSingleImage(file, targetWidthPx, isSharpenEnabled);
        processedImages.push(...results);
        successCount++;
      } catch (err) {
        console.error(`處理失敗 [${file.name}]:`, err);
      }
    }

    statusText.innerText = `✅ 成功處理 ${successCount} 張，生成 ${processedImages.length} 張切片。`;
    progressBar.style.width = "100%";

    btnConvert.classList.add("hidden");
    btnReconvert.classList.remove("hidden");
    btnClear.setAttribute("data-active", "true");

    showResultButtons();
  }

  /**
   * 處理單一圖片轉換與切片
   */
  async function processSingleImage(file, targetWidthPx, isSharpenEnabled) {
    let currentBlob = file;
    
    // HEIC 轉換預處裡
    if (file.name.toLowerCase().endsWith(".heic") || file.type === "image/heic") {
      const heicBlob = await heic2any({
        blob: file,
        toType: "image/jpeg",
        quality: 0.92,
      });
      currentBlob = Array.isArray(heicBlob) ? heicBlob[0] : heicBlob;
    }

    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = async () => {
        const results = [];
        const originalW = img.width;
        const originalH = img.height;
        const ratio = originalH / originalW;

        const scale = targetWidthPx / originalW; // 縮放係數
        const isLongMobile = ratio > config.ratioThreshold; // 是否為長截圖
        const baseName = file.name.replace(/\.[^/.]+$/, "");

        if (isLongMobile) {
          // --- 長截圖切片模式 ---
          const sliceHeightSource = Math.floor(originalW * 1.6); // 切片高度設為寬度的 1.6 倍
          const overlap = config.overlapSource;
          let currentY = 0;
          let sliceIndex = 1;

          while (currentY < originalH) {
            let actualSliceH = sliceHeightSource;
            if (currentY + actualSliceH > originalH) actualSliceH = originalH - currentY;

            const targetSliceH = Math.floor(actualSliceH * scale);
            const blob = await generateProcessedBlob(
              img, 0, currentY, originalW, actualSliceH, 
              targetWidthPx, targetSliceH, isSharpenEnabled
            );
            
            results.push({
              name: `${baseName}_${String(sliceIndex).padStart(2, "0")}.jpg`,
              blob: blob,
              width: targetWidthPx,
              height: targetSliceH
            });

            if (currentY + actualSliceH >= originalH) break;
            currentY += (sliceHeightSource - overlap);
            sliceIndex++;
          }
        } else {
          // --- 一般模式 ---
          const targetHeight = Math.floor(originalH * scale);
          const blob = await generateProcessedBlob(
            img, 0, 0, originalW, originalH, 
            targetWidthPx, targetHeight, isSharpenEnabled
          );
          results.push({
            name: `${baseName}.jpg`,
            blob: blob,
            width: targetWidthPx,
            height: targetHeight
          });
        }
        resolve(results);
      };
      img.onerror = () => reject(new Error("載入圖片發生錯誤"));
      img.src = URL.createObjectURL(currentBlob);
    });
  }

  /**
   * 生成經過處理 (縮放、銳化) 的 Blob
   */
  function generateProcessedBlob(img, sx, sy, sw, sh, dw, dh, doSharpen) {
    return new Promise(resolve => {
      const canvas = document.createElement('canvas');
      canvas.width = dw;
      canvas.height = dh;
      const ctx = canvas.getContext('2d');

      ctx.imageSmoothingEnabled = true;
      ctx.imageSmoothingQuality = 'high';
      ctx.drawImage(img, sx, sy, sw, sh, 0, 0, dw, dh);

      if (doSharpen) {
        applySharpenFilter(ctx, dw, dh, config.sharpenMix);
      }

      canvas.toBlob(resolve, 'image/jpeg', config.outputQuality);
    });
  }

  /**
   * 銳化濾鏡核心
   * @param {CanvasRenderingContext2D} ctx 
   * @param {number} w 
   * @param {number} h 
   * @param {number} mix 
   */
  function applySharpenFilter(ctx, w, h, mix) {
    const imageData = ctx.getImageData(0, 0, w, h);
    const data = imageData.data;
    const inputData = new Uint8ClampedArray(data);
    
    // 卷積核心 (3x3)
    const kernel = [0, -1, 0, -1, 5, -1, 0, -1, 0];

    const clamp = (val) => Math.min(255, Math.max(0, val));

    for (let y = 1; y < h - 1; y++) {
      for (let x = 1; x < w - 1; x++) {
        const idx = (y * w + x) * 4;
        let r = 0, g = 0, b = 0;

        for (let ky = -1; ky <= 1; ky++) {
          for (let kx = -1; kx <= 1; kx++) {
            const pixelIdx = ((y + ky) * w + (x + kx)) * 4;
            const weight = kernel[(ky + 1) * 3 + (kx + 1)];
            r += inputData[pixelIdx] * weight;
            g += inputData[pixelIdx + 1] * weight;
            b += inputData[pixelIdx + 2] * weight;
          }
        }

        data[idx] = clamp(inputData[idx] * (1 - mix) + r * mix);
        data[idx + 1] = clamp(inputData[idx + 1] * (1 - mix) + g * mix);
        data[idx + 2] = clamp(inputData[idx + 2] * (1 - mix) + b * mix);
      }
    }
    ctx.putImageData(imageData, 0, 0);
  }

  /**
   * 顯示下載 ZIP 按鈕
   */
  function showResultButtons() {
    const btnGroup = document.querySelector(".btn-group");
    if (document.getElementById("btnDownloadZip")) return;

    const btn = document.createElement("button");
    btn.id = "btnDownloadZip";
    btn.className = "btn btn-success bg-success text-white hover:bg-[#219150] border-none py-3 px-5 rounded-xl text-[15px] font-semibold cursor-pointer transition-all duration-200 flex-1 flex items-center justify-center gap-2";
    btn.innerHTML = '<span><i class="fa-solid fa-file-zipper"></i> 下載圖片 (ZIP)</span>';
    btn.onclick = executeZipDownload;
    btnGroup.appendChild(btn);
  }

  /**
   * 執行 ZIP 封裝與下載
   */
  async function executeZipDownload() {
    if (processedImages.length === 0) return;
    
    statusText.innerText = "⏳ 正在打包壓縮檔...";
    const zip = new JSZip();

    processedImages.forEach(img => {
      if (img.blob) zip.file(img.name, img.blob);
    });

    const content = await zip.generateAsync({ type: "blob" });
    const timeRef = new Date().toISOString().replace(/[-:T]/g, "").slice(0, 12);
    
    if (typeof saveAs !== "undefined") {
      saveAs(content, `影像轉檔包_${timeRef}.zip`);
    } else {
      const url = URL.createObjectURL(content);
      const a = document.createElement("a");
      a.href = url;
      a.download = `影像轉檔包_${timeRef}.zip`;
      a.click();
      URL.revokeObjectURL(url);
    }
    statusText.innerText = "✅ 下載已啟動";
  }

  /**
   * 同步配置資料至進階面板 UI
   */
  function syncConfigToUI() {
    const mapping = {
      'cfg-overlap': 'overlapSource',
      'cfg-threshold': 'ratioThreshold',
      'cfg-a4width': 'a4WidthCm',
      'cfg-sharpen': 'sharpenMix',
      'cfg-quality': 'outputQuality'
    };

    for (let domId in mapping) {
      const el = document.getElementById(domId);
      const display = document.getElementById(domId + '-val');
      const key = mapping[domId];

      if (el) {
        el.value = config[key];
        if (display) display.innerText = config[key];

        el.oninput = (e) => {
          let val = parseFloat(e.target.value);
          if (domId === 'cfg-overlap') val = parseInt(e.target.value);
          config[key] = val;
          saveUserPreferences();
          if (display) display.innerText = val;
        };
      }
    }

    updateDpiButtonsUI(config.dpi);
    initSidePanelLogic();
  }

  /**
   * 初始化側邊面板 (抽屜) 互動邏輯
   */
  function initSidePanelLogic() {
    const btnToggle = document.getElementById('btnToggleConfig');
    const btnClose = document.getElementById('btnCloseConfig');
    const panel = document.getElementById('configPanel');
    const overlay = document.getElementById('configOverlay');
    const btnReset = document.getElementById('btnResetConfig');

    const setDrawerState = (isOpen) => {
      if (isOpen) {
        overlay.classList.remove('hidden');
        void overlay.offsetWidth;
        overlay.classList.add('opacity-100');
        panel.setAttribute('data-open', 'true');
        document.body.style.overflow = 'hidden';
      } else {
        overlay.classList.remove('opacity-100');
        panel.removeAttribute('data-open');
        document.body.style.overflow = '';
        setTimeout(() => {
          if (!panel.hasAttribute('data-open')) overlay.classList.add('hidden');
        }, 300);
      }
    };

    if (btnToggle) btnToggle.onclick = (e) => (e.preventDefault(), setDrawerState(true));
    if (btnClose) btnClose.onclick = () => setDrawerState(false);
    if (overlay) overlay.onclick = () => setDrawerState(false);

    if (btnReset) {
      btnReset.onclick = () => {
        if (confirm("恢復預設設定值？")) {
          config = { ...CONFIG_MANAGER.DEFAULT_CONFIG };
          saveUserPreferences();
          syncConfigToUI();
        }
      };
    }
  }

  return { init };
})();

document.addEventListener("DOMContentLoaded", app.init);
