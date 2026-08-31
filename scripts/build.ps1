[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "dist"
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root "src\PoliceImageToolkit\PoliceImageToolkit.csproj"
$projectDir = Join-Path $root "src\PoliceImageToolkit"
$destination = Join-Path $root $OutputDir

# 自動定位 dotnet.exe (優先使用具有 SDK 的路徑)
$dotnet = "dotnet"
if (Test-Path "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe") {
    $dotnet = "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe"
} elseif (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
    $dotnet = "dotnet"
}
# 關閉可能佔用 dist/ 執行檔之既有進程
Get-Process "PoliceImageToolkit" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 500

Write-Output "=========================================================="
Write-Output "Police-Image-Toolkit Single-File Publish Script"
Write-Output "=========================================================="
Write-Output "Project: $projectPath"
Write-Output "Output:  $destination"
Write-Output "Config:  $Configuration (win-x64 / Self-Contained Single-File)"
Write-Output "Dotnet:  $dotnet"
Write-Output ""

# 清理舊的 bin / obj / dist 快取
Write-Output "Cleaning previous build artifacts and cache..."
$binDir = Join-Path $projectDir "bin"
$objDir = Join-Path $projectDir "obj"
if (Test-Path $binDir) { Remove-Item $binDir -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force -ErrorAction SilentlyContinue }
if (Test-Path $destination) {
    Get-ChildItem -Path $destination -File | Remove-Item -Force -ErrorAction SilentlyContinue
}

$sw = [System.Diagnostics.Stopwatch]::StartNew()

& $dotnet publish $projectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $destination

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed with exit code: $LASTEXITCODE"
}

# 清理殘留的 pdb 檔案 (若有)
Get-ChildItem -Path $destination -Filter "*.pdb" | Remove-Item -Force -ErrorAction SilentlyContinue

$sw.Stop()
$exePath = Join-Path $destination "PoliceImageToolkit.exe"
$hashPath = Join-Path $destination "SHA256SUMS.txt"
$guidePath = Join-Path $destination "GUIDE.txt"
$versionSrc = Join-Path $projectDir "version.txt"
$versionDst = Join-Path $destination "version.txt"

if (Test-Path $versionSrc) {
    Copy-Item -LiteralPath $versionSrc -Destination $versionDst -Force
}

# 觸發 Windows Shell 重新整理圖示快取 (SHChangeNotify)
try {
    Add-Type -TypeDefinition @"
    using System;
    using System.Runtime.InteropServices;
    public class ShellNotifier {
        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
"@ -ErrorAction SilentlyContinue
    [ShellNotifier]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero) # SHCNE_ASSOCCHANGED
} catch {
    # 忽略通知例外
}

if (Test-Path $exePath) {
    $item = Get-Item $exePath
    $sizeMb = [math]::Round($item.Length / 1MB, 2)
    $sha256 = (Get-FileHash -LiteralPath $exePath -Algorithm SHA256).Hash.ToLowerInvariant()
    Set-Content -LiteralPath $hashPath -Value "$sha256 *$($item.Name)" -Encoding utf8

    $guide = @"
Police Image Toolkit Portable Edition Guide (v11.3.0)
======================================================

1. 支援 Windows 10 / 11 64 位元；不需安裝 .NET Runtime。
2. 將本資料夾完整複製到可寫入的位置後，雙擊 PoliceImageToolkit.exe。
3. 本機處理影像與影片；不會自動連線。只有使用者按下「檢查更新」並確認後，才會查詢 GitHub Release。
4. 若 Windows SmartScreen 顯示提示，請先確認檔案來源與下列 SHA-256 值，再選擇是否執行。

SHA-256:
$sha256

命令列驗證指令：
Get-FileHash .\PoliceImageToolkit.exe -Algorithm SHA256
"@
    Set-Content -LiteralPath $guidePath -Value $guide -Encoding utf8

    Write-Output ""
    Write-Output "=========================================================="
    Write-Output "SUCCESS: Published single-file executable!"
    Write-Output "Path:    $exePath"
    Write-Output "Size:    $sizeMb MB"
    Write-Output "SHA256:  $sha256"
    Write-Output "Guide:   $guidePath"
    Write-Output "Elapsed: $($sw.Elapsed.TotalSeconds.ToString('0.##')) s"
    Write-Output "=========================================================="
} else {
    throw "PoliceImageToolkit.exe was not found in the output directory."
}
