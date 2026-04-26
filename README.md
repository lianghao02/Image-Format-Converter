# 🚔 警用影像格式轉換工具 (v3.2)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/版本-v3.2-green)
![Architecture](https://img.shields.io/badge/架構-單一檔案-blue)
![Offline](https://img.shields.io/badge/運算-全本機離線-orange)

提供安全、隱私的**本機端影像轉檔服務**，直接開啟 `index.html` 即可使用。無需安裝軟體，亦不需連線至雲端，確保公務影像資訊安全。

---

## 🚀 快速開始

1. **下載專案**：Clone 或下載此 Repository。
2. **開啟工具**：直接用瀏覽器（Chrome / Edge 建議）開啟 `index.html`。
3. **參數設定**：可透過右上角齒輪調整分割重疊度或輸出品質（預設已優化）。
4. **選取檔案**：點擊區域或將圖片拖曳進去（支援多選、影片截圖）。
5. **開始轉檔**：點擊「🚀 開始轉檔」。
6. **下載成果**：處理完成後，點擊「📦 下載圖片 (ZIP)」。

> 轉換後的圖片已針對行政報表自動化系統優化，可直接匯入使用。

---

## ✨ 功能特色

| 功能 | 說明 |
| --- | --- |
| **格式支援** | JPG, PNG, WebP, BMP, HEIC, MP4, MOV |
| **安全隱私** | 純本機運算，影像絕對不上傳伺服器 |
| **長截圖分割** | 自動偵測超長影像並執行智慧分割，避免內容被截斷 |
| **影片轉截圖** | 支援影片位移偵測，自動擷取動態畫面的關鍵影格 |
| **進階設定** | 側邊抽屜式設定面板，所有參數均可微調並自動儲存 |
| **單檔即用** | 無需複雜佈署，單一 HTML 檔案即具備完整功能 |

---

## 🛠️ 技術棧

| 分類 | 使用技術 |
| --- | --- |
| **UI 框架** | [Tailwind CSS](https://tailwindcss.com/) (CDN) |
| **圖示** | [Font Awesome 6](https://fontawesome.com/) (CDN) |
| **HEIC 解碼** | [heic2any](https://github.com/alexcorvi/heic2any) (CDN) |
| **ZIP 封裝** | [JSZip](https://stuk.github.io/jszip/) (CDN) |
| **檔案下載** | [FileSaver.js](https://github.com/eligrey/FileSaver.js/) (CDN) |
| **架構** | 單一 HTML 檔案交付，無需建置流程 |

---

## 📄 授權

[MIT License](LICENSE)
