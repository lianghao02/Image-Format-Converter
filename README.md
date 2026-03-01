# 🚔 警務專用影像轉換器 (v3.2)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/版本-v3.2-green)
![Architecture](https://img.shields.io/badge/架構-單一檔案-blue)
![Offline](https://img.shields.io/badge/運算-全本機離線-orange)

專為警務行政工作設計的**離線版圖片處理工具**，直接開啟 `index.html` 即可使用，無需安裝任何軟體，亦無需連線至伺服器。

---

## 🚀 快速開始

1. **下載專案**：Clone 或下載此 Repository。
2. **開啟工具**：直接用瀏覽器（Chrome / Edge 建議）開啟 `index.html`。
3. **選擇畫質**：依用途選擇 DPI（預設 220，適合大多數列印需求）。
4. **上傳圖片**：點擊上傳框或將圖片拖曳進去（支援多選）。
5. **開始轉換**：點擊「🚀 開始轉換」。
6. **下載成果**：等進度條跑完後，點擊「📦 下載圖片 (ZIP)」。

> 解壓縮後的 JPG 格式圖片，尺寸已針對 Word / Excel 表格插圖最佳化，可直接使用。

---

## ✨ 功能特色

| 功能 | 說明 |
|---|---|
| **格式支援** | JPG, PNG, WebP, BMP, HEIC (iPhone 原生格式) |
| **智慧縮放** | 根據 DPI 與 A4 標準寬度自動換算像素，確保插圖不失真 |
| **長截圖切片** | 自動偵測超長截圖並切割成多張，避免文字被截斷 |
| **自動銳化** | 可選的 3x3 卷積銳化濾鏡，強化文字清晰度 |
| **進階設定** | 側邊抽屜式設定面板，所有參數均可調整並自動儲存 |
| **絕對離線** | 純前端運算，照片絕不上傳伺服器 |

---

## 🛠️ 技術棧

| 分類 | 使用技術 |
|---|---|
| **UI 框架** | [Tailwind CSS](https://tailwindcss.com/) (CDN) |
| **圖示** | [Font Awesome 6](https://fontawesome.com/) (CDN) |
| **HEIC 解碼** | [heic2any](https://github.com/alexcorvi/heic2any) (CDN) |
| **ZIP 封裝** | [JSZip](https://stuk.github.io/jszip/) (CDN) |
| **檔案下載** | [FileSaver.js](https://github.com/eligrey/FileSaver.js/) (CDN) |
| **架構** | 單一 HTML 檔案交付，無需建置流程 |

---

## 📄 授權

[MIT License](LICENSE)
