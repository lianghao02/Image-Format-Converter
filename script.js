const app = (function () {
  // 私有變數
  let fileInput, fileListContainer, dropArea;
  let btnConvert, btnReconvert, btnClear, btnExportExcel;
  let progressArea, progressBar, statusText, dpiBtns, chkSharpen;
  let selectedFiles = [];
  let processedImages = [];

  // --- 參數解耦配置 (Config) ---
  const STORAGE_KEY = "HEIC_TO_JPG_user_config";
  const DEFAULT_CONFIG = {
    dpi: 220,               // 預設 DPI (220)
    overlapSource: 150,     // 分割重疊像素 (150px)
    ratioThreshold: 2.5,    // 長截圖判定比例 (2.5)
    a4WidthCm: 20,          // 目標 A4 寬度 (20cm)
    sharpenMix: 0.3,        // 銳化混合強度 (0.3)
    outputQuality: 0.95     // JPEG 輸出品質 (0.95)
  };
  let config = { ...DEFAULT_CONFIG };

  // 初始化應用程式
  function init() {
    loadConfig(); // 載入使用者設定
    // 初始化元素
    fileInput = document.getElementById("fileInput");
    fileListContainer = document.getElementById("fileList");
    dropArea = document.getElementById("dropArea");
    btnConvert = document.getElementById("btnConvert");
    btnReconvert = document.getElementById("btnReconvert");
    btnClear = document.getElementById("btnClear");

    progressArea = document.getElementById("progressArea");
    progressBar = document.getElementById("progressBar");
    statusText = document.getElementById('statusText');
    dpiBtns = document.querySelectorAll('.dpi-btn');
    chkSharpen = document.getElementById('chkSharpen');

    setupEventListeners();
    syncConfigToUI(); // 將設定值同步到面板 (稍後實作面板後會使用)
  }

  // 載入與儲存 Config 邏輯
  function loadConfig() {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      try {
        config = { ...DEFAULT_CONFIG, ...JSON.parse(saved) };
      } catch (e) {
        console.error("Config parse error:", e);
      }
    }
  }

  function saveConfig() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(config));
  }

  function setupEventListeners() {
    // 拖放事件 (Drag & Drop)
    ["dragenter", "dragover"].forEach((e) => {
      dropArea.addEventListener(e, (ev) => {
        ev.preventDefault();
        // 切換為啟動狀態的 Tailwind 類別
        dropArea.classList.add("border-success", "bg-[#f0fff4]");
        dropArea.classList.remove("border-[#cbd5e0]", "bg-white");
      });
    });
    ["dragleave", "drop"].forEach((e) => {
      dropArea.addEventListener(e, (ev) => {
        ev.preventDefault();
        // 還原為預設狀態
        dropArea.classList.remove("border-success", "bg-[#f0fff4]");
        dropArea.classList.add("border-[#cbd5e0]", "bg-white");
      });
    });
    dropArea.addEventListener("drop", (e) => handleFiles(e.dataTransfer.files));
    dropArea.addEventListener('click', () => fileInput.click()); // 補上點擊選取事件
    fileInput.addEventListener("change", (e) => handleFiles(e.target.files));

    // 按鈕事件
    btnConvert.addEventListener("click", () => startProcess());

    btnReconvert.addEventListener("click", () => {
      if (confirm("確定要使用新的設定重新轉換一次嗎？")) {
        startProcess();
      }
    });

    btnClear.addEventListener("click", clearAll);

    // DPI 按鈕點擊事件
    dpiBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        // 更新當前 DPI 並儲存
        const val = parseInt(btn.dataset.value);
        config.dpi = val;
        saveConfig();
        
        // 更新 UI 狀態
        updateDpiButtonsUI(val);
      });
    });
  }

  // 抽離 DPI 按鈕 UI 更新邏輯
  function updateDpiButtonsUI(activeDpi) {
    dpiBtns.forEach(b => {
      const isSelected = parseInt(b.dataset.value) === activeDpi;
      if (isSelected) {
        b.setAttribute('data-active', 'true');
        b.classList.add('bg-accent', 'text-white', 'shadow-sm', 'border-accent');
        b.classList.remove('border-transparent', 'text-slate-600', 'hover:bg-slate-50');
      } else {
        b.removeAttribute('data-active');
        b.classList.remove('bg-accent', 'text-white', 'shadow-sm', 'border-accent');
        b.classList.add('border-transparent', 'text-slate-600', 'hover:bg-slate-50');
      }
    });
  }

  function clearAll() {
    selectedFiles = [];
    processedImages = [];
    fileInput.value = "";

    // UI 重置 (使用 Tailwind 類別控制顯示/隱藏)
    fileListContainer.classList.add("hidden");
    fileListContainer.innerHTML = "";
    progressArea.classList.add("hidden");
    statusText.innerText = "";
    progressBar.style.width = "0%";

    // 按鈕狀態重置
    // 使用 data attribute 來控制狀態，配合 CSS selector (data-[active=true])
    btnConvert.removeAttribute("data-active");
    btnClear.removeAttribute("data-active");

    btnReconvert.classList.add("hidden");
    btnConvert.classList.remove("hidden");
    btnConvert.innerHTML =
      '<span><i class="fa-solid fa-rocket"></i> 開始轉換</span>';

    // 移除 Excel 按鈕 (如果存在)
    const oldExcelBtn = document.getElementById("btnExportExcel");
    if (oldExcelBtn) oldExcelBtn.remove();

    // 移除 ZIP 下載按鈕 (如果存在)
    const oldZipBtn = document.getElementById("btnDownloadZip");
    if (oldZipBtn) oldZipBtn.remove();
  }

  function handleFiles(files) {
    // 過濾圖片檔案
    const newFiles = Array.from(files).filter(
      (f) =>
        f.type.startsWith("image/") || f.name.toLowerCase().endsWith(".heic")
    );
    if (newFiles.length === 0) {
      alert("請選取有效的圖片檔案！");
      return;
    }

    selectedFiles = newFiles;

    // 更新 UI
    fileListContainer.classList.remove("hidden");
    fileListContainer.innerHTML = selectedFiles
      .map(
        (f) =>
          `<div class="file-item flex justify-between text-[13px] py-1.5 border-b border-[#f0f0f0] text-[#555] last:border-b-0">
                <span>${f.name}</span>
                <span>${(f.size / 1024).toFixed(1)} KB</span>
            </div>`
      )
      .join("");

    // 啟動按鈕
    btnConvert.setAttribute("data-active", "true");
    btnClear.setAttribute("data-active", "true");

    btnReconvert.classList.add("hidden");
    btnConvert.classList.remove("hidden"); // 確保開始轉換按鈕顯示
    progressArea.classList.add("hidden");

    // 清除舊的動態按鈕
    const oldExcelBtn = document.getElementById("btnExportExcel");
    if (oldExcelBtn) oldExcelBtn.remove();
    const oldZipBtn = document.getElementById("btnDownloadZip");
    if (oldZipBtn) oldZipBtn.remove();
  }

  async function startProcess() {
    if (selectedFiles.length === 0) return;

    const dpi = config.dpi;
    const performSharpen = chkSharpen.checked;

    // 設定最大長邊 (依據 DPI 與 A4 寬度約 CONFIG.a4WidthCm 計算)
    const maxLongSide = Math.round((config.a4WidthCm / 2.54) * dpi);

    // 鎖定按鈕
    btnConvert.removeAttribute("data-active");
    btnReconvert.classList.remove("hidden"); // 顯示重新轉換按鈕但不啟用
    // (這裡設計有點特別，重新轉換按鈕一開始不給按，直到完成才亮起)
    // 暫時將其隱藏，直到完成
    btnReconvert.classList.add("hidden");

    btnClear.removeAttribute("data-active");
    progressArea.classList.remove("hidden");

    processedImages = [];
    let processedCount = 0;
    let failCount = 0;

    for (let i = 0; i < selectedFiles.length; i++) {
      const file = selectedFiles[i];
      statusText.innerText = `正在處理 (${i + 1}/${selectedFiles.length})：${
        file.name
      }`;
      progressBar.style.width = `${(i / selectedFiles.length) * 90}%`;

      try {
        const results = await processImage(file, maxLongSide, performSharpen);
        processedImages.push(...results);
      } catch (e) {
        console.error(e);
        failCount++;
      }
      processedCount++;
    }

    statusText.innerText = `✅ 處理完成！成功 ${
      processedCount - failCount
    } 張原始圖片，生成 ${processedImages.length} 個切片`;
    progressBar.style.width = "100%";

    // 完成狀態更新
    btnConvert.classList.add("hidden");
    btnReconvert.classList.remove("hidden");
    // 啟用按鈕
    // btnReconvert 本身沒有 data-active 樣式，我們手動給它 style 或是它原本就沒有 disabled 狀態
    // 根據 HTML，它預設是 hidden。顯示即代表可用。

    btnClear.setAttribute("data-active", "true");

    // 顯示下載 ZIP 按鈕
    showDownloadButton();
  }

  // 處理單張圖片 (轉檔 + 切圖 + 縮放邏輯 + 銳化)
  async function processImage(file, maxLongSide, performSharpen) {
    let srcBlob = file;
    // 若為 HEIC 格式，先轉為 JPEG
    if (
      file.name.toLowerCase().endsWith(".heic") ||
      file.type === "image/heic"
    ) {
      try {
        const heicBlob = await heic2any({
          blob: file,
          toType: "image/jpeg",
          quality: 0.92,
        });
        srcBlob = Array.isArray(heicBlob) ? heicBlob[0] : heicBlob;
      } catch (err) {
        // 錯誤處理：轉換失敗
        throw new Error(`HEIC 轉換失敗: ${file.name}`);
      }
    }

    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = async () => {
        const results = [];
        const w = img.width;
        const h = img.height;
        const ratio = h / w;

        // --- 核心修改：實作 DPI 縮放邏輯 ---
        // maxLongSide 原意為「最大長邊」，在此計畫中被定義為「目標寬度」 (對應 A4 寬度約 20cm)
        // 我們強制將圖片寬度縮放至 targetWidth，高度依比例調整
        const targetWidth = maxLongSide;
        const scale = targetWidth / w; // 計算縮放比例

        // 自動偵測邏輯：
        // 若 高/寬 比 > config.ratioThreshold，視為手機長截圖 -> 進行切片
        // 若 高/寬 比 <= config.ratioThreshold，視為電腦截圖/一般照片 -> 不切片
        const isLongMobile = ratio > config.ratioThreshold;
        const templateHeightCm = isLongMobile ? 17 : 9;
        const baseName = file.name.replace(/\.[^/.]+$/, "");

        if (isLongMobile) {
          // 切片模式
          // 設定切片高度：約為原始寬度的 1.6 倍 (保持視覺比例)，保留緩衝區
          // 注意：這裡的計算基於「原始圖片」尺寸
          const sliceHeightSource = Math.floor(w * 1.6);
          const overlapSource = config.overlapSource; // 讀取自參數設定
          let y = 0;
          let index = 1;

          while (y < h) {
            // 計算原始圖片中的切片高度
            let currentSliceHeightSource = sliceHeightSource;
            if (y + currentSliceHeightSource > h)
              currentSliceHeightSource = h - y;

            // 計算目標輸出尺寸 (依據縮放比例)
            const targetSliceWidth = targetWidth; // 寬度固定為 targetWidth
            const targetSliceHeight = Math.floor(
              currentSliceHeightSource * scale
            );

            // 進行裁切與縮放 (加入銳化參數)
            const blob = await cropImage(img, 0, y, w, currentSliceHeightSource, targetSliceWidth, targetSliceHeight, performSharpen);
            
            results.push({
              name: `${baseName}_${String(index).padStart(2, "0")}.jpg`,
              blob: blob,
              width: targetSliceWidth,
              height: targetSliceHeight,
              templateHeight: templateHeightCm,
            });

            if (y + currentSliceHeightSource >= h) break;
            y += sliceHeightSource - overlapSource;
            index++;
          }
        } else {
          // 一般模式：不需要切片，但仍需縮放
          const targetHeight = Math.floor(h * scale);

          const blob = await cropImage(img, 0, 0, w, h, targetWidth, targetHeight, performSharpen);
          results.push({
            name: `${baseName}.jpg`,
            blob: blob,
            width: targetWidth,
            height: targetHeight,
            templateHeight: templateHeightCm,
          });
        }
        resolve(results);
      };
      img.onerror = () => reject(new Error("圖片載入失敗"));
      img.src = URL.createObjectURL(srcBlob);
    });
  }

  // 裁切並縮放圖片輔助函式 (含銳化)
  // 参数: (影像物件, 來源X, 來源Y, 來源寬, 來源高, 目標寬, 目標高, 是否銳化)
  function cropImage(img, sx, sy, sw, sh, dw, dh, doSharpen) {
    return new Promise(resolve => {
      const canvas = document.createElement('canvas');
      // 設定 Canvas 為目標尺寸 (縮放後的尺寸)
      canvas.width = dw;
      canvas.height = dh;
      const ctx = canvas.getContext('2d');

      // 啟用平滑縮放演算法
      ctx.imageSmoothingEnabled = true;
      ctx.imageSmoothingQuality = 'high';

      // drawImage(img, sx, sy, sw, sh, dx, dy, dw, dh)
      ctx.drawImage(img, sx, sy, sw, sh, 0, 0, dw, dh);

      // 如果需要銳化
      if (doSharpen) {
        sharpenCanvas(ctx, dw, dh, config.sharpenMix); // 使用自定義強度
      }

      // 輸出高品質 JPEG (讀取自參數設定)
      canvas.toBlob(resolve, 'image/jpeg', config.outputQuality);
    });
  }

  // 影像銳化濾鏡 (Convolution Filter)
  function sharpenCanvas(ctx, w, h, mix) {
    const imageData = ctx.getImageData(0, 0, w, h);
    const data = imageData.data;
    const width = w;
    const height = h;

    // 銳化卷積核心 (Sharpen Kernel)
    // [ 0, -1,  0]
    // [-1,  5, -1]
    // [ 0, -1,  0]
    const kernel = [
      0, -1, 0,
      -1, 5, -1,
      0, -1, 0
    ];

    // 建立輸出緩衝區 (避免修改來源影響計算)
    // 為了效能，我們直接操作 buffer，但卷積需要原始鄰居數據，所以複製一份 input
    const inputData = new Uint8ClampedArray(data);

    const cat = function(val) {
      return val < 0 ? 0 : (val > 255 ? 255 : val);
    };

    for (let y = 0; y < height; y++) {
      for (let x = 0; x < width; x++) {
        const idx = (y * width + x) * 4;

        // 處理邊緣像素：忽略或直接複製原值 (這裡選擇忽略計算，保留原值)
        if (x === 0 || y === 0 || x === width - 1 || y === height - 1) {
          continue; // 保持原值
        }

        let r = 0,
          g = 0,
          b = 0;

        // 卷積運算 3x3
        // kx, ky 相對座標
        // 核心索引 kIdx = (ky + 1) * 3 + (kx + 1)
        for (let ky = -1; ky <= 1; ky++) {
          for (let kx = -1; kx <= 1; kx++) {
            const pixelIdx = ((y + ky) * width + (x + kx)) * 4;
            const kWeight = kernel[(ky + 1) * 3 + (kx + 1)];

            r += inputData[pixelIdx] * kWeight;
            g += inputData[pixelIdx + 1] * kWeight;
            b += inputData[pixelIdx + 2] * kWeight;
          }
        }

        // 混合原始圖像與銳化結果 (根據 mix 強度)
        // Result = Original * (1 - mix) + Sharpened * mix
        // 但這裡我們直接套用 kernel 後通常值會變大，上面 kernel (sum=1) 已經保持亮度不變
        // 我們可以簡單地寫回計算值

        // 上述 Kernel [-1, 5, -1] 是強銳化。
        // 如果覺得太強，可以調整 Kernel 或與原圖混合。
        // 這裡採用與原圖混合的方式來控制強度

        const originalR = inputData[idx];
        const originalG = inputData[idx + 1];
        const originalB = inputData[idx + 2];

        data[idx] = cat(originalR * (1 - mix) + r * mix);
        data[idx + 1] = cat(originalG * (1 - mix) + g * mix);
        data[idx + 2] = cat(originalB * (1 - mix) + b * mix);
        // Alpha channel 不變
      }
    }

    ctx.putImageData(imageData, 0, 0);
  }

  function showDownloadButton() {
    const btnGroup = document.querySelector(".btn-group");
    if (document.getElementById("btnDownloadZip")) return;

    const btn = document.createElement("button");
    btn.id = "btnDownloadZip";
    // 使用與 HTML 定義一致的 class
    btn.className =
      "btn btn-success bg-success text-white hover:bg-[#219150] border-none py-3 px-5 rounded-xl text-[15px] font-semibold cursor-pointer transition-all duration-200 flex-1 flex items-center justify-center gap-2";
    btn.innerHTML =
      '<span><i class="fa-solid fa-file-zipper"></i> 下載圖片 (ZIP)</span>';
    btn.onclick = downloadZip;
    btnGroup.appendChild(btn);
  }

  async function downloadZip() {
    if (processedImages.length === 0) {
      alert("錯誤：沒有可下載的圖片！請重新轉換。");
      return;
    }

    if (typeof JSZip === "undefined") {
      alert("錯誤：JSZip 函式庫未載入，請檢查網路連線。");
      return;
    }

    try {
      const originalStatus = statusText.innerText;
      statusText.innerText = "⏳ 正在壓縮打包圖片，請稍候...";

      const zip = new JSZip();

      // 加入圖片至 ZIP (扁平化結構，直接存於根目錄)
      for (let i = 0; i < processedImages.length; i++) {
        const item = processedImages[i];
        if (!item.blob) {
          continue;
        }
        zip.file(item.name, item.blob);
      }

      // 生成 ZIP
      const content = await zip.generateAsync({ type: "blob" });
      const timestamp = new Date()
        .toISOString()
        .replace(/[-:T]/g, "")
        .slice(0, 12);

      if (typeof saveAs === "undefined") {
        // 若 FileSaver 未載入的備案
        const url = URL.createObjectURL(content);
        const a = document.createElement("a");
        a.href = url;
        a.download = `警務照片_${timestamp}.zip`;
        a.click();
        URL.revokeObjectURL(url);
      } else {
        saveAs(content, `警務照片_${timestamp}.zip`);
      }

      statusText.innerText = "✅ 下載已開始！";
      setTimeout(() => {
        statusText.innerText = originalStatus;
      }, 3000);
    } catch (e) {
      console.error("ZIP 生成失敗:", e);
      alert("下載失敗：" + e.message);
      statusText.innerText = "❌ 下載失敗";
    }
  }


  // --- 設定面板互動邏輯 (精細化說明與側邊抽屜控制) ---
  function syncConfigToUI() {
    const inputs = {
      'cfg-overlap': 'overlapSource',
      'cfg-threshold': 'ratioThreshold',
      'cfg-a4width': 'a4WidthCm',
      'cfg-sharpen': 'sharpenMix',
      'cfg-quality': 'outputQuality'
    };

    for (let id in inputs) {
      const el = document.getElementById(id);
      const display = document.getElementById(id + '-val');
      const configKey = inputs[id];
      const currentVal = config[configKey];

      if (el) {
        el.value = currentVal;
        // 初始化時同步顯示數值
        if (display) display.innerText = currentVal;

        // 監聽變更
        el.oninput = (e) => {
          let val = parseFloat(e.target.value);
          if (id === 'cfg-overlap') val = parseInt(e.target.value);
          config[configKey] = val;
          saveConfig();
          // 更新數值顯示器
          if (display) display.innerText = val;
        };
      }
    }

    // 初始化 DPI 按鈕狀態
    updateDpiButtonsUI(config.dpi);

    // --- 側邊抽屜控制邏輯 ---
    const btnToggle = document.getElementById('btnToggleConfig');
    const btnClose = document.getElementById('btnCloseConfig');
    const panel = document.getElementById('configPanel');
    const overlay = document.getElementById('configOverlay');

    const toggleDrawer = (isOpen) => {
      if (isOpen) {
        overlay.classList.remove('hidden');
        // 強制瀏覽器重繪以觸發動畫
        void overlay.offsetWidth;
        overlay.classList.add('opacity-100');
        panel.setAttribute('data-open', 'true');
        document.body.style.overflow = 'hidden'; // 禁止背景捲動
      } else {
        overlay.classList.remove('opacity-100');
        panel.removeAttribute('data-open');
        document.body.style.overflow = ''; // 恢復捲動
        // 等待動畫結束後隱藏遮罩
        setTimeout(() => {
          if (!panel.hasAttribute('data-open')) {
            overlay.classList.add('hidden');
          }
        }, 300);
      }
    };

    if (btnToggle) {
      btnToggle.onclick = (e) => {
        e.preventDefault();
        toggleDrawer(true);
      };
    }

    if (btnClose) {
      btnClose.onclick = () => toggleDrawer(false);
    }

    if (overlay) {
      overlay.onclick = () => toggleDrawer(false);
    }

    // 恢復預設值
    const btnReset = document.getElementById('btnResetConfig');
    if (btnReset) {
      btnReset.onclick = () => {
        if (confirm("確定要恢復所有進階設定為預設值嗎？")) {
          config = { ...DEFAULT_CONFIG };
          saveConfig();
          // 重新載入 UI 並保持抽屜開啟
          syncConfigToUI();
          // 注意：syncConfigToUI 會重新綁定事件，這裡可以直接結束
        }
      };
    }
  }

  return { init };
})();

document.addEventListener("DOMContentLoaded", app.init);
