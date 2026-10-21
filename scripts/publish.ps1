<#
.SYNOPSIS
    Builds the Tmatmaia installer.

.DESCRIPTION
    Outputs, all in dist/:
      Tmatmaia/                          self-contained build that the installer packs
                                         (you can also run Tmatmaia.exe from here)
      Tmatmaia-<version>-setup.exe       the installer to release (needs Inno Setup 6)
    The installed app doesn't need .NET on the target machine.

.PARAMETER SkipInstaller
    Only build the dist/Tmatmaia folder.
#>
param(
    [string]$Configuration = 'Release',
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'src\Tmatmaia\Tmatmaia.csproj'
$outDir = Join-Path $root 'dist\Tmatmaia'

$running = Get-Process Tmatmaia -ErrorAction SilentlyContinue
if ($running) {
    $running | Stop-Process -Force
    $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}
if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }

dotnet publish $project -c $Configuration -r win-x64 --self-contained true -o $outDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

$version = ([xml](Get-Content $project)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
Write-Host "App folder: $outDir"

if ($SkipInstaller) { return }

$iscc = @(
    (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source,
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

if (-not $iscc) {
    Write-Warning 'Inno Setup 6 not found, skipping the installer. Install it with: winget install JRSoftware.InnoSetup'
    return
}

& $iscc /Q "/DAppVersion=$version" "/DSourceDir=$outDir" (Join-Path $root 'installer\Tmatmaia.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed with exit code $LASTEXITCODE" }
Write-Host "Installer:  $(Join-Path $root "dist\Tmatmaia-$version-setup.exe")"
