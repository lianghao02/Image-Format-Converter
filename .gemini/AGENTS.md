# Antigravity 專案指引

進入本專案時，先閱讀工作區規則、`MEMORY.md`、`README.md` 與 `CHANGELOG.md`。

## 核心規範
- 本專案主力為 C# (.NET 8 WPF) 原生桌面應用程式，原始碼位於 `src/PoliceImageToolkit/`。
- 歷史網頁版已封存於 Git 歷史，工作區為純粹的 C# WPF 單一專案結構。
- 遵循 MVVM 架構，修改 XAML 時注意 `<Resources>` 必須宣告於使用處之前。
- SDK 位置：優先使用 `scripts/build.ps1` 與 `scripts/qa.ps1` 進行建置與檢核。
- 發布前執行 `scripts/build.ps1`，提交前執行 `scripts/qa.ps1`。