// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Aura.UI;
using Aura.UI.Components;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// One line of text drawn larger than the font: each pixel of the font becomes a square of
/// Scale pixels. For a clock and a screen's title, until the shell has a font of each size.
/// </summary>
internal sealed class ScaledLabel : Component
{
    private readonly int _scale;
    private readonly Color _color;
    private string _text;

    /// <summary>The label takes the size of its text.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (value == _text)
            {
                return;
            }

            _text = value;
            if (PreferredWidth != Width)
            {
                SetSize(PreferredWidth, PreferredHeight);
            }

            MarkDirty();
        }
    }

    public override int PreferredWidth => Gui.Font.MeasureString(_text) * _scale;

    public override int PreferredHeight => Gui.Font.Height * _scale;

    public ScaledLabel(string text, Color color, int scale, int x, int y)
        : base(x, y, Gui.Font.MeasureString(text) * scale, Gui.Font.Height * scale)
    {
        _text = text;
        _color = color;
        _scale = scale;
    }

    /// <summary>The text looks the same under the pointer.</summary>
    public override void Update()
    {
    }

    public override void Draw()
    {
        Clear(Color.Transparent);
        if (_text.Length == 0)
        {
            return;
        }

        // The text at the font's size, then stretched onto the label.
        Canvas glyphs = new(Gui.Font.MeasureString(_text), Gui.Font.Height);
        glyphs.Clear(Color.Transparent);
        glyphs.DrawString(_text, Gui.Font, _color, 0, 0);
        GetBuffer().DrawCanvas(glyphs, 0, 0, Width, Height);
    }
}
