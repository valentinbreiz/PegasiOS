#!/usr/bin/env python3
"""Boots PegasiOS headless in the PinePhone VM, drives it and takes screenshots.

    scripts/vm-drive.py --out DIR [--wait-for TEXT] [--boot-timeout S] STEP...

Each STEP runs in order once the serial log shows the boot marker:

    shot=NAME        save the phone panel to DIR/NAME.png
    sleep=SECONDS    wait
    move=DX,DY       move the pointer by that many pixels (relative, like a finger drag)
    tap              press and release the left button (a tap)
    press / release  hold or lift the finger
    key=NAME         press a key (QEMU qcode: ret, esc, a, ...)

The serial output goes to DIR/uart.log. Exit code 0 when every step ran and
the log holds no kernel panic.
"""

import argparse
import json
import os
import socket
import struct
import subprocess
import sys
import time
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PANIC_MARKERS = ("KERNEL PANIC", "Unhandled exception", "PANIC:")


class Qmp:
    """A minimal QMP client over a unix socket."""

    def __init__(self, path, timeout):
        deadline = time.monotonic() + timeout
        while True:
            try:
                self.sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
                self.sock.connect(path)
                break
            except OSError:
                self.sock.close()
                if time.monotonic() > deadline:
                    raise
                time.sleep(0.1)
        self.file = self.sock.makefile("rw", encoding="utf-8")
        self._read()  # greeting
        self.command("qmp_capabilities")

    def _read(self):
        while True:
            line = self.file.readline()
            if not line:
                raise RuntimeError("QMP connection closed")
            message = json.loads(line)
            if "event" not in message:
                return message

    def command(self, name, **arguments):
        request = {"execute": name}
        if arguments:
            request["arguments"] = arguments
        self.file.write(json.dumps(request) + "\n")
        self.file.flush()
        reply = self._read()
        if "error" in reply:
            raise RuntimeError(f"{name}: {reply['error']}")
        return reply.get("return")


def wait_for(path, text, timeout):
    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        try:
            with open(path, "rb") as handle:
                if text.encode() in handle.read():
                    return True
        except FileNotFoundError:
            pass
        time.sleep(0.25)
    return False


def send(qmp, events):
    qmp.command("input-send-event", events=events)


def ppm_to_png(ppm_path, png_path):
    """Converts QEMU's binary PPM screen dump to PNG (the bundled QEMU has no libpng)."""
    with open(ppm_path, "rb") as handle:
        data = handle.read()
    fields = []
    position = 0
    while len(fields) < 4:
        end = position
        while data[end:end + 1] not in (b" ", b"\n", b"\t", b"\r"):
            end += 1
        fields.append(data[position:end])
        position = end + 1
    if fields[0] != b"P6" or fields[3] != b"255":
        raise RuntimeError(f"unexpected screen dump format: {fields}")
    width, height = int(fields[1]), int(fields[2])
    pixels = data[position:position + width * height * 3]
    rows = b"".join(b"\x00" + pixels[row * width * 3:(row + 1) * width * 3] for row in range(height))

    def chunk(kind, body):
        return struct.pack(">I", len(body)) + kind + body + struct.pack(">I", zlib.crc32(kind + body))

    with open(png_path, "wb") as handle:
        handle.write(b"\x89PNG\r\n\x1a\n")
        handle.write(chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)))
        handle.write(chunk(b"IDAT", zlib.compress(rows, 6)))
        handle.write(chunk(b"IEND", b""))
    return width, height


def button(down):
    return {"type": "btn", "data": {"down": down, "button": "left"}}


def run_step(qmp, step, out):
    name, _, value = step.partition("=")
    if name == "shot":
        target = os.path.join(out, value + ".png")
        dump = os.path.join(out, value + ".ppm")
        qmp.command("screendump", filename=dump, device="panel")
        width, height = ppm_to_png(dump, target)
        os.remove(dump)
        print(f"shot {target} {width}x{height}")
    elif name == "sleep":
        time.sleep(float(value))
    elif name == "move":
        dx, dy = (int(part) for part in value.split(","))
        # Small relative steps, as a hand would produce: one large jump is
        # clamped by the guest's pointer handling.
        steps = max(abs(dx), abs(dy)) // 20 + 1
        for index in range(steps):
            part_x = dx * (index + 1) // steps - dx * index // steps
            part_y = dy * (index + 1) // steps - dy * index // steps
            send(qmp, [
                {"type": "rel", "data": {"axis": "x", "value": part_x}},
                {"type": "rel", "data": {"axis": "y", "value": part_y}},
            ])
            time.sleep(0.02)
    elif name == "tap":
        send(qmp, [button(True)])
        time.sleep(0.15)
        send(qmp, [button(False)])
    elif name == "press":
        send(qmp, [button(True)])
    elif name == "release":
        send(qmp, [button(False)])
    elif name == "key":
        qmp.command("send-key", keys=[{"type": "qcode", "data": value}])
    else:
        raise SystemExit(f"unknown step: {step}")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--out", required=True)
    parser.add_argument("--iso")
    parser.add_argument("--wait-for", default="[PegasiOS] ready")
    parser.add_argument("--boot-timeout", type=float, default=120)
    parser.add_argument("steps", nargs="*")
    args = parser.parse_args()

    os.makedirs(args.out, exist_ok=True)
    uart = os.path.join(args.out, "uart.log")
    qmp_path = os.path.join(args.out, "qmp.sock")
    for stale in (uart, qmp_path):
        if os.path.exists(stale):
            os.remove(stale)

    command = [os.path.join(ROOT, "scripts", "run-pinephone.sh"), "--headless", "--serial", f"file:{uart}"]
    if args.iso:
        command += ["--iso", args.iso]
    command += ["--", "-qmp", f"unix:{qmp_path},server=on,wait=off"]

    qemu = subprocess.Popen(command, stdin=subprocess.DEVNULL)
    status = 1
    try:
        qmp = Qmp(qmp_path, 15)
        if not wait_for(uart, args.wait_for, args.boot_timeout):
            print(f"boot marker {args.wait_for!r} not seen after {args.boot_timeout:.0f} s", file=sys.stderr)
        else:
            for step in args.steps:
                run_step(qmp, step, args.out)
            status = 0
        try:
            qmp.command("quit")
        except (RuntimeError, OSError):
            pass
    finally:
        try:
            qemu.wait(timeout=10)
        except subprocess.TimeoutExpired:
            qemu.kill()

    with open(uart, "rb") as handle:
        log = handle.read().decode("utf-8", "replace")
    panics = sum(log.count(marker) for marker in PANIC_MARKERS)
    print(f"panic lines: {panics}")
    if panics:
        status = 1
    print(f"DONE rc={status}")
    return status


if __name__ == "__main__":
    sys.exit(main())
