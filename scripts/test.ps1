[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $root 'tests\PoliceImageToolkit.CoreTests\PoliceImageToolkit.CoreTests.csproj'

$dotnet = 'dotnet'
if (Test-Path "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe") {
    $dotnet = "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe"
}

& $dotnet run --project $testProject -c Release --nologo
if ($LASTEXITCODE -ne 0) {
    throw "核心服務測試失敗，結束代碼：$LASTEXITCODE"
}

