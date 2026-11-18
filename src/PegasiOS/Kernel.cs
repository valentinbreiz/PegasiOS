// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Aura.UI;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Input;
using Cosmos.Kernel.System.Timers;
using PegasiOS.Hardware;
using PegasiOS.Shell;
using Sys = Cosmos.Kernel.System;

namespace PegasiOS;

/// <summary>
/// The PegasiOS kernel. Takes the phone panel, starts the UI kit on it and runs the shell:
/// one pass reads the pointer and the keys, and a frame is drawn when something changed.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>How long the loop sleeps when a pass had nothing to draw.</summary>
    private const uint IdleWaitMs = 10;

    /// <summary>
    /// How often the loop asks for a collection. The collector otherwise runs only as the last
    /// resort of an allocation that found no memory: the loop collects on a period instead, so
    /// the garbage of the frames does not pile up until then.
    /// </summary>
    private const long CollectPeriodMs = 2000;

    private PhoneShell? _shell;

    protected override void BeforeRun()
    {
        try
        {
            Canvas canvas = Canvas.GetFullScreen();

            // The pointer stands in for the finger: it moves over the panel, nowhere else.
            MouseManager.SetScreenSize(canvas.Width, canvas.Height);
            MouseManager.SetPosition(canvas.Width / 2, canvas.Height / 2);

            Gui.Initialize(canvas, PCScreenFont.DefaultFont);
            _shell = new PhoneShell(canvas, DeviceFacts.Read(canvas));

            Log.WriteString("[PegasiOS] panel ");
            Log.WriteNumber(canvas.Width);
            Log.WriteString("x");
            Log.WriteNumber(canvas.Height);
            Log.WriteString("\n");
        }
        catch (Exception ex)
        {
            // An exception that leaves BeforeRun is not caught by the base kernel.
            ReportFailure("start", ex);
        }
    }

    protected override void Run()
    {
        PhoneShell? shell = _shell;
        if (shell is null)
        {
            Stop();
            return;
        }

        // The base kernel writes two serial lines around every Run() call, so
        // the frame loop stays inside one call. It ends when the kernel stops.
        bool announced = false;
        long lastCollect = Environment.TickCount64;
        try
        {
            while (!Stopped)
            {
                long now = Environment.TickCount64;
                if (now - lastCollect >= CollectPeriodMs)
                {
                    MemoryDiagnostics.Collect();
                    lastCollect = now;
                }

                if (!shell.Update(DateTime.Now))
                {
                    TimerManager.Wait(IdleWaitMs);
                    continue;
                }

                shell.Draw();

                if (!announced)
                {
                    announced = true;
                    Log.WriteString("[PegasiOS] ready\n");
                }
            }
        }
        catch (Exception ex)
        {
            ReportFailure("frame", ex);
            Stop();
        }
    }

    private static void ReportFailure(string stage, Exception ex)
    {
        Log.WriteString("[PegasiOS] ");
        Log.WriteString(stage);
        Log.WriteString(" failed: ");
        Log.WriteString(ex.Message);
        Log.WriteString("\n");
    }
}
