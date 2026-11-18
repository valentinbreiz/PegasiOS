// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;

namespace PegasiOS.Shell;

/// <summary>The colors of the shell and of its controls.</summary>
internal static class Palette
{
    public static readonly Color Background = Color.FromArgb(0xFF, 0x0B, 0x12, 0x20);
    public static readonly Color Bar = Color.FromArgb(0xFF, 0x07, 0x0C, 0x16);
    public static readonly Color Surface = Color.FromArgb(0xFF, 0x16, 0x21, 0x3A);
    public static readonly Color SurfacePressed = Color.FromArgb(0xFF, 0x26, 0x38, 0x5E);
    public static readonly Color Separator = Color.FromArgb(0xFF, 0x26, 0x33, 0x52);
    public static readonly Color Text = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
    public static readonly Color DimText = Color.FromArgb(0xFF, 0x9F, 0xB0, 0xC8);
    public static readonly Color Accent = Color.FromArgb(0xFF, 0x2F, 0x8F, 0xE8);
    public static readonly Color AccentPressed = Color.FromArgb(0xFF, 0x7C, 0xC4, 0xFF);

    /// <summary>The track of a switch that is off, and the part of a slider's rail past its knob.</summary>
    public static readonly Color Off = Color.FromArgb(0xFF, 0x4A, 0x56, 0x6E);
}
