# 🛡️ 警務手機影像轉檔與逐格截圖系統 Police-Image-Toolkit (v11.0.0)

[![Version](https://img.shields.io/badge/version-v11.0.0-blue.svg)](https://github.com/lianghao02/Police-Image-Toolkit)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20LTS-purple.svg)](https://dotnet.microsoft.com)

專為第一線外勤與警務同仁設計之 Windows 原生免安裝影像處理工具箱。
徹底擺脫瀏覽器記憶體限制與付費編解碼器困擾，專注於**手機圖片高速批次轉檔**與**手機影片精準逐格截圖**。

## 技術架構現況（2026-08-24）

主力版已完成 **HTML／JavaScript → C#／.NET 8／WPF** 遷移。現行原生程式位於 `src/PoliceImageToolkit/`，舊版網頁工具封存於 `legacy_web/`，僅作功能比對與歷史備援；正式發行以 Self-Contained 單檔 `PoliceImageToolkit.exe` 為準。

---

## ⚡ 核心功能特色

### 1. 📁 手機圖片高速批次轉檔 (Image Batch Converter)
- **格式通吃**：支援 iPhone HEIC/HEIF、Android WebP、PNG、JPG、BMP、TIFF 等主流手機格式互轉。
- **多核心平行處理**：內建 CPU 多執行緒管線，上百張照片拖曳即轉，極速不卡頓。
- **Exif 方向自動校正**：自動辨識手機拍攝方向中繼資料，修正旋轉角度，轉出正常視角圖檔。
- **品質與大小彈性調整**：可自訂 JPG 壓縮品質與等比例縮放限制。

### 2. 🎬 手機影片精準逐格截圖 (Video Frame Snapshot)
- **解決 iPhone 格式地獄**：原生支援 iPhone 4K 60fps MOV (HEVC/H.264)、HDR 與 Android MP4 影片順暢播放與定位。
- **逐格精準定位 (Frame-by-frame Seek)**：支援毫秒級（~33ms）上一格 / 下一格步進微調，精確抓取關鍵動態瞬間。
- **時間戳記浮水印 (Timestamp Overlay)**：截圖時可自動於影像下方烙印高對比半透明時間戳記（`hh:mm:ss.fff`）、原始檔名與解析度。
- **自訂儲存分類**：自動依影片名稱建立專屬截圖子資料夾，檔名標註毫秒時間戳。

---

## 🚀 下載、依賴與執行

- **免安裝單檔執行**：直接下載或發布產出之 `dist/PoliceImageToolkit.exe` 即可點擊開啟（0.1 秒秒開）。
- **零環境依賴**：採 Self-Contained 獨立封裝，目標電腦**不需預先安裝 .NET Runtime 或其他軟體**。
- **100% 離線本機處理**：所有影音與圖片解碼運算均在電腦本機端完成，不連外網、無資料外洩疑慮。
- **證物注意事項**：輸出圖檔為衍生檔案，正式司法程序請妥善保存原始手機影音檔與操作紀錄。

---

## 🛠️ 開發與建置指令

### 專案結構
```text
03_Police-Image-Toolkit/
├── legacy_web/                   # 📦【歷史隔離封存】原純前端網頁版本
├── src/
│   └── PoliceImageToolkit/       # 🚀【C# .NET 8 WPF 原生桌面專案】
│       ├── Models/               # 資料模型
│       ├── Services/             # 轉檔與截圖核心引擎
│       ├── ViewModels/           # MVVM 邏輯
│       └── Views/                # 現代化 Fluent UI 介面
└── scripts/
    ├── build.ps1                 # 一鍵發布單檔 Exe
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
