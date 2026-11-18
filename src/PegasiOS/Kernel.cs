// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Timers;
using PegasiOS.Hardware;
using PegasiOS.Shell;
using Sys = Cosmos.Kernel.System;

namespace PegasiOS;

/// <summary>
/// The PegasiOS kernel. Takes the phone panel, then runs the demo screen:
/// it reads the pointer as a finger and redraws when something changed.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>How long the loop sleeps when a pass had nothing to draw.</summary>
    private const uint IdleWaitMs = 10;

    private Canvas? _canvas;
    private DemoScreen? _screen;

    protected override void BeforeRun()
    {
        Canvas canvas = Canvas.GetFullScreen();

        // The pointer stands in for the finger: it moves over the panel, nowhere else.
        MouseManager.SetScreenSize(canvas.Width, canvas.Height);
        MouseManager.SetPosition(canvas.Width / 2, canvas.Height / 2);

        _canvas = canvas;
        _screen = new DemoScreen(canvas.Width, canvas.Height, DeviceFacts.Read(canvas));

        Log.WriteString("[PegasiOS] panel ");
        Log.WriteNumber(canvas.Width);
        Log.WriteString("x");
        Log.WriteNumber(canvas.Height);
        Log.WriteString("\n");
    }

    protected override void Run()
    {
        Canvas? canvas = _canvas;
        DemoScreen? screen = _screen;
        if (canvas is null || screen is null)
        {
            Stop();
            return;
        }

        // The base kernel writes two serial lines around every Run() call, so
        // the frame loop stays inside one call. It ends when the kernel stops.
        bool announced = false;
        int taps = 0;
        while (!Stopped)
        {
            bool changed = screen.Update(MouseManager.X, MouseManager.Y, MouseManager.LeftButton, DateTime.Now);
            if (!changed)
            {
                TimerManager.Wait(IdleWaitMs);
                continue;
            }

            screen.Draw(canvas);
            canvas.Display();

            if (screen.Taps != taps)
            {
                taps = screen.Taps;
                Log.WriteString("[PegasiOS] taps ");
                Log.WriteNumber(taps);
                Log.WriteString("\n");
            }

            if (!announced)
            {
                announced = true;
                Log.WriteString("[PegasiOS] ready\n");
            }
        }
    }
}
