// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Collections.Generic;
using Cosmos.Kernel.HAL.Devices.Storage;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Storage;
using Sys = Cosmos.Kernel.System;

namespace PegasiOS.Hardware;

/// <summary>One line of the device card: what it is and what the kernel found.</summary>
internal readonly struct DeviceFact
{
    public string Label { get; }

    public string Value { get; }

    public DeviceFact(string label, string value)
    {
        Label = label;
        Value = value;
    }
}

/// <summary>
/// What the running kernel sees of the machine. Every value is read from the
/// kernel at boot, none is copied from the PinePhone's data sheet: the card
/// shows what was found, so it tells a wrong VM setup from a right one.
/// </summary>
internal static class DeviceFacts
{
    private const ulong BytesPerMegabyte = 1024UL * 1024UL;
    private const ulong BytesPerGigabyte = 1024UL * 1024UL * 1024UL;

    public static DeviceFact[] Read(Canvas canvas)
    {
        List<DeviceFact> facts = new(6);

        DisplayDevice? display = DisplayManager.Primary;
        string panel = canvas.Width + "x" + canvas.Height;
        facts.Add(new DeviceFact("Display", display is null ? panel : panel + " " + display.Name));

        uint cpus = SchedulerDiagnostics.CpuCount;
        facts.Add(new DeviceFact("CPU", cpus + (cpus == 1 ? " core online" : " cores online")));

        facts.Add(new DeviceFact("Memory", (MemoryDiagnostics.RamSizeBytes / BytesPerMegabyte) + " MB"));

        IBlockDevice? disk = StorageManager.PrimaryDevice;
        facts.Add(new DeviceFact("Storage", disk is null
            ? "none"
            : disk.Name + " " + (disk.BlockCount * disk.BlockSize / BytesPerGigabyte) + " GB"));

        facts.Add(new DeviceFact("Touch", FindPointer() ?? "none"));
        facts.Add(new DeviceFact("Kernel", "Cosmos " + Sys.Kernel.VersionString));

        return facts.ToArray();
    }

    /// <summary>The name of the first pointer a driver published, or null without one.</summary>
    private static string? FindPointer()
    {
        int count = DriverDiagnostics.DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (DriverDiagnostics.TryGetDevice(i, out PublishedDeviceInfo info)
                && info.Kind == PublishedDeviceKind.Pointer
                && !info.IsWithdrawn)
            {
                return info.Name;
            }
        }

        return null;
    }
}
