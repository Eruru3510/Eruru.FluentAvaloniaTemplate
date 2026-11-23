
#!/bin/bash
set -e

BASE_NAME="FluentAvaloniaTemplate"
WORK_DIR="$(pwd)"
SOURCE_APP="$WORK_DIR/App"
RELEASE_DIR="$WORK_DIR/../bin/Release"
TARGET_APP="$RELEASE_DIR/macOS/$BASE_NAME.app"
PUBLISH_DIR="$TARGET_APP/Contents/MacOS"
DMG_PATH="$HOME/Desktop/$BASE_NAME.dmg"

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
	--icon-size 160 \
	--window-size 660 400 \
	--app-drop-link 480 170 \
	--icon $BASE_NAME.app 180 170 \
	--overwrite \
    "$DMG_PATH" \
    "$RELEASE_DIR/macOS"

echo "Build completed!"
echo "Runtime: $runtime"
echo "DMG: $DMG_PATH"
