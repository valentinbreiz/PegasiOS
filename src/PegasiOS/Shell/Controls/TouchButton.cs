// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Aura.UI;
using Aura.UI.Components;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// A button for a finger: a rounded rectangle that lights up while it is pressed and calls
/// Click when the finger lifts on it. The kit's Button clicks when the press starts, which
/// suits a mouse; a tap is a press and a release that both land on the control, so a finger
/// that slides off cancels it. The other tappable controls of the shell derive from this one.
/// </summary>
internal class TouchButton : Button
{
    private bool _held;

    public Color Fill { get; set; } = Palette.Accent;

    public Color PressedFill { get; set; } = Palette.AccentPressed;

    public int Radius { get; set; } = 16;

    /// <summary>True while a press that started on the control is still on it.</summary>
    protected bool IsPressed => State == State.Pressed;

    public TouchButton(string? text, int x, int y, int width, int height) : base(text, x, y, width, height)
    {
        TextColor = Palette.Text;
    }

    /// <summary>The click of the kit comes with the press. A tap waits for the release, in Update.</summary>
    public override void HandleLeftClick()
    {
    }

    public override void Update()
    {
        PointerManager pointer = Gui.Pointer;
        bool inside = IsInside(pointer.X, pointer.Y);

        if (pointer.IsLeftPressOn(this))
        {
            _held = true;
        }
        else if (_held && !pointer.IsLeftButtonDown)
        {
            // A tap is the release seen on the control. A control hidden while it was held (Back
            // from a key) comes back with the button already up: that one is not a tap.
            _held = false;
            if (inside && pointer.IsLeftButtonReleased)
            {
                OnTap();
            }
        }

        State next = _held && inside ? State.Pressed : State.Normal;
        if (next != State)
        {
            State = next;
            MarkDirty();
        }
    }

    /// <summary>The finger lifted on the control. Calls Click.</summary>
    protected virtual void OnTap()
    {
        Click?.Invoke();
    }

    public override void Draw()
    {
        Clear(Color.Transparent);
        Shapes.FillRoundedRectangle(GetBuffer(), IsPressed ? PressedFill : Fill, 0, 0, Width, Height, Radius);

        if (Text is not null)
        {
            int textX = (Width - Gui.Font.MeasureString(Text)) / 2;
            int textY = (Height - Gui.Font.Height) / 2;
            DrawString(Text, Gui.Font, TextColor, textX, textY);
        }
    }
}
