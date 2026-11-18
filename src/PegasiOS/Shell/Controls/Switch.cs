// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>An on/off switch: a tap moves the knob to the other side.</summary>
internal sealed class Switch : TouchButton
{
    public const int TrackWidth = 96;
    public const int TrackHeight = 52;

    /// <summary>Space between the knob and the edge of the track.</summary>
    private const int KnobInset = 6;

    private bool _checked;

    /// <summary>Called after a tap turned the switch on or off.</summary>
    public Action? Changed { get; set; }

    /// <summary>Setting it does not call Changed.</summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (value != _checked)
            {
                _checked = value;
                MarkDirty();
            }
        }
    }

    public Switch(int x, int y, bool isChecked = false) : base(null, x, y, TrackWidth, TrackHeight)
    {
        _checked = isChecked;
    }

    protected override void OnTap()
    {
        Checked = !_checked;
        Changed?.Invoke();
    }

    public override void Draw()
    {
        Clear(Color.Transparent);

        Canvas buffer = GetBuffer();
        Color track = _checked ? Palette.Accent : Palette.Off;
        Shapes.FillRoundedRectangle(buffer, IsPressed ? Palette.AccentPressed : track, 0, 0, Width, Height, Height / 2);

        int center = Height / 2;
        int knobX = _checked ? Width - center - 1 : center;
        buffer.DrawFilledCircle(Palette.Text, knobX, center, center - KnobInset);
    }
}
