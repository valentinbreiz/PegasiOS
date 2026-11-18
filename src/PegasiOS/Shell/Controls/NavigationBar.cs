// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// The bar at the bottom of the screen: Back on the left, Home in the middle. Each takes a
/// third of the bar, so the finger does not have to aim.
/// </summary>
internal sealed class NavigationBar : Container
{
    public const int BarHeight = 72;

    private readonly NavigationButton _back;
    private readonly NavigationButton _home;

    /// <summary>Called on a tap on Back.</summary>
    public Action? Back
    {
        get => _back.Click;
        set => _back.Click = value;
    }

    /// <summary>Called on a tap on Home.</summary>
    public Action? Home
    {
        get => _home.Click;
        set => _home.Click = value;
    }

    /// <summary>Back is dimmed while there is no screen to go back to.</summary>
    public bool CanGoBack
    {
        get => _back.Enabled;
        set => _back.Enabled = value;
    }

    public NavigationBar(int y, int width) : base(0, y, width, BarHeight)
    {
        Background = Palette.Bar;

        int third = width / 3;
        _back = new NavigationButton(NavigationGlyph.Back, 0, 0, third, BarHeight);
        _home = new NavigationButton(NavigationGlyph.Home, third, 0, width - (third * 2), BarHeight);
        AddChild(_back);
        AddChild(_home);
    }
}
