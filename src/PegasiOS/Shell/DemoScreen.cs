// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using PegasiOS.Hardware;

namespace PegasiOS.Shell;

/// <summary>
/// The first screen of PegasiOS: a status bar, the clock, a card with what the
/// kernel found of the machine, and a round button that counts taps. It is
/// drawn straight on the canvas; the UI kit replaces it.
/// </summary>
internal sealed class DemoScreen
{
    private const int Margin = 24;
    private const int StatusBarHeight = 56;
    private const int ClockScale = 4;
    private const int ClockTopGap = 64;
    private const int DateTopGap = 16;
    private const int CardTopGap = 56;
    private const int CardPadding = 20;
    private const int CardRadius = 18;
    private const int RowGap = 12;
    private const int TapRadius = 120;
    private const int TapLabelScale = 2;
    private const int TouchRadius = 22;
    private const int TouchRingThickness = 3;
    private const int NavPillWidth = 220;
    private const int NavPillHeight = 8;
    private const int NavPillBottomGap = 24;

    private static readonly Color s_background = Color.FromArgb(0xFF, 0x0B, 0x12, 0x20);
    private static readonly Color s_statusBar = Color.FromArgb(0xFF, 0x07, 0x0C, 0x16);
    private static readonly Color s_card = Color.FromArgb(0xFF, 0x16, 0x21, 0x3A);
    private static readonly Color s_text = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
    private static readonly Color s_dimText = Color.FromArgb(0xFF, 0x9F, 0xB0, 0xC8);
    private static readonly Color s_accent = Color.FromArgb(0xFF, 0x2F, 0x8F, 0xE8);
    private static readonly Color s_accentPressed = Color.FromArgb(0xFF, 0x7C, 0xC4, 0xFF);

    private static readonly string[] s_days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    private static readonly string[] s_months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    private readonly int _width;
    private readonly int _height;
    private readonly DeviceFact[] _facts;
    private readonly Font _font;
    private readonly Canvas _tapLabel;

    private readonly int _cardTop;
    private readonly int _cardHeight;
    private readonly int _tapCenterY;

    private int _pointerX;
    private int _pointerY;
    private bool _pressed;
    private bool _pressStartedOnButton;
    private int _taps;
    private int _minute = -1;
    private string _date = string.Empty;
    private Canvas? _clock;
    private bool _drawn;

    public DemoScreen(int width, int height, DeviceFact[] facts)
    {
        _width = width;
        _height = height;
        _facts = facts;
        _font = PCScreenFont.DefaultFont;
        _tapLabel = RenderText("TAP", s_text);

        int rowHeight = _font.Height + RowGap;
        int clockBottom = StatusBarHeight + ClockTopGap + (_font.Height * ClockScale);
        _cardTop = clockBottom + DateTopGap + _font.Height + CardTopGap;
        _cardHeight = (CardPadding * 2) + (rowHeight * (facts.Length + 1));

        // The button sits halfway between the card and the navigation pill.
        int below = _cardTop + _cardHeight;
        int navTop = height - NavPillBottomGap - NavPillHeight;
        _tapCenterY = below + ((navTop - below) / 2) - _font.Height;
    }

    /// <summary>The number of taps the button counted.</summary>
    public int Taps => _taps;

    /// <summary>
    /// Takes the finger's position and state and the time.
    /// </summary>
    /// <returns>True when the screen no longer shows what it should and must be drawn again.</returns>
    public bool Update(int pointerX, int pointerY, bool pressed, DateTime now)
    {
        bool changed = !_drawn;

        if (pointerX != _pointerX || pointerY != _pointerY)
        {
            _pointerX = pointerX;
            _pointerY = pointerY;
            changed = true;
        }

        if (pressed != _pressed)
        {
            // A tap is a press and a release that both land on the button.
            bool onButton = IsOnTapButton(pointerX, pointerY);
            if (pressed)
            {
                _pressStartedOnButton = onButton;
            }
            else
            {
                if (_pressStartedOnButton && onButton)
                {
                    _taps++;
                }

                _pressStartedOnButton = false;
            }

            _pressed = pressed;
            changed = true;
        }

        if (now.Minute != _minute)
        {
            _minute = now.Minute;
            _clock = RenderText(TwoDigits(now.Hour) + ":" + TwoDigits(now.Minute), s_text);
            _date = s_days[(int)now.DayOfWeek] + " " + now.Day + " " + s_months[now.Month - 1] + " " + now.Year;
            changed = true;
        }

        return changed;
    }

    public void Draw(Canvas canvas)
    {
        canvas.Clear(s_background);
        DrawStatusBar(canvas);
        DrawClock(canvas);
        DrawDeviceCard(canvas);
        DrawTapButton(canvas);
        DrawNavPill(canvas);
        DrawTouch(canvas);
        _drawn = true;
    }

    private void DrawStatusBar(Canvas canvas)
    {
        canvas.DrawFilledRectangle(s_statusBar, 0, 0, _width, StatusBarHeight);

        int textY = (StatusBarHeight - _font.Height) / 2;
        const string Name = "PegasiOS";
        canvas.DrawString(Name, _font, s_text, Margin, textY);

        const string Device = "PinePhone";
        canvas.DrawString(Device, _font, s_dimText, _width - Margin - _font.MeasureString(Device), textY);
    }

    private void DrawClock(Canvas canvas)
    {
        int top = StatusBarHeight + ClockTopGap;
        if (_clock is Canvas clock)
        {
            int clockWidth = clock.Width * ClockScale;
            canvas.DrawCanvas(clock, (_width - clockWidth) / 2, top, clockWidth, clock.Height * ClockScale);
        }

        int dateY = top + (_font.Height * ClockScale) + DateTopGap;
        canvas.DrawString(_date, _font, s_dimText, (_width - _font.MeasureString(_date)) / 2, dateY);
    }

    private void DrawDeviceCard(Canvas canvas)
    {
        int cardWidth = _width - (Margin * 2);
        FillRoundedRectangle(canvas, s_card, Margin, _cardTop, cardWidth, _cardHeight, CardRadius);

        int rowHeight = _font.Height + RowGap;
        int left = Margin + CardPadding;
        int right = Margin + cardWidth - CardPadding;
        int y = _cardTop + CardPadding;

        canvas.DrawString("This device", _font, s_accentPressed, left, y);
        y += rowHeight;

        for (int i = 0; i < _facts.Length; i++)
        {
            DeviceFact fact = _facts[i];
            canvas.DrawString(fact.Label, _font, s_dimText, left, y);
            canvas.DrawString(fact.Value, _font, s_text, right - _font.MeasureString(fact.Value), y);
            y += rowHeight;
        }
    }

    private void DrawTapButton(Canvas canvas)
    {
        int centerX = _width / 2;
        bool held = _pressed && _pressStartedOnButton;
        canvas.DrawFilledCircle(held ? s_accentPressed : s_accent, centerX, _tapCenterY, TapRadius);

        int labelWidth = _tapLabel.Width * TapLabelScale;
        int labelHeight = _tapLabel.Height * TapLabelScale;
        canvas.DrawCanvas(_tapLabel, centerX - (labelWidth / 2), _tapCenterY - (labelHeight / 2), labelWidth, labelHeight);

        string count = _taps == 1 ? "1 tap" : _taps + " taps";
        int countY = _tapCenterY + TapRadius + RowGap;
        canvas.DrawString(count, _font, s_text, (_width - _font.MeasureString(count)) / 2, countY);
    }

    private void DrawNavPill(Canvas canvas)
    {
        int x = (_width - NavPillWidth) / 2;
        int y = _height - NavPillBottomGap - NavPillHeight;
        FillRoundedRectangle(canvas, s_dimText, x, y, NavPillWidth, NavPillHeight, NavPillHeight / 2);
    }

    /// <summary>Shows where the finger is: a ring, filled while it presses.</summary>
    private void DrawTouch(Canvas canvas)
    {
        if (_pressed)
        {
            canvas.DrawFilledCircle(s_text, _pointerX, _pointerY, TouchRadius);
            return;
        }

        for (int i = 0; i < TouchRingThickness; i++)
        {
            canvas.DrawCircle(s_text, _pointerX, _pointerY, TouchRadius - i);
        }
    }

    private bool IsOnTapButton(int x, int y)
    {
        int dx = x - (_width / 2);
        int dy = y - _tapCenterY;
        return (dx * dx) + (dy * dy) <= TapRadius * TapRadius;
    }

    /// <summary>
    /// Draws a line of text on a canvas of its own, the size of the text, so it can be
    /// stretched onto the screen larger than the font draws it.
    /// </summary>
    private Canvas RenderText(string text, Color color)
    {
        Canvas image = new(_font.MeasureString(text), _font.Height);
        image.Clear(Color.Transparent);
        image.DrawString(text, _font, color, 0, 0);
        return image;
    }

    private static void FillRoundedRectangle(Canvas canvas, Color color, int x, int y, int width, int height, int radius)
    {
        canvas.DrawFilledRectangle(color, x + radius, y, width - (radius * 2), height);
        canvas.DrawFilledRectangle(color, x, y + radius, width, height - (radius * 2));

        int right = x + width - radius - 1;
        int bottom = y + height - radius - 1;
        canvas.DrawFilledCircle(color, x + radius, y + radius, radius);
        canvas.DrawFilledCircle(color, right, y + radius, radius);
        canvas.DrawFilledCircle(color, x + radius, bottom, radius);
        canvas.DrawFilledCircle(color, right, bottom, radius);
    }

    private static string TwoDigits(int value)
    {
        return value < 10 ? "0" + value : value.ToString();
    }
}
