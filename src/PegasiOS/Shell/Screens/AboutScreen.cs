// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using PegasiOS.Hardware;
using PegasiOS.Shell.Controls;

namespace PegasiOS.Shell.Screens;

/// <summary>
/// What the kernel found of the machine at boot, one row per fact, and under it who the
/// wallpaper is credited to.
/// </summary>
internal sealed class AboutScreen : Screen
{
    public AboutScreen(PhoneShell shell, DeviceFact[] facts) : base(shell, "About")
    {
        AddTitle(Name);

        ListRow[] rows = new ListRow[facts.Length];
        for (int i = 0; i < facts.Length; i++)
        {
            rows[i] = new ListRow(facts[i].Label, facts[i].Value, 0, 0, ContentWidth);
        }

        Add(Card.OfRows(ContentWidth, rows));
        Add(Card.OfRows(ContentWidth, new ListRow("Wallpaper", Wallpaper.Credit, 0, 0, ContentWidth)));
        Arrange();
    }
}
