
#!/bin/bash
set -e

WORK_DIR="$(pwd)"
SOURCE_APP="$WORK_DIR/App"
RELEASE_DIR="$WORK_DIR/../bin/Release"
TARGET_APP="$RELEASE_DIR/macOS/FluentAvaloniaTemplate.app"
PUBLISH_DIR="$TARGET_APP/Contents/MacOS"
DMG_PATH="$HOME/Desktop/FluentAvaloniaTemplate.dmg"

echo "Working directory: $WORK_DIR"

runtime="${1:?Usage: $0 <runtime>}"

if [ -z "$runtime" ]; then
    echo "Error: Runtime cannot be empty."
    exit 1
fi

PUBLISH_SOURCE="$RELEASE_DIR/net11.0/publish/$runtime"

if [ ! -d "$SOURCE_APP" ]; then
    echo "Error: App directory not found: $SOURCE_APP"
    exit 1
fi

if [ ! -d "$PUBLISH_SOURCE" ]; then
    echo "Error: Publish directory not found: $PUBLISH_SOURCE"
    exit 1
fi

echo "Removing previous macOS directory..."
rm -rf "$RELEASE_DIR/macOS"

echo "Creating application directory..."
mkdir -p "$TARGET_APP"

echo "Copying App contents..."
ditto "$SOURCE_APP" "$TARGET_APP"

echo "Copying publish artifacts..."
mkdir -p "$PUBLISH_DIR"
ditto "$PUBLISH_SOURCE" "$PUBLISH_DIR"

echo "Creating DMG..."
create-dmg \
    --window-pos 200 120 \
    --window-size 600 400 \
    --icon-size 100 \
    --app-drop-link 450 190 \
    "$DMG_PATH" \
    "$RELEASE_DIR/macOS"

echo "Build completed!"
echo "Runtime: $runtime"
echo "DMG: $DMG_PATH"
