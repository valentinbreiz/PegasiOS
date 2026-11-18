// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;

namespace PegasiOS.Shell;

/// <summary>
/// The time and the date as the shell writes them. Written out by hand: the kernel has one
/// culture, and the names it would give are the same ones.
/// </summary>
internal static class Clock
{
    private static readonly string[] s_days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] s_months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    /// <summary>Hours and minutes on 24 hours, two digits each: "09:41".</summary>
    public static string Time(DateTime now)
    {
        return $"{TwoDigits(now.Hour)}:{TwoDigits(now.Minute)}";
    }

    /// <summary>The day in full: "Wednesday 18 November 2026".</summary>
    public static string Date(DateTime now)
    {
        return $"{s_days[(int)now.DayOfWeek]} {now.Day} {s_months[now.Month - 1]} {now.Year}";
    }

    private static string TwoDigits(int value)
    {
        return value < 10 ? "0" + value : value.ToString();
    }
}
