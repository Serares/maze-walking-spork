#!/usr/bin/env pwsh
# Build script for creating production artifact of MazeWalking application
# This script builds the React UI and .NET backend into a single deployable package

$ErrorActionPreference = "Stop"

Write-Host "=== MazeWalking Build Artifact Script ===" -ForegroundColor Cyan
Write-Host ""

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Step 1: Build React UI
Write-Host "[1/4] Building React UI..." -ForegroundColor Yellow
Set-Location "MazeWalking.UI"

if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm dependencies..." -ForegroundColor Gray
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Error "npm install failed"
        exit 1
    }
}

Write-Host "Running production build..." -ForegroundColor Gray
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Error "npm build failed"
    exit 1
}

Write-Host "✓ React UI built successfully" -ForegroundColor Green
Write-Host ""

# Step 2: Copy dist to wwwroot
Write-Host "[2/4] Copying static files to wwwroot..." -ForegroundColor Yellow
Set-Location $scriptDir

$wwwrootPath = "MazeWalking.Web\wwwroot"
if (Test-Path $wwwrootPath) {
    Write-Host "Removing existing wwwroot..." -ForegroundColor Gray
    Remove-Item -Recurse -Force $wwwrootPath
}

Write-Host "Copying dist to wwwroot..." -ForegroundColor Gray
Copy-Item -Recurse "MazeWalking.UI\dist" $wwwrootPath

Write-Host "✓ Static files copied successfully" -ForegroundColor Green
Write-Host ""

# Step 3: Build .NET application
Write-Host "[3/4] Building .NET application..." -ForegroundColor Yellow
Set-Location "MazeWalking.Web"

Write-Host "Running dotnet publish..." -ForegroundColor Gray
dotnet publish -c Release -o "..\publish"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed"
    exit 1
}

Write-Host "✓ .NET application published successfully" -ForegroundColor Green
Write-Host ""

# Step 4: Summary
Write-Host "[4/4] Build Summary" -ForegroundColor Yellow
Set-Location $scriptDir

$publishSize = (Get-ChildItem -Recurse "publish" | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Artifact location: $scriptDir\publish" -ForegroundColor Cyan
Write-Host "Artifact size: $($publishSize.ToString('F2')) MB" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To run the application:" -ForegroundColor White
Write-Host "  cd publish" -ForegroundColor Gray
Write-Host "  dotnet MazeWalking.Web.dll" -ForegroundColor Gray
