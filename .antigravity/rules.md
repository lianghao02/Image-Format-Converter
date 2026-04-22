# 專案特定細則：警務影像轉換器 (Police Image Converter Rules)

> [!IMPORTANT]
> 本專案嚴格遵循「全域開發憲法 v1.1」。由於涉及警務蒐證影像，**全本機離線運算 (Offline-First)** 是最高核心準則，嚴禁任何資料上傳至第三方伺服器。

## 1. 核心開發準則 (Technical Standards)
- **架構模式**：堅持單檔交付 (All-in-One HTML)，所有邏輯、樣式與資源必須封裝於 `index.html`。
- **配置管理**：所有核心參數（如 DPI、重疊像素、銳化強度）必須統一定義在 `CONFIG_MANAGER.DEFAULT_CONFIG` 中。
- **效能優化**：對於長截圖處理，必須使用 `async/await` 配合 `Promise` 處理 Blob 生成，避免阻塞瀏覽器主執行緒 (UI Thread)。

## 2. 動態配置與 UI 互動 (UI-Driven Pattern)
- **參數同步**：任何新增的圖像處理參數，必須同步實作於側邊進階設定面板 (`configPanel`)。
- **即時回饋**：UI 必須具備 `syncConfigToUI` 邏輯，確保 `localStorage` 的設定值與滑桿 (Range Input) 狀態即時對應。
- **視覺回饋**：轉換過程中必須使用進度條 (`progressBar`) 顯示當前處理張數與狀態文字。

## 3. 影像處理邏輯 (Image Processing Logic)
- **長截圖判定**：採用 `ratioThreshold` (預設 2.5) 作為分割判準，超過此比例之影像必須執行自動切片。
- **重疊裁切 (Overlap)**：為了蒐證文字完整性，切片起始點必須包含 `overlapSource` 像素之重疊區域，確保文字銜接不中斷。
- **畫質保證**：
  - **DPI 轉換**：依據輸出畫質設定，自動計算目標 A4 寬度像素。
  - **銳化濾鏡**：使用 3x3 卷積核 (Convolution Kernel) 進行文字邊緣強化，且混合強度需為可調參數。

## 4. 安全與隱私 (Privacy & Security)
- **外部資源**：僅限使用憲法認可之 CDN (如 cdnjs, jsdelivr)，嚴禁本地路徑依賴，確保檔案在任何環境皆可即時執行。
- **資料處理**：所有轉檔邏輯（HEIC2any, JSZip）必須在客戶端瀏覽器內完成，嚴禁任何 API 呼叫外傳資料。

## 5. UI/UX 與視覺設計
- **字體規範**：優先使用 `Noto Sans TC` 與系統字體，確保公務環境顯示穩定。
- **響應式佈局**：遵循 Mobile-First 規範，確保在行動載具上也能順暢操作進階設定面板。
- **語系鎖定**：所有介面文字、提示語、下載檔名命名規則，必須 100% 使用台灣繁體中文。