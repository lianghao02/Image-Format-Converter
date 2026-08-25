[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "dist"
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root "src\PoliceImageToolkit\PoliceImageToolkit.csproj"
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

$sw = [System.Diagnostics.Stopwatch]::StartNew()

& $dotnet publish $projectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $destination

if ($LASTEXITCODE -ne 0) {
    throw "Publish failed with exit code: $LASTEXITCODE"
}

$sw.Stop()
$exePath = Join-Path $destination "PoliceImageToolkit.exe"

if (Test-Path $exePath) {
    $item = Get-Item $exePath
    $sizeMb = [math]::Round($item.Length / 1MB, 2)
    Write-Output ""
    Write-Output "=========================================================="
    Write-Output "SUCCESS: Published single-file executable!"
    Write-Output "Path:    $exePath"
    Write-Output "Size:    $sizeMb MB"
    Write-Output "Elapsed: $($sw.Elapsed.TotalSeconds.ToString('0.##')) s"
    Write-Output "=========================================================="
} else {
    throw "PoliceImageToolkit.exe was not found in the output directory."
}
