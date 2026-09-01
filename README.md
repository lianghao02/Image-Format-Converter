# 🛡️ 警務手機影像轉檔與逐格截圖系統 Police-Image-Toolkit (v11.3.1)

[![Version](https://img.shields.io/badge/version-v11.3.1-blue.svg)](https://github.com/lianghao02/Police-Image-Toolkit/releases)
[![Platform](https://img.shields.io/badge/Platform-Windows%20x64-0078D6.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0%20LTS-purple.svg)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

專為第一線外勤與警務鑑識同仁打造之 **Windows 原生免安裝影像處理工具箱**。
徹底擺脫瀏覽器記憶體限制與資安疑慮，專注於**手機圖片高速批次轉檔**、**手機影片精準逐格截圖**與**長截圖分頁輔助**。

---

## 🚀 下載與快速開始 (Download & Quick Start)

### 📥 取得程式（請選擇其一下載）
| 下載項目 | 格式 | 說明 | 連結 |
|:---|:---:|:---|:---:|
| **PoliceImageToolkit.exe** | `.exe` | **推薦**。免安裝單一執行檔，下載後直接雙擊開啟 | [⬇️ 點此直接下載（最新版）](https://github.com/lianghao02/Police-Image-Toolkit/releases/latest/download/PoliceImageToolkit.exe) |
| **PoliceImageToolkit-v11.3.1-win-x64.zip** | `.zip` | 壓縮包（適用於被瀏覽器或端點限制直接下載 exe 之環境） | [📦 點此下載 ZIP 壓縮檔](https://github.com/lianghao02/Police-Image-Toolkit/releases/latest/download/PoliceImageToolkit-v11.3.1-win-x64.zip) |

> 🔗 想下載歷史版本或查看詳細更新紀錄，請至 [GitHub Releases 頁面](https://github.com/lianghao02/Police-Image-Toolkit/releases)。

### 💻 系統需求
* **作業系統**：Windows 10 / 11 (64-bit)
* **執行環境**：**完全免安裝、零相依性**（已內建獨立 Runtime，電腦無需預先安裝 .NET）
* **網路狀態**：預設完全離線；只有使用者主動按下「檢查更新」並確認後，才會連線至 GitHub Release 查詢版本資訊。所有影像與影片處理均在本機完成，不會上傳檔案。

### 💡 安裝與初次執行說明
1. **免安裝**：下載 `PoliceImageToolkit.exe` 後，放置於任意資料夾（如桌面或隨身碟）即可。
2. **雙擊即開**：直接雙擊 `PoliceImageToolkit.exe` 開啟工具。
3. **若出現 Windows SmartScreen 防護提示**：
   * 由於本程式為內部獨立編譯開源軟體，未購買微軟商業數位憑證，Windows 可能跳出「*Windows 已保護您的電腦*」藍色提示。
   * 請點擊 **「其他資訊」** ➜ 再點擊 **「仍要執行」** 即可正常啟動。
4. **驗證發布檔**：`dist\SHA256SUMS.txt` 與 `使用說明.txt` 會隨本機發布產生，可用 `Get-FileHash .\PoliceImageToolkit.exe -Algorithm SHA256` 比對雜湊值。

---

## ⚡ 核心功能特色

### 1. 📁 手機圖片高速批次轉檔 (Image Batch Converter)
專門解決外勤人員收到民眾 iPhone (HEIC) 或 Android (WebP) 照片無法在公務電腦開啟的問題。

* **支援格式**：iPhone `HEIC`/`HEIF`、Android `WebP`、`PNG`、`JPG`、`BMP`、`TIFF`。
* **操作方式**：
  1. 將照片批次**拖曳**進主視窗。
  2. 選擇目標輸出格式（預設轉為相容性最高之 `JPG`）。
  3. 設定壓縮品質（預設 90%）與是否限制最大寬高（預設等比例原尺寸）。
  4. 點擊 **「開始批次轉檔」**，多核心平行處理，百張相片瞬間完成。
* **智慧方向校正**：自動辨識手機拍攝 Exif 中繼資料，修正旋轉角度，轉出正常視角圖檔。

---

### 2. 🎬 手機影片精準逐格截圖 (Video Frame Snapshot)
專為翻拍監視器、密錄器、手機對話紀錄（LINE / 簡訊）滾動錄影打造之逐格證物截圖工具。

* **自適應大畫面手機圖框**：專為手機側錄與對話錄影打造，畫面大而清晰。
* **🔄 上頁末行對照視窗 (Anti-Leak Preview)**：即時顯示前一張截圖畫面與時間戳，比對對話紀錄絕不漏行或重複。
* **⚡ 全域極速鍵盤快捷鍵**（支援盲按秒截，輸入文字時自動旁路）：
  | 按鍵 | 動作說明 |
  |:---:|:---|
  | **`Space`（空白鍵）** | **秒截當前證物畫面**（伴隨快門白光閃爍視覺回饋，非同步零阻塞存檔） |
  | **`←` / `→`** 或 **`A` / `D`** | **微調 0.1 秒**（逐格精準對齊） |
  | **`↑` / `↓`** 或 **`W` / `S`** | **快進 / 快退 1.0 秒** |
  | **`Z`** | **復原 / 刪除上一張截圖**（防誤截快速清理） |
  | **`P`** 或 **`Enter`** | **播放 / 暫停影片** |
* **純淨原始輸出**：輸出 100% 原始解析度圖檔，檔名自動標註毫秒時間戳與自訂案號前綴。

---

### 3. 📜 手機長截圖分頁輔助 (Long Screenshot Splitter)
專為對話紀錄長截圖輸出至公務「偵查報告書」或 Word 報表圖框設計。

* **Word / 報表圖框精準切分**：依照報告書常用圖框實體比例（預設 `8 cm × 17.5 cm`、重疊 `5 mm`）自動計算最佳切分頁數。
* **前後頁重疊遮罩（防漏字）**：相鄰頁面保留固定重疊像素，避免關鍵字句恰好落在分頁裁切線上；預覽畫面以黃色虛線遮罩清晰標示。
* **原始像素 1:1 零失真**：堅持原始點陣精準裁切，不重新取樣或縮放，維持文字絕對清晰。
* **低解析度常駐警告**：原圖寬度低於 `600px` 時即時提示放進 Word 可能模糊，引導使用者向當事人索取原始長截圖（如 Telegram 請以「檔案」傳送而非壓縮相片）。

---

### 4. 🏷️ 版本徽章與 GitHub 自動檢查更新 (Auto Update Checker)
* **頂部版本徽章**：主介面頂端動態載入 `version.txt`，清楚標註當前版本號。
* **『🔄 檢查更新』按鈕**：點擊後非同步向 GitHub Release API 查詢最新發行資訊：
  * **已是最新版**：彈出成功提示與當前版本。
  * **發現新版**：彈出對話框展示新版本號與更新重點，點擊確認可直接開啟下載。
  * **離線防護**：公務內網或連線逾時時顯示友善提醒，不影響本機核心工具運作。

---

## 🔮 下一個版本規劃 (Roadmap)

預計於後續版本（`v11.4.0`）陸續推動之優化項目：
1. **Photo-Report-Generator 連動索引檔（`report_index.json` / CSV）**：截圖完成時自動於資料夾產出中繼檔，記載影片建立時間、頁碼序號與檔案對應，實現報告書製作自動化。
2. **影片手機框滑鼠滾輪步進微調（Mouse Wheel Scrubbing）**：游標懸停於手機螢幕時，滾輪向上/向下直接前後 0.1 秒步進，達成「右手滾輪定位、左手 Space 秒截」極速盲操。
3. **影片多倍速播放切換（1.0x / 1.5x / 2.0x）**：長影片快速瀏覽跳轉涉案段落。
4. **長截圖分頁切線上下微調（±20px）**：手動拖曳避開切割貼圖與關鍵轉帳單據本體。

---

## ❓ 常見問題 (FAQ)

#### Q1: 下載後需要安裝或設定任何執行環境嗎？
> **不需要。** 本程式採用 .NET 8 LTS Self-Contained 獨立單檔打包，已內建所有必備核心庫，目標電腦不需要安裝 .NET Runtime 或其他軟體，隨點隨開。

#### Q2: 為什麼 Windows 會提示「Windows 已保護您的電腦」？
> 這是微軟針對未購買昂貴企業憑證的開源軟體的標準 SmartScreen 提示。本專案原始碼完全公開開源，無任何惡意代碼，請安心點擊 **「其他資訊」 ➜ 「仍要執行」** 即可。

#### Q3: 影像與影音資料會不會被上傳到雲端？
> **不會。** 所有編解碼運算均在電腦本機執行，不會上傳影像、影片、案號或其他本機檔案。程式平時完全離線；只有使用者主動按下「檢查更新」並確認後，才會向 GitHub Release 查詢版本與更新說明。

#### Q4: 若特定 iPhone HEIC 或 WebP 圖檔無法解碼該怎麼辦？
> 本工具會呼叫 Windows 內建的 WIC 影像編解碼管線。若舊版 Windows 10 缺少相關 Codec，系統會跳出繁體中文友善提示，請至微軟市集安裝免費的「HEIF 影像延伸模組」或「WebP 影像延伸模組」即可。

---

## 🛠️ 開發者資訊與本機建置

### 專案結構
```text
03_Police-Image-Toolkit/
├── src/
│   └── PoliceImageToolkit/       # 🚀【C# .NET 8 WPF 原生桌面專案】
│       ├── app.ico / app_icon.png# 專屬警務鑑識圖示
│       ├── version.txt           # 當前版本宣告檔 (v11.3.1)
│       ├── Models/               # 轉檔、影片快照與長截圖分頁資料模型
│       ├── Services/             # 轉檔、影片截圖、長截圖裁切與更新檢查引擎
│       ├── ViewModels/           # MVVM 架構邏輯
│       └── Views/                # Fluent UI 風格 WPF 介面
├── dist/                         # 發行成品：EXE、version.txt、SHA256SUMS.txt、使用說明.txt
└── scripts/
    ├── build.ps1                 # 一鍵發布單檔 Exe (含快取清理、無 pdb、圖示快取重新整理)
    └── qa.ps1                    # QA 檢核與建置測試腳本
```

### 本機建置發布指令
```powershell
# 執行 QA 與語法建置檢核
powershell -ExecutionPolicy Bypass -File .\scripts\qa.ps1

# 發布單檔獨立免安裝 Exe (產出於 dist/PoliceImageToolkit.exe)
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

---

## 📄 授權條款 (License)

本專案採用 [MIT License](LICENSE) 授權開放使用。
