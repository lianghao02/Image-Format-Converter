# 🖼️ 警務影像轉檔與銳化工具箱 Police-Image-Toolkit (v10.0.2)

[![Version](https://img.shields.io/badge/version-v10.0.2-blue.svg)](https://github.com/lianghao02/Police-Image-Toolkit)
[![Canvas](https://img.shields.io/badge/Core-HTML5%20Canvas-orange.svg)](https://w3.org)

## 下載、依賴與執行

- **安裝**：不需 Python 或 Node.js；下載 ZIP、解壓後以新版 Chrome、Edge 或 Firefox 開啟 `index.html`。
- **功能**：HEIC 轉檔、PDF 頁面輸出、影片快照、影像銳化與掃描線處理；處理在瀏覽器本機執行。
- **外部依賴**：Tailwind CSS、Google Fonts、Font Awesome、heic2any、JSZip、FileSaver 與 PDF.js 由 CDN 載入，因此首次載入及未快取時需要網路。
- **打包／部署**：本專案是靜態網站，不需建置；完整部署 Repository 內容即可。若要完全離線，須另將上述第三方函式庫合法下載並改成本機引用。
- **證物注意**：輸出是衍生檔，應保留原始檔及操作紀錄，不能以濾鏡結果取代原始證物。

## 🏆 v10.0 里程碑：HEIC/PDF 高清轉檔與警務銳化濾鏡

## 📖 重大更新摘要 (Summary)

本版本整合 HEIC 轉檔、PDF 頁面輸出、影片快照與影像強化濾鏡，主要處理流程在瀏覽器本機完成。

工具透過 Canvas 與瀏覽器解碼能力輸出可供人工檢視的影像。濾鏡會改變像素，處理後檔案不可取代原始證物；正式使用時應同時保存原檔、處理參數與輸出檔。

## 🛡️ v10.0.1 穩定性補強

- 影片載入逾時、解碼失敗、seek 無回應與快照例外後會解除操作鎖。
- 圖片 Object URL 與暫用 Canvas 在成功或失敗後釋放。
- PDF 反向頁碼範圍會正規化並限制於實際頁數。
- 掃描橫紋濾除增加紅色印章與連續文字筆畫保護。

## ✨ 重點更新特色

- 🔄 **HEIC / PDF 免費本機轉檔 (Browser Local Conversion)**：
  - 轉檔與切片在本機瀏覽器完成，程式本身不會主動將使用者檔案上傳至專案伺服器。
  - 第三方程式庫由 CDN 載入；正式環境仍應依機關網路與資安規範使用。

- 🔍 **車牌人臉二值化與 Unsharp 銳化引擎 (Image Sharpening Filter)**：
  - 提供拉普拉斯 (Laplacian) 矩陣與動態二值化對比度調校濾鏡。
  - 精準拋出原本模糊不清的車牌號碼與監視器畫面細節。
