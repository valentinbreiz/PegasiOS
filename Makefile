# PegasiOS: build the kernel and boot it in the PinePhone virtual machine.
#
#   make build              build output-arm64/PegasiOS.iso
#   make run                build, then boot it in a QEMU window
#   make shot               build, boot headless and save the screen to out/home.png
#   make clean              remove build outputs
#
# CONFIG=Release builds the release configuration.

CONFIG ?= Debug

.PHONY: build run shot clean

build:
	scripts/build.sh $(CONFIG)

run: build
	scripts/run-pinephone.sh

shot: build
	python3 -I scripts/vm-drive.py --out out sleep=1 shot=home

clean:
	rm -rf output-arm64 out bin obj src/*/bin src/*/obj
