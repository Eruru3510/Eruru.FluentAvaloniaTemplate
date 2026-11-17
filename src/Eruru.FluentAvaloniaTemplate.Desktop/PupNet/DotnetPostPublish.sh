#!/usr/bin/env bash
source="../bin/${BUILD_TARGET}/net11.0/publish/${DOTNET_RUNTIME}"
target="${BUILD_APP_BIN}"
mkdir -p "$target" || exit $?
cp -a "$source/." "$target/" || exit $?