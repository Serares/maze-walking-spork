#!/bin/bash
# Build script for creating production artifact of MazeWalking application
# This script builds the React UI and .NET backend into a single deployable package

set -e

echo "=== MazeWalking Build Artifact Script ==="
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Step 1: Build React UI
echo "[1/4] Building React UI..."
cd "MazeWalking.UI"

if [ ! -d "node_modules" ]; then
    echo "Installing npm dependencies..."
    npm install
fi

echo "Running production build..."
npm run build

echo "✓ React UI built successfully"
echo ""

# Step 2: Copy dist to wwwroot
echo "[2/4] Copying static files to wwwroot..."
cd "$SCRIPT_DIR"

WWWROOT_PATH="MazeWalking.Web/wwwroot"
if [ -d "$WWWROOT_PATH" ]; then
    echo "Removing existing wwwroot..."
    rm -rf "$WWWROOT_PATH"
fi

echo "Copying dist to wwwroot..."
cp -r "MazeWalking.UI/dist" "$WWWROOT_PATH"

echo "✓ Static files copied successfully"
echo ""

# Step 3: Build .NET application
echo "[3/4] Building .NET application..."
cd "MazeWalking.Web"

echo "Running dotnet publish..."
dotnet publish -c Release -o "../publish"

echo "✓ .NET application published successfully"
echo ""

# Step 4: Summary
echo "[4/4] Build Summary"
cd "$SCRIPT_DIR"

PUBLISH_SIZE=$(du -sh publish | cut -f1)
echo "Artifact location: $SCRIPT_DIR/publish"
echo "Artifact size: $PUBLISH_SIZE"
echo ""
echo "✓ Build completed successfully!"
echo ""
echo "To run the application:"
echo "  cd publish"
echo "  dotnet MazeWalking.Web.dll"
