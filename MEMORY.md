# Police-Image-Toolkit 專案記憶庫 (MEMORY.md)

本檔記錄專案重大技術決策、踩坑歷史、核心架構與維護要點，供 Codex 與 Antigravity Agent 接手時快速理解。

---

## 📌 1. 專案定位與架構演進

- **專案名稱**：警務手機影像轉檔與逐格截圖系統 (`Police-Image-Toolkit`)
- **現行架構**：C# 13 / .NET 8 LTS / WPF (MVVM 架構)
- **歷史封存**：原純前端網頁版（v10.0）已封存於 `legacy_web/` 目錄，僅作功能對照與歷史備援。
- **發行模式**：Self-Contained Single-File（單檔免安裝，大小約 69MB，路徑為 `dist/PoliceImageToolkit.exe`），零外部依賴、秒開、100% 離線本機處理。

---

## 🏗️ 2. 核心模組與目錄結構

```text
03_Police-Image-Toolkit/
├── legacy_web/                   # 📦【歷史隔離封存】原純前端網頁版本
├── src/
│   └── PoliceImageToolkit/       # 🚀【C# .NET 8 WPF 核心】
│       ├── PoliceImageToolkit.csproj
│       ├── app.ico / app_icon.png# 專屬警務鑑識圖示 (16~256px 多層級)
│       ├── App.xaml / App.xaml.cs# 全域資源、主題配色與 crash.log 全域例外攔截
│       ├── MainWindow.xaml / .cs # 主導航列與全域鍵盤快捷鍵分發 (PreviewKeyDown)
│       ├── Models/
│       │   ├── ImageTaskItem.cs  # 圖片轉檔任務模型
│       │   └── VideoSnapshotConfig.cs # 快照設定模型 (案號、格式、浮水印開關)
│       ├── Services/
│       │   ├── IImageService.cs / ImageService.cs # 多核心轉檔與 Exif 轉向校正
│       │   └── IVideoService.cs / VideoService.cs # 影格擷取、縮圖生成與純淨輸出
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── ImageConverterViewModel.cs
│       │   └── VideoSnapshotViewModel.cs
│       ├── Views/
│       │   ├── ImageConverterView.xaml / .cs # 圖片轉檔介面
│       │   └── VideoSnapshotView.xaml / .cs  # 自適應手機外框鑑識工作台
│       └── ValueConverters.cs
├── dist/                         # 發布目錄 (僅 PoliceImageToolkit.exe 單檔)
├── scripts/
│   ├── build.ps1                 # 一鍵發布腳本 (清理快取、無 pdb、通知 Shell 更新圖示)
│   └── qa.ps1                    # 自動化 QA 與編譯檢核腳本
├── README.md
├── CHANGELOG.md
└── MEMORY.md
```

---

## ⚡ 3. 核心功能與快捷鍵設計

### 3.1 圖片轉檔引擎 (ImageConverter)
- 原生多核心平行處理管線。
- 支援格式：iPhone HEIC/HEIF、Android WebP、PNG、JPG、BMP、TIFF 互轉。
- 支援 Exif Orientation 旋轉校正與等比例縮放控制。

### 3.2 手機影片逐格截圖工作台 (VideoSnapshot)
- **自適應大畫面手機圖框**：以 `寬 360px × 高 580px` 舒適比例呈現直向對話錄影，影片完美貼齊手機內螢幕。
- **🔄 上頁末行對照視窗 (Anti-Leak Preview)**：即時顯示前一張成功截圖與毫秒時間戳，方便滾動錄影時比對有無漏截或重複。
- **⚡ 全域鍵盤快捷鍵系統**（焦點在 TextBox 時自動旁路）：
  | 按鍵 | 功能 | 說明 |
  | :--- | :--- | :--- |
  | `Space`（空白鍵） | **秒截證物** | 附帶快門白光閃爍回饋 |
  | `←` / `→` 或 `A` / `D` | **微調 0.1 秒** | 逐格精準對齊 |
  | `↑` / `↓` 或 `W` / `S` | **快進 / 快退 1.0 秒** | 快速跳轉 |
  | `Z` | **復原 / 刪除上一頁** | 自動自硬碟刪除圖檔並還原對照圖 |
  | `P` 或 `Enter` | **播放 / 暫停** | 切換播放狀態 |
- **零阻塞極速連續操作**：截圖採非同步記憶體捕捉（`RenderTargetBitmap`）與背景 Task 存檔，截圖瞬間不鎖定 UI，可連按移動並連續秒截。
- **純淨輸出**：預設取消畫面內部深色時間戳浮水印條帶（100% 原始畫面無遮擋），輸出檔名自動帶毫秒時間戳與自訂案號（例：`A1130825_SNAP_00-00-12_333.png`）。

---

### 3.3 長截圖分頁輔助 (LongScreenshotSplit)
- **業務目的**：協助外勤與鑑識同仁將超長手機對話長截圖，依 Word/Photo-Report-Generator 報表圖框（預設 `8 cm × 17.5 cm`、重疊 `5 mm`）精準切成連續分頁。
- **原始像素不失真原則**：
  - 嚴格採用**原始像素裁切**，不縮放、不重取樣、不在輸出圖上烙印遮罩或頁碼。
  - 每頁裁切高度以 `來源像素寬度 × 圖框高 ÷ 圖框寬` 計算；重疊像素以 `來源像素寬度 × 重疊實體高度 ÷ 圖框寬` 計算。
- **手機框預覽與導航**：
  - 支援單一長截圖拖放（拒絕多檔與資料夾）。
  - 滑鼠拖曳、滾輪、鍵盤 `↑`/`↓`、`PageUp`/`PageDown`、`Home`/`End` 檢視全圖。
  - 預覽中以黃色斜線遮罩標示前後頁重疊區域（僅供檢視，不輸出至檔案）。
- **原圖低解析度警示**：
  - 當長截圖來源像素寬度低於 `600px` 時，介面以黃色常駐警示提醒文字放進 Word 可能模糊，並提示改用原始檔傳送（如 Telegram 請選「以檔案傳送」避免壓縮）。

---

## 🚨 4. 關鍵踩坑與防禦規則 (Agent 注意事項)

1. **WPF 點陣圖跨執行緒存取例外 (The calling thread cannot access this object...)**：
   - 在背景工作 (`Task.Run`) 裁切 `BitmapSource` 時，若點陣圖是在 UI 執行緒建立且未凍結，會引發跨執行緒存取例外。
   - **處方**：在傳入背景前，使用 `BitmapCacheOption.OnLoad` 完整載入，並明確呼叫 `.Freeze()` 後，才進背景 Task 執行 `CroppedBitmap` 與編碼存檔。
2. **.NET SDK 本機路徑陷阱**：
   - 系統 `PATH` 中的 `dotnet` 僅有 Runtime，完整的 .NET 8.0.406 SDK 位於：
     `C:\Users\chia-hao\AppData\Local\Microsoft\dotnet\dotnet.exe`
   - `build.ps1` 與 `qa.ps1` 已實作優先偵測該路徑，執行指令時請一律透過腳本或指定該 SDK 路徑。
3. **WPF XAML 靜態資源宣告順序 (StaticResource Crash)**：
   - XAML 中 `StaticResource` 是由上而下解析，**嚴禁**將 `<Window.Resources>` 或 `<UserControl.Resources>` 放置在引用該資源的控制項下方，否則將導致 `XamlParseException (找不到資源)` 崩潰。
4. **單檔發布與進程鎖定**：
   - 發布前必須先透過腳本關閉既有 `PoliceImageToolkit` 進程，避免 `GenerateBundle` 因檔案被佔用拋出 `IOException`。
5. **單檔乾淨度與雙主線禁止**：
   - 發布設定已包含 `<DebugType>none</DebugType>` 與 `<DebugSymbols>false</DebugSymbols>`，確保 `dist/` 目錄只有單一 `PoliceImageToolkit.exe`。
   - 專案已全面統一為 C# WPF，不另行維護 Python 雙主線版本。

