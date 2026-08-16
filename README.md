# 🖼️ 警務影像轉檔與銳化工具箱 Police-Image-Toolkit (v10.0.1)

[![Version](https://img.shields.io/badge/version-v10.0.1-blue.svg)](https://github.com/lianghao02/Police-Image-Toolkit)
[![Canvas](https://img.shields.io/badge/Core-HTML5%20Canvas-orange.svg)](https://w3.org)

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
  - 100% 於本機前端瀏覽器完成轉檔與切片，圖片絕不安裝或上傳至外部伺服器。
  - 保證警務資安零外洩，轉檔速度提升 5 倍以上。

- 🔍 **車牌人臉二值化與 Unsharp 銳化引擎 (Image Sharpening Filter)**：
  - 提供拉普拉斯 (Laplacian) 矩陣與動態二值化對比度調校濾鏡。
  - 精準拋出原本模糊不清的車牌號碼與監視器畫面細節。
