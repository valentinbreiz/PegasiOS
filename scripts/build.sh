#!/usr/bin/env bash
# Builds the PegasiOS kernel ISO for the PinePhone (ARM64).
#
#   scripts/build.sh [Debug|Release]
#
# The Cosmos build calls the IL patcher as `cosmos.patcher` on PATH. A patcher
# of another Cosmos version than the packages gives build errors that look like
# code errors, so this script installs the patcher of the version global.json
# pins under .tools/ and puts that folder first on PATH. The global tools of
# the machine are left alone.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIG="${1:-Debug}"
TOOLS="$ROOT/.tools"
OUT="$ROOT/output-arm64"

VERSION="$(sed -n 's/.*"Cosmos\.Sdk"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$ROOT/global.json")"
if [ -z "$VERSION" ]; then
    echo "error: no Cosmos.Sdk version in global.json" >&2
    exit 1
fi

if [ "$(cat "$TOOLS/.cosmos-version" 2>/dev/null || true)" != "$VERSION" ]; then
    echo "Installing cosmos.patcher $VERSION into .tools/"
    rm -rf "$TOOLS"
    dotnet tool install Cosmos.Patcher --version "$VERSION" --tool-path "$TOOLS" \
        --add-source https://api.nuget.org/v3/index.json >/dev/null
    echo "$VERSION" > "$TOOLS/.cosmos-version"
fi

export PATH="$TOOLS:$PATH"

echo "Building PegasiOS ($CONFIG, linux-arm64) against Cosmos $VERSION"
# From the project folder: the ISO stage writes bin/ and obj/ under the current directory.
cd "$ROOT/src/PegasiOS"
dotnet publish PegasiOS.csproj -c "$CONFIG" -r linux-arm64 -o "$OUT" --verbosity minimal

ls -l "$OUT/PegasiOS.iso"
