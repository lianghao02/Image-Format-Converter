# 03_Police-Image-Toolkit Agent 開發規範

本專案遵循目前有效之全域開發憲法；本檔僅定義專案專屬規則與例外。

---

## 1. 技術棧與建置規範
- **主力架構**：C# 12 / .NET 8.0 WPF，保持原生秒開、獨立單檔發布（`PublishSingleFile=true`）與零環境依賴。
- **.NET SDK 本機環境**：`.NET 8.0.406 SDK` 位於 `%LOCALAPPDATA%\Microsoft\dotnet`。CLI 建置與測試需確保 `DOTNET_ROOT` 正確設定。
- **發布標準**：維持 `dist/PoliceImageToolkit.exe` 單檔發布與 `SHA256SUMS.txt` 雜湊驗證。

---

## 2. 業務領域與警務鑑識核心邊界
- **三大工具整合入口與獨立流程**：
  1. **圖片批次轉檔**：支援多種手機原生格式轉換為通用圖檔，支援 Exif 自動旋轉校正。
  2. **影片逐格截圖**：針對監視器、密錄器與手機對話滾動錄影；維護「上頁末行對照視窗（Anti-Leak Preview）」避免漏行，並提供免切換視窗之流暢盲截與微調操作。
  3. **長截圖分頁輔助**：依公務報告書圖框比例自動計算切分，相鄰頁面保留重疊遮罩防漏字，原圖寬度過低時主動提示解析度不足。
- **原始證物不可覆寫與可追溯性原則**：
  - **原始影像保護**：任何轉檔、裁切或截圖操作，**嚴禁覆寫原始相片或影音證物檔案**。
  - **鑑識可追溯性**：輸出檔維持不覆寫的流水號命名；各輸出資料夾以 `report_index.json` 對應輸出檔名、來源檔名、工具類型、影片時間與像素尺寸，供後續串接 `Photo-Report-Generator`。

---

## 3. 穩定性與資安防護
- **100% 離線機敏隱私保證**：
  - 所有影像與影片逐格處理 100% 於本機端點執行，**嚴禁將警務個資與現場證物影像上傳任何外部伺服器**。
  - 僅在使用者主動點擊「檢查更新」時連線 GitHub Release 讀取公開版本號，不傳輸任何本機業務資料。

---

## 4. 核心驗證方式
- 修改 Core 演算法、影像轉換或截圖模組後，必須執行單元測試：
  ```powershell
  $env:DOTNET_ROOT = "$env:LOCALAPPDATA\Microsoft\dotnet"
  $env:PATH = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:PATH"
  dotnet test tests\PoliceImageToolkit.CoreTests --no-restore --nologo
  ```
