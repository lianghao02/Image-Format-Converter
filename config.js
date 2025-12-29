/**
 * 影像格式轉換器 - 全域配置檔案
 * 包含預設參數、儲存金鑰與檔案類型定義
 */

const CONFIG_MANAGER = {
  // 本機儲存金鑰
  STORAGE_KEY: "IMAGE_CONVERTER_user_config",

  // 預設配置參數
  DEFAULT_CONFIG: {
    dpi: 220,               // 預設 DPI (220)
    overlapSource: 150,     // 分割重疊像素 (150px)
    ratioThreshold: 2.5,    // 長截圖判定比例 (2.5)
    a4WidthCm: 20,          // 目標 A4 寬度 (20cm)
    sharpenMix: 0.3,        // 銳化混合強度 (0.3)
    outputQuality: 0.95     // JPEG 輸出品質 (0.95)
  },

  // 支援的檔案類型說明
  SUPPORTED_TYPES: ["image/jpeg", "image/png", "image/webp", "image/heic", "image/bmp"],
  
  // 支援的副檔名 (用於過濾)
  SUPPORTED_EXTENSIONS: [".jpg", ".jpeg", ".png", ".webp", ".heic", ".bmp"],

  // DPI 選項及其用途描述
  DPI_OPTIONS: [
    { value: 96, label: "螢幕用" },
    { value: 150, label: "一般文件" },
    { value: 220, label: "推薦列印" },
    { value: 300, label: "極致清晰" }
  ]
};

// 凍結物件以防止意外修改
Object.freeze(CONFIG_MANAGER);
Object.freeze(CONFIG_MANAGER.DEFAULT_CONFIG);
