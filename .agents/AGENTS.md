# Codex 專案指引

進入本專案時，先閱讀 `MEMORY.md`、`README.md` 與 `CHANGELOG.md`。

## 核心規範
- 本專案主力為 C# (.NET 8 WPF) 原生桌面應用程式，原始碼位於 `src/PoliceImageToolkit/`。
- 舊版純前端網頁工具已封存至 `legacy_web/`，僅作功能比對，不在此開發新功能。
- 遵循 MVVM 架構，修改 XAML 時注意 `<Resources>` 必須宣告於使用處之前。
- SDK 位置：優先使用 `scripts/build.ps1` 與 `scripts/qa.ps1` 進行建置與檢核。
- 交付前必須執行 `powershell -ExecutionPolicy Bypass -File .\scripts\qa.ps1` 驗證通過。