// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Aura.UI;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// An entry of the home screen: a rounded square with the first letter of the name, and the
/// name under it. A tap opens what it stands for.
/// </summary>
internal sealed class AppTile : TouchButton
{
    public const int TileWidth = 176;
    public const int TileHeight = 196;

    private const int IconSize = 128;
    private const int IconRadius = 28;
    private const int LetterScale = 2;
    private const int CaptionGap = 12;

    private readonly Canvas _letter;

    public AppTile(string name, Color color, int x, int y) : base(name, x, y, TileWidth, TileHeight)
    {
        Fill = color;

        // The letter at the font's size, stretched when the tile is drawn.
        string letter = name.Substring(0, 1);
        _letter = new Canvas(Gui.Font.MeasureString(letter), Gui.Font.Height);
        _letter.Clear(Color.Transparent);
        _letter.DrawString(letter, Gui.Font, Palette.Text, 0, 0);
    }

    public override void Draw()
    {
        Clear(Color.Transparent);

        Canvas buffer = GetBuffer();
        int iconX = (Width - IconSize) / 2;
        Shapes.FillRoundedRectangle(buffer, IsPressed ? PressedFill : Fill, iconX, 0, IconSize, IconSize, IconRadius);

        int letterWidth = _letter.Width * LetterScale;
        int letterHeight = _letter.Height * LetterScale;
        buffer.DrawCanvas(_letter, iconX + ((IconSize - letterWidth) / 2), (IconSize - letterHeight) / 2, letterWidth, letterHeight);

        DrawString(Text, Gui.Font, Palette.Text, (Width - Gui.Font.MeasureString(Text)) / 2, IconSize + CaptionGap);
    }
}
