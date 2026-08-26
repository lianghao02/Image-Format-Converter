# 🛡️ 警務手機影像轉檔與逐格截圖系統 Police-Image-Toolkit (v11.2.0)

[![Version](https://img.shields.io/badge/version-v11.2.0-blue.svg)](https://github.com/lianghao02/Police-Image-Toolkit)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20LTS-purple.svg)](https://dotnet.microsoft.com)

專為第一線外勤與警務同仁設計之 Windows 原生免安裝影像處理工具箱。
徹底擺脫瀏覽器記憶體限制，專注於**手機圖片高速批次轉檔**、**手機影片精準逐格截圖**與**長截圖分頁輔助**。

## 技術架構現況（2026-08-26）

主力版本已全面完成由 **HTML／JavaScript → C#／.NET 8 LTS／WPF (MVVM)** 遷移。
- **現行原生主線**：原始碼位於 `src/PoliceImageToolkit/`，發行以單檔免安裝 `dist/PoliceImageToolkit.exe` 為唯一基準。
- **純淨單一主線**：歷史版本已封存於 Git 提交歷史；專案無 Python 原始碼或執行依賴，不維護雙主線。

---

## ⚡ 核心功能特色

### 1. 📁 手機圖片高速批次轉檔 (Image Batch Converter)
- **格式通吃**：支援 iPhone HEIC/HEIF、Android WebP、PNG、JPG、BMP、TIFF 等主流手機格式互轉。
- **多核心平行處理**：內建 CPU 多執行緒管線，上百張照片拖曳即轉，極速不卡頓。
- **Exif 方向自動校正**：自動辨識手機拍攝方向中繼資料，修正旋轉角度，轉出正常視角圖檔。
- **品質與大小彈性調整**：可自訂 JPG 壓縮品質與等比例縮放限制。

### 2. 🎬 手機影片精準逐格截圖 (Video Frame Snapshot)
- **自適應大畫面手機圖框**：專為手機側錄與 LINE / 簡訊對話滾動錄影打造，畫面大而清晰。
- **🔄 上頁末行對照視窗 (Anti-Leak Preview)**：即時預覽前一張截圖與時間戳，滾動比對防漏行或重複。
- **⚡ 全域鍵盤快捷鍵系統**（文字輸入時自動旁路）：
  - `Space`（空白鍵）：**秒截證物**（附帶快門白光閃爍回饋）。
  - `←` / `→` 或 `A` / `D`：**微調 0.1 秒**（逐格精準對齊）。
  - `↑` / `↓` 或 `W` / `S`：**快進 / 快退 1.0 秒**。
  - `Z` 鍵：**復原 / 刪除上一頁截圖**。
  - `P` 或 `Enter`：**播放 / 暫停**。
- **🚀 零阻塞極速連續操作**：非同步記憶體捕捉與背景存檔，截圖瞬間不鎖定 UI，可連按移動並連續秒截。
- **純淨原始輸出**：預設取消畫面內部遮擋條帶，輸出 100% 原始畫面，檔名自帶毫秒時間戳與自訂案號。

### 3. 📜 手機長截圖分頁輔助 (Long Screenshot Splitter)
- **Word / 報表圖框比例切分**：自動依照 Photo-Report-Generator 或 Word 圖框實體比例（預設 `8 cm × 17.5 cm`、重疊 `5 mm`）精準計算最佳裁切頁數。
- **前後頁重疊遮罩防漏字**：相鄰頁面保留固定重疊像素，避免關鍵字句恰好落在分頁切線上；預覽中以黃色斜線遮罩標示（僅供預覽，不輸出至圖檔）。
- **原始像素零失真裁切**：堅持原始點陣裁切，絕不重新取樣或縮放，維持原始長截圖字體清晰度。
- **低解析度常駐警示**：來源長截圖寬度低於 `600px` 時即時提示放進 Word 可能模糊，引導使用者索取原始檔案（如 Telegram 請以「檔案」傳送而非壓縮相片）。
- **手機框預覽與導航**：支援單一圖檔拖曳載入，可用滑鼠滾輪、鍵盤 `↑`/`↓`、`PageUp`/`PageDown`、`Home`/`End` 快速檢視全圖。

---

## 🚀 下載、依賴與執行

- **免安裝單檔執行**：直接下載或發布產出之 `dist/PoliceImageToolkit.exe` 即可點擊開啟（0.1 秒秒開）。
- **零 .NET Runtime 依賴**：採 Self-Contained 獨立封裝，目標電腦不需預先安裝 .NET Runtime。
- **100% 離線本機處理**：所有影音與圖片解碼運算均在電腦本機端完成，不連外網、無資料外洩疑慮。
- **證物注意事項**：輸出圖檔為衍生檔案，正式司法程序請妥善保存原始手機影音檔與操作紀錄。

---

## 🛠️ 開發與建置指令

### 專案結構
```text
03_Police-Image-Toolkit/
├── src/
│   └── PoliceImageToolkit/       # 🚀【C# .NET 8 WPF 原生桌面專案】
│       ├── app.ico / app_icon.png# 專屬警務鑑識圖示
│       ├── Models/               # 轉檔、影片快照與長截圖分頁模型
│       ├── Services/             # 轉檔、影片截圖與長截圖裁切核心引擎
│       ├── ViewModels/           # MVVM 邏輯
│       └── Views/                # 現代化 Fluent UI 介面
└── scripts/
    ├── build.ps1                 # 一鍵發布單檔 Exe (含快取清理、無 pdb、圖示通知)
    └── qa.ps1                    # QA 檢核與建置測試腳本
```


### 發布單檔 Exe
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

### 執行 QA 檢核
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\qa.ps1
```
