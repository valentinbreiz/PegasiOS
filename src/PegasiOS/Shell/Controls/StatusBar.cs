// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Aura.UI;
using Aura.UI.Components;

namespace PegasiOS.Shell.Controls;

/// <summary>The bar at the top of the screen: the time on the left, the system's name on the right.</summary>
internal sealed class StatusBar : Container
{
    public const int BarHeight = 56;

    private const int Padding = 24;
    private const string SystemName = "PegasiOS";

    private readonly Label _time;

    /// <summary>The time shown, as text ("09:41").</summary>
    public string Time
    {
        get => _time.Text;
        set => _time.Text = value;
    }

    public StatusBar(int width) : base(0, 0, width, BarHeight)
    {
        Background = Palette.Bar;

        int textY = (BarHeight - Gui.Font.Height) / 2;
        _time = new Label(string.Empty, Palette.Text, Padding, textY);
        AddChild(_time);

        int nameX = width - Padding - Gui.Font.MeasureString(SystemName);
        AddChild(new Label(SystemName, Palette.DimText, nameX, textY));
    }
}
