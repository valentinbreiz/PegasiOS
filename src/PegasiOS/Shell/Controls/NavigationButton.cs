// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>What a button of the navigation bar shows.</summary>
internal enum NavigationGlyph
{
    /// <summary>An arrow head pointing left.</summary>
    Back,

    /// <summary>A short horizontal pill.</summary>
    Home
}

/// <summary>A button of the navigation bar: a shape instead of a text, on the bar's own color.</summary>
internal sealed class NavigationButton : TouchButton
{
    private const int ArrowSize = 12;
    private const int ArrowThickness = 4;
    private const int PillWidth = 96;
    private const int PillHeight = 8;
    private const int PressedInset = 8;

    private readonly NavigationGlyph _glyph;
    private bool _enabled = true;

    /// <summary>A disabled button is dimmed and ignores the finger.</summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value != _enabled)
            {
                _enabled = value;
                MarkDirty();
            }
        }
    }

    public NavigationButton(NavigationGlyph glyph, int x, int y, int width, int height) : base(null, x, y, width, height)
    {
        _glyph = glyph;
    }

    public override void Update()
    {
        if (_enabled)
        {
            base.Update();
        }
    }

    public override void Draw()
    {
        Clear(Color.Transparent);

        Canvas buffer = GetBuffer();
        if (IsPressed)
        {
            Shapes.FillRoundedRectangle(buffer, Palette.SurfacePressed, PressedInset, PressedInset, Width - (PressedInset * 2), Height - (PressedInset * 2), Radius);
        }

        Color color = _enabled ? Palette.Text : Palette.Off;
        int centerX = Width / 2;
        int centerY = Height / 2;

        if (_glyph == NavigationGlyph.Home)
        {
            Shapes.FillRoundedRectangle(buffer, color, centerX - (PillWidth / 2), centerY - (PillHeight / 2), PillWidth, PillHeight, PillHeight / 2);
            return;
        }

        int tipX = centerX - (ArrowSize / 2);
        for (int i = 0; i < ArrowThickness; i++)
        {
            buffer.DrawLine(color, tipX + ArrowSize + i, centerY - ArrowSize, tipX + i, centerY);
            buffer.DrawLine(color, tipX + i, centerY, tipX + ArrowSize + i, centerY + ArrowSize);
        }
    }
}
