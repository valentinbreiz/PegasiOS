// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using Aura.UI.Components;
using Aura.UI.Layout;
using PegasiOS.Shell.Controls;

namespace PegasiOS.Shell.Screens;

/// <summary>The first screen: the clock, the date and a tile for each screen that can be opened.</summary>
internal sealed class HomeScreen : Screen
{
    private const int ClockScale = 4;
    private const int ClockTopMargin = 40;
    private const int TilesTopMargin = 56;
    private const int TilesPerRow = 3;

    private static readonly Color[] s_tileColors =
    [
        Color.FromArgb(0xFF, 0x2F, 0x8F, 0xE8),
        Color.FromArgb(0xFF, 0x2E, 0xA8, 0x7E),
        Color.FromArgb(0xFF, 0xD9, 0x7A, 0x2B)
    ];

    private readonly ScaledLabel _clock;
    private readonly Label _date;
    private int _minute = -1;

    public HomeScreen(PhoneShell shell, Screen[] apps) : base(shell, "Home")
    {
        _clock = new ScaledLabel("00:00", Palette.Text, ClockScale, 0, 0);
        _date = new Label(string.Empty, Palette.DimText, 0, 0);
        Add(_clock, Alignment.Center, ClockTopMargin);
        Add(_date, Alignment.Center);

        // Tiles left to right, the first and the last of a row against the margins.
        int rows = (apps.Length + TilesPerRow - 1) / TilesPerRow;
        int gap = (ContentWidth - (AppTile.TileWidth * TilesPerRow)) / (TilesPerRow - 1);
        Container tiles = new(0, 0, ContentWidth, rows * AppTile.TileHeight);
        for (int i = 0; i < apps.Length; i++)
        {
            Screen app = apps[i];
            int x = i % TilesPerRow * (AppTile.TileWidth + gap);
            int y = i / TilesPerRow * AppTile.TileHeight;
            tiles.AddChild(new AppTile(app.Name, s_tileColors[i % s_tileColors.Length], x, y)
            {
                Click = () => shell.Open(app)
            });
        }

        Add(tiles, Alignment.Stretch, TilesTopMargin);
        Arrange();
    }

    public override void Tick(DateTime now)
    {
        if (now.Minute == _minute)
        {
            return;
        }

        _minute = now.Minute;
        _clock.Text = Clock.Time(now);

        string date = Clock.Date(now);
        if (date != _date.Text)
        {
            // Another day is another width: center it again.
            _date.Text = date;
            Arrange();
        }
    }
}
