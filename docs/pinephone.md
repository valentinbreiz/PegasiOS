# PinePhone Beta Edition with Convergence Package

The first device PegasiOS targets. The figures below come from the PINE64 store page and the PINE64 documentation, both read on 18 November 2026:

- <https://pine64.com/product/pinephone-beta-edition-with-convergence-package/>
- <https://pine64.org/documentation/PinePhone/Further_information/Specifications/>
- <https://pine64.org/documentation/PinePhone/Further_information/Components/>
- <https://pine64.org/2021/03/19/beta-edition-pre-orders/>

## Specifications

| | |
|---|---|
| Body | 160.5 x 76.6 x 9.2 mm, 185 g, plastic, black, Micro-SIM |
| Display | 5.95 inch HD IPS, capacitive touchscreen, 16M colors, 1440x720, 18:9 |
| Chipset | Allwinner A64 |
| CPU | 64-bit quad-core ARM Cortex-A53, 1.2 GHz (clocked at 1.152 GHz on the PinePhone) |
| GPU | Mali-400 MP2 |
| Memory | 3 GB LPDDR3 SDRAM |
| Storage | 32 GB eMMC, microSD slot (SDHC and SDXC, up to 2 TB) |
| Main camera | 5 MP, 1/4", LED flash |
| Selfie camera | 2 MP, f/2.8, 1/5" |
| Sound | mono loudspeaker, 3.5 mm stereo jack with microphone |
| Modem | Quectel EG25-G: LTE-FDD B1/B2/B3/B4/B5/B7/B8/B12/B13/B18/B19/B20/B25/B26/B28, LTE-TDD B38/B39/B40/B41, WCDMA B1/B2/B4/B5/B6/B8/B19, GSM 850/900/1800/1900 MHz |
| WLAN | Wi-Fi 802.11 b/g/n, single band, hotspot |
| Bluetooth | 4.0, A2DP |
| GNSS | GPS, GLONASS, BeiDou, Galileo, QZSS, with A-GPS |
| USB | Type-C, USB host, DisplayPort Alternate Mode output |
| Sensors | accelerometer, gyroscope, proximity, ambient light, magnetometer |
| Actuator | vibration motor |
| Privacy switches | modem (with GNSS), Wi-Fi and Bluetooth, microphone, cameras |
| Battery | removable Li-Po, 2750 to 3000 mAh, Samsung J7 form factor |
| Charging | USB Type-C, 15 W (5 V 3 A), USB PD |
| In the box | the phone, a USB-A to USB-C cable, a USB-C docking bar (2 USB Type-A host ports, digital video out, 10/100 Ethernet) |
| Board | PCBA revision 1.2b |
| Shipped OS | Manjaro with Plasma Mobile |

The Convergence Package is the 3 GB / 32 GB variant with the dock. The plain Beta Edition has 2 GB and 16 GB.

## Components

What a driver has to talk to, from the Components page of the documentation.

| Part | Model |
|---|---|
| LCD | Xingbangda XBD599 (MIPI DSI) |
| Touchscreen | Goodix GT917S |
| Power management, power button, fuel gauge | X-Powers AXP803 |
| Wi-Fi and Bluetooth | Realtek RTL8723CS |
| Modem and GNSS | Quectel EG25-G |
| Rear camera | OmniVision OV5640 |
| Front camera | GalaxyCore GC2145 |
| Camera flash | SGMICRO SGM3140 |
| Accelerometer and gyroscope | InvenSense MPU-6050 |
| Magnetometer | ST LIS3MDL (the documentation says the Beta Edition carries a Voltafield AF8133L in its place) |
| Ambient light and proximity | SensorTek STK3335 |
| Notification LED | RGB LED |
| Volume buttons | on the SoC's KEYADC |
| Serial console | on the headphone jack, when hardware switch 6 is off |

## The virtual machine

`scripts/run-pinephone.sh` boots PegasiOS in QEMU. QEMU has no Allwinner A64 machine (its Allwinner boards, `cubieboard`, `orangepi-pc` and `bpim2u`, are 32-bit), so the VM is the generic ARM64 `virt` machine given the shape of the phone. It has the phone's CPU model, core count, memory, panel size and storage size. It does not have its chips.

| PinePhone | Virtual machine |
|---|---|
| 4 Cortex-A53 cores | `-cpu cortex-a53 -smp 4` |
| 3 GB of RAM | `-m 3072M` |
| 720x1440 panel (portrait) | virtio-gpu, `xres=720,yres=1440` |
| Goodix touchscreen | virtio mouse: the pointer is the finger, a click is a tap |
| 32 GB eMMC | a 32 GB sparse image on virtio-blk |
| serial console on the jack | PL011 UART on stdio |
| U-Boot, device tree | EDK2 UEFI firmware, Limine, ACPI |
| modem, Wi-Fi, Bluetooth, cameras, sensors, battery | absent |

What the About screen reports on this VM (Cosmos 3.0.97):

- Display: `720x1440 virtio-gpu`.
- CPU: one core online. Cosmos 3.0.97 counts a single CPU on ARM64 (`ARM64PlatformInitializer.GetCpuCount` returns 1), so the three others are never started.
- Memory: 2836 MB. Cosmos's page allocator takes the largest usable region of the memory map, not all of the 3072 MB.
- Storage: `vblk0`, 32 GB.
- Touch: `virtio-mouse`. Cosmos's pointer manager takes the buttons of an absolute device and not its position (`MouseManager.HandleAbsolute`), so a tablet or a touch device would not move the pointer: the VM uses a relative mouse.

## What the real phone needs

None of this is started. The VM proves the kernel, the shell and its controls, not the hardware. Booting on a PinePhone needs at least:

- a boot path: the phone starts U-Boot and hands over a device tree, where the VM gives UEFI and ACPI;
- a serial driver for the A64 UART, which is not the PL011 of the VM;
- the display pipeline of the A64 (display engine, MIPI DSI) and the XBD599 panel;
- an I2C driver and the Goodix GT917S on top of it;
- the A64 MMC controller for the eMMC and the microSD card;
- the AXP803 for power, the battery level and the power button.
