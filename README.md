# 🚔 Police Image Converter (警務專用圖片處理助手)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Version](https://img.shields.io/badge/Version-v3.1-green)
![Features](https://img.shields.io/badge/Feature-Auto%20Resize-blue)
![Features](https://img.shields.io/badge/Feature-DPI%20Selection-orange)

這是一個專為警務行政工作設計的**離線版**圖片處理工具。
它不僅能轉換 **HEIC/PNG/WebP** 格式，還能自動將圖片**縮放至符合 Word/Excel 表格的最佳尺寸**，大幅簡化後續製作筆錄的工作。

---

## ✨ v3.1 重大更新

* **🖨️ 自訂 DPI (DPI Selection)**：新增 DPI 選單，可依需求選擇 **96 (螢幕用)**、**150 (一般文件)**、**220 (標準列印)** 或 **300 (高畫質)**。
* **📏 智慧縮放**：以 **20公分** 為基準自動計算最佳像素，確保插入 Word/Excel 時不會過大或模糊。
* **⚡ 統一轉換流程**：不再區分照片或截圖模式，系統自動判斷並進行最佳化處理。
* **🎨 介面優化**：全新的現代化 UI，支援拖曳上傳與進度條顯示。
* **🛠️ 程式碼重構**：將 HTML/CSS/JS 分離，提升維護性與效能。

## 🌟 核心特色

* **🔒 絕對資安**：全本機 (Client-side) 運算，照片**絕不**上傳伺服器，完全符合資安規定。
* **📱 廣泛支援**：解決 iPhone HEIC 無法讀取問題，並修正 PNG 透明背景。
* **📦 一鍵打包**：處理完成後，自動將所有圖片打包成 ZIP 下載，檔名自動編號防重複。

## 🛠️ 使用教學

1. **選擇畫質**：依據用途選擇合適的 DPI (預設為 220 DPI，適合大多數列印需求)。
2. **上傳圖片**：點擊框框或將照片拖曳至網頁中 (支援多選)。
3. **開始轉換**：點擊「🚀 開始轉換」按鈕。
4. **下載檔案**：等待進度條跑完，系統會自動下載 ZIP 壓縮檔。
5. **直接使用**：解壓縮後的圖片已是標準 JPG 格式且尺寸適中，可直接插入文書軟體。

## 🔧 技術規格

* **基準尺寸**：長邊約 20cm (依 DPI 自動換算像素)
* **輸出格式**：標準 JPEG (品質 92%, 白底去背)
* **支援輸入**：JPG, HEIC, PNG, BMP, WebP

---

## 📄 License

[MIT License](LICENSE)
