// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Aura.UI;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// A row of a list: a title on the left and, on the right, a value in a dimmer color. A row
/// with a Click lights up under the finger and shows an arrow; a row without one only shows.
/// Rows are stacked on a Card, each drawing the line that separates it from the next.
/// </summary>
internal sealed class ListRow : TouchButton
{
    public const int RowHeight = 80;

    private const int Padding = 20;
    private const int ArrowSize = 10;
    private const int ArrowThickness = 3;

    private string _value;

    /// <summary>The text on the right of the row.</summary>
    public string Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
                MarkDirty();
            }
        }
    }

    /// <summary>False for the last row of a card, which has no row under it to be separated from.</summary>
    public bool ShowSeparator { get; set; } = true;

    public ListRow(string title, string value, int x, int y, int width) : base(title, x, y, width, RowHeight)
    {
        _value = value;
        Radius = Card.Radius;
    }

    public override void Update()
    {
        // A row nobody listens to does not answer the finger.
        if (Click is not null)
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
            Shapes.FillRoundedRectangle(buffer, Palette.SurfacePressed, 0, 0, Width, Height, Radius);
        }

        int textY = (Height - Gui.Font.Height) / 2;
        DrawString(Text, Gui.Font, Palette.Text, Padding, textY);

        int right = Width - Padding;
        if (Click is not null)
        {
            DrawArrow(buffer, right - ArrowSize, Height / 2);
            right -= ArrowSize + Padding;
        }

        DrawString(_value, Gui.Font, Palette.DimText, right - Gui.Font.MeasureString(_value), textY);

        if (ShowSeparator)
        {
            buffer.DrawFilledRectangle(Palette.Separator, Padding, Height - 1, Width - (Padding * 2), 1);
        }
    }

    /// <summary>An arrow head pointing right, its tip at the right end of the row's content.</summary>
    private static void DrawArrow(Canvas buffer, int x, int centerY)
    {
        for (int i = 0; i < ArrowThickness; i++)
        {
            buffer.DrawLine(Palette.DimText, x + i, centerY - ArrowSize, x + ArrowSize + i, centerY);
            buffer.DrawLine(Palette.DimText, x + ArrowSize + i, centerY, x + i, centerY + ArrowSize);
        }
    }
}
