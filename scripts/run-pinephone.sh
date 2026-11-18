#!/usr/bin/env bash
# Boots the PegasiOS ISO in a QEMU virtual machine sized like the
# PinePhone Beta Edition with Convergence Package (docs/pinephone.md).
#
#   scripts/run-pinephone.sh [--headless] [--iso PATH] [--serial SPEC] [-- extra QEMU args]
#
# --serial takes a QEMU character device (default stdio, e.g. file:uart.log).
#
# QEMU has no Allwinner A64 machine, so this is the generic ARM64 `virt`
# machine given the phone's shape, not an emulation of its chips:
#
#   PinePhone                          this VM
#   4x Cortex-A53 (Allwinner A64)      -cpu cortex-a53 -smp 4
#   3 GB LPDDR3                        -m 3072M
#   5.95" panel, 720x1440 portrait     virtio-gpu, xres=720 yres=1440
#   capacitive touchscreen             virtio mouse (a click is a tap)
#   32 GB eMMC                         32 GB sparse image on virtio-blk
#   serial console (headphone jack)    PL011 on stdio
#
# The ramfb device is only there for the firmware and Limine, which need a
# linear framebuffer to boot; the kernel draws on the virtio-gpu panel. It is
# declared after the panel so the panel is the first screen QEMU shows.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ISO="$ROOT/output-arm64/PegasiOS.iso"
EMMC="$ROOT/emmc.img"
SERIAL="stdio"
HEADLESS=0
EXTRA=()

while [ $# -gt 0 ]; do
    case "$1" in
        --headless) HEADLESS=1 ;;
        --iso) ISO="$2"; shift ;;
        --serial) SERIAL="$2"; shift ;;
        --) shift; EXTRA=("$@"); break ;;
        *) echo "unknown option: $1" >&2; exit 2 ;;
    esac
    shift
done

if [ ! -f "$ISO" ]; then
    echo "error: $ISO not found, run scripts/build.sh first" >&2
    exit 1
fi

# Prefer the QEMU the Cosmos tools bundle ships (it carries the EDK2 firmware).
BUNDLE="$HOME/.cosmos/tools/qemu"
if [ -x "$BUNDLE/qemu-system-aarch64" ]; then
    QEMU="$BUNDLE/qemu-system-aarch64"
    FIRMWARE=(-L "$BUNDLE/share/qemu" -bios edk2-aarch64-code.fd)
else
    QEMU="$(command -v qemu-system-aarch64)"
    FIRMWARE=(-bios "${PEGASI_FIRMWARE:-/usr/share/AAVMF/AAVMF_CODE.fd}")
fi

# The eMMC: a raw sparse file, so 32 GB on paper and nothing on disk until written.
if [ ! -f "$EMMC" ]; then
    truncate -s 32G "$EMMC"
fi

DISPLAY_ARGS=()
if [ "$HEADLESS" = 1 ]; then
    DISPLAY_ARGS=(-display none)
fi

exec "$QEMU" \
    -M virt,highmem=off \
    -cpu cortex-a53 -smp 4 -m 3072M \
    "${FIRMWARE[@]}" \
    -device virtio-gpu-pci,id=panel,xres=720,yres=1440 \
    -device ramfb \
    -device virtio-scsi-pci,id=scsi0 \
    -drive "file=$ISO,if=none,id=cosmoscd,format=raw,readonly=on,media=cdrom" \
    -device scsi-cd,drive=cosmoscd,bus=scsi0.0,bootindex=0 \
    -drive "file=$EMMC,if=none,id=emmc,format=raw" \
    -device virtio-blk-device,drive=emmc \
    -device virtio-keyboard-device \
    -device virtio-mouse-device \
    -nic none \
    -boot d -no-reboot \
    -serial "$SERIAL" \
    "${DISPLAY_ARGS[@]}" \
    "${EXTRA[@]}"
