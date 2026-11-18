// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using Aura.UI;
using Aura.UI.Components;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// A horizontal slider with a knob large enough for a finger, Value from 0 to Maximum. A press
/// anywhere on it takes the knob, which then follows the finger until it lifts, on or off the
/// slider.
/// </summary>
internal sealed class TouchSlider : Component
{
    public const int Maximum = 100;
    public const int SliderHeight = 56;

    private const int KnobRadius = 22;
    private const int RailHeight = 8;

    private int _value;
    private bool _dragging;

    /// <summary>Called when the finger moved the knob to another value.</summary>
    public Action? Changed { get; set; }

    /// <summary>Setting it does not call Changed.</summary>
    public int Value
    {
        get => _value;
        set
        {
            int clamped = Math.Clamp(value, 0, Maximum);
            if (clamped != _value)
            {
                _value = clamped;
                MarkDirty();
            }
        }
    }

    public TouchSlider(int x, int y, int width) : base(x, y, width, SliderHeight)
    {
    }

    public override void Update()
    {
        PointerManager pointer = Gui.Pointer;
        bool wasDragging = _dragging;

        if (pointer.IsLeftPressOn(this))
        {
            _dragging = true;
        }
        else if (!pointer.IsLeftButtonDown)
        {
            _dragging = false;
        }

        if (_dragging != wasDragging)
        {
            MarkDirty();
        }

        // The knob's center travels from one radius in to one radius short of the end.
        int travel = Width - (KnobRadius * 2);
        if (!_dragging || travel <= 0)
        {
            return;
        }

        int position = Math.Clamp(pointer.X - AbsoluteX - KnobRadius, 0, travel);
        int value = ((position * Maximum) + (travel / 2)) / travel;
        if (value != _value)
        {
            Value = value;
            Changed?.Invoke();
        }
    }

    public override void Draw()
    {
        Clear(Color.Transparent);

        Canvas buffer = GetBuffer();
        int travel = Math.Max(0, Width - (KnobRadius * 2));
        int knobX = KnobRadius + (travel * _value / Maximum);
        int railY = (Height - RailHeight) / 2;

        Shapes.FillRoundedRectangle(buffer, Palette.Off, 0, railY, Width, RailHeight, RailHeight / 2);
        Shapes.FillRoundedRectangle(buffer, Palette.Accent, 0, railY, knobX, RailHeight, RailHeight / 2);
        buffer.DrawFilledCircle(_dragging ? Palette.AccentPressed : Palette.Text, knobX, Height / 2, KnobRadius);
    }
}
