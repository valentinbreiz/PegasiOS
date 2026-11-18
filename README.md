# PegasiOS

A phone operating system written in C# on [Cosmos](https://github.com/CosmosOS/Cosmos) gen3. The first target is the [PinePhone Beta Edition with Convergence Package](docs/pinephone.md).

PegasiOS is at its first step: a demo kernel that boots in a virtual machine shaped like the phone and draws one screen. It does not run on a real PinePhone yet.

## What works

- The kernel builds for ARM64 against Cosmos 3.0.97 from nuget.org.
- It boots in QEMU on a machine with the phone's CPU model, memory, panel size and storage size.
- It draws a 720x1440 screen: a status bar, the clock, a card with the hardware the kernel found, and a button that counts taps.
- The mouse stands in for the finger: the pointer is where the finger is, a click is a tap.

[docs/pinephone.md](docs/pinephone.md) has the phone's specifications, what the virtual machine reproduces of them, and what the real device still needs.

## Requirements

- The [.NET 10 SDK](https://dotnet.microsoft.com/download).
- The Cosmos tools (clang, lld, xorriso, QEMU with the ARM64 firmware): `dotnet tool install -g Cosmos.Tools`, then `cosmos install`.

## Build and run

```bash
make build    # output-arm64/PegasiOS.iso
make run      # build, then boot it in a QEMU window
make shot     # build, boot without a window, save the screen to out/home.png
```

The first build installs the Cosmos IL patcher of the pinned version under `.tools/`, so the global tools of the machine are not touched.

In the QEMU window, click the screen to give it the mouse, and press Ctrl+Alt+G to take it back. The kernel log is on the terminal.

`scripts/vm-drive.py` boots the kernel without a window and plays steps on it (move the finger, tap, take a screenshot). It is how a change to the screen is checked:

```bash
python3 -I scripts/vm-drive.py --out out sleep=1 move=0,300 tap sleep=0.5 shot=tapped
```

## Layout

| Path | What it holds |
|---|---|
| `src/PegasiOS/` | the kernel |
| `scripts/` | build, the PinePhone virtual machine, the screen driver |
| `docs/pinephone.md` | the device and its virtual machine |
| `global.json` | the Cosmos version everything builds against |

## License

BSD 3-Clause, see [LICENSE](LICENSE).
