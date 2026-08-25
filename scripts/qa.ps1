[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$safe = $root.Replace('\', '/')

# 1. Git Diff Check
& git -c "safe.directory=$safe" -C $root diff --check
if ($LASTEXITCODE -ne 0) { throw "Git diff format check failed." }

# 2. Sensitive Data Scan
$pattern = '(?i)(api[_-]?key|secret|access[_-]?token|password)\s*[:=]\s*["''][^"'']{8,}'
$files = @(& git -c "safe.directory=$safe" -C $root ls-files --cached --others --exclude-standard)
foreach ($file in $files) {
    if ($file -match '(^|/)\.env(\.|$)|\.(png|jpe?g|gif|zip|db|ico|dll|exe)$') { continue }
    try {
        if (Select-String -LiteralPath (Join-Path $root $file) -Pattern $pattern -Quiet -Encoding UTF8 -ErrorAction Stop) {
            throw "Suspected sensitive value found in: $file"
        }
    } catch [System.ArgumentException] { }
}

# 3. C# Build Verification
$projectPath = Join-Path $root "src\PoliceImageToolkit\PoliceImageToolkit.csproj"
if (Test-Path $projectPath) {
    $dotnet = "dotnet"
    if (Test-Path "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe") {
        $dotnet = "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe"
    } elseif (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
        $dotnet = "dotnet"
    }
    Write-Output "Building C# project for verification..."
    & $dotnet build $projectPath -c Release --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "C# build verification failed." }
}

Write-Output "Shared QA passed. Build and integrity checks normal."