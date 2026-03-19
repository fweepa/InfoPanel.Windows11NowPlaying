# Build Windows11NowPlayingPlugin and create .infopanel package for InfoPanel import
# Run: .\pack.ps1

$ErrorActionPreference = "Stop"

# Build (skip if output exists)
$releaseDir = "bin\Release\net8.0-windows10.0.19041.0"
$debugDir = "bin\Debug\net8.0-windows10.0.19041.0"

if (Test-Path $releaseDir) {
    $outDir = $releaseDir
    Write-Host "Using existing Release build." -ForegroundColor Cyan
} elseif (Test-Path $debugDir) {
    $outDir = $debugDir
    Write-Host "Using existing Debug build." -ForegroundColor Cyan
} else {
    Write-Host "Building project..." -ForegroundColor Cyan
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $outDir = $releaseDir
}

# Create package structure (InfoPanel expects the marketplace-matching folder inside zip)
# Marketplace repoName is derived from the repo portion after `.../` in `PluginInfo.ini` Website:
#   https://github.com/fweepa/InfoPanel.Windows11NowPlaying -> repoName = InfoPanel.Windows11NowPlaying
$packageDir = "packaging\InfoPanel.Windows11NowPlaying"
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

# Loader convention expects <FolderName>.dll (FolderName = InfoPanel.Windows11NowPlaying)
Copy-Item "$outDir\InfoPanel.Windows11NowPlaying.dll" -Destination $packageDir
Copy-Item "$outDir\PluginInfo.ini" -Destination $packageDir

# Create .zip package (InfoPanel only processes InfoPanel.*.zip files)
$zipPath = "InfoPanel.Windows11NowPlayingPlugin.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath }

Compress-Archive -Path $packageDir -DestinationPath $zipPath -Force

# Cleanup
Remove-Item -Recurse -Force "packaging"

Write-Host ""
Write-Host "Package created: $zipPath" -ForegroundColor Green
Write-Host "InfoPanel only processes InfoPanel.*.zip files - use the .zip for import." -ForegroundColor Gray
Write-Host ""
Write-Host "Import via InfoPanel: Plugins -> Add Plugin from ZIP (select the .zip file)" -ForegroundColor Yellow
