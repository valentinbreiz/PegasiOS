// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Aura.UI.Components;
using Aura.UI.Layout;
using Cosmos.Kernel.System.Diagnostics;
using PegasiOS.Shell.Controls;

namespace PegasiOS.Shell.Screens;

/// <summary>
/// The shell's controls, each one working: a switch, a slider and a button, with a row that
/// shows what each last did. Under them, a button and a text box of the UI kit as Aura draws
/// them, to show that the same kit carries both.
/// </summary>
internal sealed class ControlsScreen : Screen
{
    private const int ButtonHeight = 88;
    private const int KitButtonHeight = 72;
    private const int KitTextBoxHeight = 48;
    private const int AccessoryPadding = 20;
    private const int StartingSliderValue = 40;

    private readonly ListRow _switchRow;
    private readonly ListRow _sliderRow;
    private readonly ListRow _tapsRow;
    private readonly Switch _switch;
    private readonly TouchSlider _slider;
    private int _taps;

    public ControlsScreen(PhoneShell shell) : base(shell, "Controls")
    {
        AddTitle(Name);

        _switchRow = new ListRow("Switch", string.Empty, 0, 0, ContentWidth);
        _sliderRow = new ListRow("Slider", StartingSliderValue.ToString(), 0, 0, ContentWidth);
        _tapsRow = new ListRow("Taps", "0", 0, 0, ContentWidth);

        Card card = Card.OfRows(ContentWidth, _switchRow, _sliderRow, _tapsRow);
        _switch = new Switch(0, 0) { Changed = OnSwitchChanged };
        card.AddAccessory(_switchRow, _switch, AccessoryPadding);
        Add(card);

        _slider = new TouchSlider(0, 0, ContentWidth) { Value = StartingSliderValue, Changed = OnSliderChanged };
        Add(_slider);

        Add(new TouchButton("Tap", 0, 0, ContentWidth, ButtonHeight) { Click = OnTap });

        Add(new Label("From Aura's kit, as it draws them:", Palette.DimText, 0, 0), Alignment.Start);
        Add(new Button("Aura button", 0, 0, ContentWidth, KitButtonHeight) { Click = OnKitButton });
        Add(new TextBox(0, 0, ContentWidth, KitTextBoxHeight, "Text box"));

        Arrange();
    }

    private void OnSwitchChanged()
    {
        Log.WriteString(_switch.Checked ? "[PegasiOS] switch on\n" : "[PegasiOS] switch off\n");
    }

    private void OnSliderChanged()
    {
        _sliderRow.Value = _slider.Value.ToString();

        Log.WriteString("[PegasiOS] slider ");
        Log.WriteNumber(_slider.Value);
        Log.WriteString("\n");
    }

    private void OnTap()
    {
        _taps++;
        _tapsRow.Value = _taps.ToString();

        Log.WriteString("[PegasiOS] taps ");
        Log.WriteNumber(_taps);
        Log.WriteString("\n");
    }

    private static void OnKitButton()
    {
        Log.WriteString("[PegasiOS] kit button\n");
    }
}
