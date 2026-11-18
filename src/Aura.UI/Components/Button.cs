/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Button class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace Aura.UI.Components
{
    public class Button : Component
    {
        public enum TextAlign
        {
            Center,
            Left
        }

        public string Text;
        public Bitmap Image;
        public Action Click;
        public Color BackColor = Color.LightGray;
        public Color TextColor = Color.Black;
        public bool NoBackground = false;
        public bool Light = false;
        public bool Focused = false;
        public TextAlign TextAlignStyle = TextAlign.Center;

        public Button(int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
        }

        public Button(string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
            Text = text;
        }

        public Button(Bitmap image, int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
            Image = image;
        }

        public Button(Bitmap image, int x, int y) : base(x, y, (int)image.Width, (int)image.Height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
            Image = image;
        }

        public Button(Bitmap image, string text, int x, int y, int width, int height) : base(x, y, width, height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
            Text = text;
            Image = image;
        }

        public Button(Bitmap image, string text, int x, int y) : base(x, y, (int)image.Width, (int)image.Height)
        {
            SetNormalFrame(Gui.Theme.GetFrame("button.normal"));
            SetHighlightedFrame(Gui.Theme.GetFrame("button.highlighted"));
            SetPressedFrame(Gui.Theme.GetFrame("button.pressed"));
            Text = text;
            Image = image;
        }

        public override void HandleLeftClick()
        {
            if (Click != null)
            {
                if (IsInside(Gui.Pointer.X, Gui.Pointer.Y))
                {
                    Click();
                }
            }
        }

        public override void Draw()
        {
            if (NoBackground)
            {
                Clear(Color.Transparent);
            }
            else
            {
                base.Draw();
            }

            if (Text != null && Image != null)
            {
                DrawString(Text, Gui.Font, TextColor, 4 + (int)Image.Width + 4, (Height / 2 - Gui.Font.Height / 2));

                int imageX = 4;
                int imageY = Height / 2 - (int)Image.Height / 2;
                DrawImage(Image, imageX, imageY);
            }
            else if (Text != null)
            {
                int textX;
                int textY = (Height / 2 - Gui.Font.Height / 2);

                if (TextAlignStyle == TextAlign.Center)
                {
                    int textWidth = Text.Length * Gui.Font.Width;
                    textX = (Width / 2) - (textWidth / 2);
                }
                else
                {
                    textX = 4;
                }

                DrawString(Text, Gui.Font, TextColor, textX, textY);
            }
            else if (Image != null)
            {
                int imageX = 0 + (Width / 2 - (int)Image.Width / 2);
                int imageY = 0 + (Height / 2 - (int)Image.Height / 2);
                DrawImage(Image, imageX, imageY);
            }
        }
    }
}
