// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Aura.UI.Components;

namespace PegasiOS.Shell.Controls;

/// <summary>A rounded panel that groups rows and controls on a screen.</summary>
internal sealed class Card : Container
{
    public const int Radius = 18;

    public Card(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }

    /// <summary>
    /// A card of rows, one under the other, as wide as the card and as tall as its rows.
    /// </summary>
    public static Card OfRows(int width, params ListRow[] rows)
    {
        Card card = new(0, 0, width, rows.Length * ListRow.RowHeight);
        for (int i = 0; i < rows.Length; i++)
        {
            ListRow row = rows[i];
            row.Y = i * ListRow.RowHeight;
            row.ShowSeparator = i < rows.Length - 1;
            card.AddChild(row);
        }

        return card;
    }

    protected override void DrawBackground()
    {
        Shapes.FillRoundedRectangle(GetBuffer(), Palette.Surface, 0, 0, Width, Height, Radius);
    }

    /// <summary>Puts a control on the right of a row of the card, centered on the row's height.</summary>
    public void AddAccessory(ListRow row, Component accessory, int rightPadding)
    {
        accessory.X = Width - rightPadding - accessory.Width;
        accessory.Y = row.Y + ((row.Height - accessory.Height) / 2);
        AddChild(accessory);
    }
}
