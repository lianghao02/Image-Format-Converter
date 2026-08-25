# Antigravity 專案指引

先閱讀工作區 AGENTS.md、專案文件與 docs/DEVELOPMENT_RULES.md。

- 本專案已升級為 C# (.NET 8 WPF) 原生桌面應用程式，舊網頁版位於 legacy_web/ 封存。
- 核心原始碼位於 src/PoliceImageToolkit/。
- 不修改 .agents/，也不複製 Codex 指引。
- 發布前執行 scripts/build.ps1，提交前執行 scripts/qa.ps1。