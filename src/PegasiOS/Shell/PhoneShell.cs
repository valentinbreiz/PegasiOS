// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using Aura.UI;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;
using PegasiOS.Hardware;
using PegasiOS.Shell.Controls;
using PegasiOS.Shell.Screens;

namespace PegasiOS.Shell;

/// <summary>
/// The phone's interface: the status bar, the screen shown and the navigation bar, each a root
/// component of the UI kit, and the history of screens Back walks. The kernel calls Update once
/// per pass and Draw when Update says the picture changed.
/// </summary>
internal sealed class PhoneShell
{
    private const int TouchRadius = 22;
    private const int RingThickness = 3;
    private const int PressedRingThickness = 8;

    private readonly Canvas _canvas;
    private readonly StatusBar _statusBar;
    private readonly NavigationBar _navigationBar;

    /// <summary>The screens opened, in order; the last one is shown. The home screen is always first.</summary>
    private readonly List<Screen> _history = [];

    private int _minute = -1;
    private int _pointerX = -1;
    private int _pointerY = -1;
    private bool _pressed;
    private bool _drawn;

    public int Width { get; }

    public int Height { get; }

    /// <summary>Where a screen starts, under the status bar.</summary>
    public int ContentTop => StatusBar.BarHeight;

    /// <summary>The height a screen has between the two bars.</summary>
    public int ContentHeight => Height - StatusBar.BarHeight - NavigationBar.BarHeight;

    private Screen Current => _history[_history.Count - 1];

    public PhoneShell(Canvas canvas, DeviceFact[] facts)
    {
        _canvas = canvas;
        Width = canvas.Width;
        Height = canvas.Height;

        _statusBar = new StatusBar(Width);
        _navigationBar = new NavigationBar(Height - NavigationBar.BarHeight, Width)
        {
            Back = Back,
            Home = GoHome
        };
        Gui.BottomInset = NavigationBar.BarHeight;

        Screen[] apps = [new AboutScreen(this, facts), new ControlsScreen(this)];
        foreach (Screen app in apps)
        {
            app.Visible = false;
        }

        _history.Add(new HomeScreen(this, apps));
        _navigationBar.CanGoBack = false;

        KeyboardInput.Filter = OnKey;
    }

    /// <summary>Shows a screen over the current one. Back returns to the current one.</summary>
    public void Open(Screen screen)
    {
        Screen current = Current;
        if (ReferenceEquals(screen, current))
        {
            return;
        }

        current.Visible = false;

        // A screen is in the history once: opening it again brings it to the end.
        _history.Remove(screen);
        _history.Add(screen);
        Show(screen);
    }

    /// <summary>Returns to the screen the current one was opened from. Nothing on the home screen.</summary>
    public void Back()
    {
        if (_history.Count <= 1)
        {
            return;
        }

        Current.Visible = false;
        _history.RemoveAt(_history.Count - 1);
        Show(Current);
    }

    /// <summary>Returns to the home screen, whatever was open.</summary>
    public void GoHome()
    {
        if (_history.Count <= 1)
        {
            return;
        }

        Current.Visible = false;
        _history.RemoveRange(1, _history.Count - 1);
        Show(Current);
    }

    /// <summary>
    /// Takes the pointer and the keys, lets the bars and the screen shown react, and moves the clock.
    /// </summary>
    /// <returns>True when the picture changed and Draw has to run.</returns>
    public bool Update(DateTime now)
    {
        PointerManager pointer = Gui.Pointer;
        pointer.Update();
        KeyboardInput.Update();

        _statusBar.Update();
        Current.Update();
        _navigationBar.Update();

        // Keys no control of this screen read are dropped: no later frame would want them.
        while (KeyboardInput.TryGetKey(out _))
        {
        }

        if (now.Minute != _minute)
        {
            _minute = now.Minute;
            _statusBar.Time = Clock.Time(now);
        }

        Current.Tick(now);

        // Not a short-circuit: each call marks its container.
        bool changed = !_drawn;
        changed |= _statusBar.RefreshDirty();
        changed |= Current.RefreshDirty();
        changed |= _navigationBar.RefreshDirty();

        if (pointer.X != _pointerX || pointer.Y != _pointerY || pointer.IsLeftButtonDown != _pressed)
        {
            _pointerX = pointer.X;
            _pointerY = pointer.Y;
            _pressed = pointer.IsLeftButtonDown;
            changed = true;
        }

        return changed;
    }

    /// <summary>Composes the bars and the screen, marks where the finger is and shows the frame.</summary>
    public void Draw()
    {
        Gui.Windows.DrawWindows();
        DrawTouch();
        _canvas.Display();
        _drawn = true;
    }

    private void Show(Screen screen)
    {
        screen.Visible = true;
        screen.MarkDirty();
        _navigationBar.CanGoBack = _history.Count > 1;

        Log.WriteString("[PegasiOS] screen ");
        Log.WriteString(screen.Name);
        Log.WriteString("\n");
    }

    /// <summary>Escape is Back, for a keyboard on the dock. The other keys go to the controls.</summary>
    private bool OnKey(KeyEvent key)
    {
        if (key.Key != Key.Escape)
        {
            return false;
        }

        Back();
        return true;
    }

    /// <summary>
    /// Marks the pointer, which stands in for the finger on a machine without a touchscreen:
    /// a ring, thicker while it presses. A ring and not a disc, so the control under the
    /// finger stays visible.
    /// </summary>
    private void DrawTouch()
    {
        int thickness = _pressed ? PressedRingThickness : RingThickness;
        for (int i = 0; i < thickness; i++)
        {
            _canvas.DrawCircle(Palette.Text, _pointerX, _pointerY, TouchRadius - i);
        }
    }
}
