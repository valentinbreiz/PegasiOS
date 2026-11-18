/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Drawing;
using Aura.UI.Skin;
using Cosmos.Kernel.System.Graphics;

namespace Aura.UI.Components
{
    public class Window : Component
    {
        public Bitmap Icon;
        public string Name;
        public Button Close;
        public Button Minimize;
        public Button Maximize;
        public Panel TopBar;

        public bool HasBorders;
        public bool HasCloseButton;
        public bool HasMinimizeButton;
        public bool HasMaximizeButton;

        public int MaxHeight { get; set; }
        public int MaxWidth { get; set; }

        public Window(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Frame = Gui.Theme.GetFrame("window");
            MaxHeight = Gui.ScreenHeight;
            MaxWidth = Gui.ScreenWidth;
            HasBorders = false;
        }

        public Window(string name, int x, int y, int width, int height, bool hasCloseButton = true, bool hasMinimizeButton = true) : base(x, y, width, height)
        {
            Frame = Gui.Theme.GetFrame("window");

            Icon = Gui.Resources.GetIcon("16-program.bmp");
            Name = name;
            HasCloseButton = hasCloseButton;
            HasMinimizeButton = hasMinimizeButton;
            HasBorders = true;

            if (HasBorders)
            {
                TopBar = new Panel(Color.Transparent, 3, 3, Width - 5, 18);
                TopBar.Background = false;
                TopBar.Borders = false;
                TopBar.Text = name;
                AddChild(TopBar);
            }

            if (HasCloseButton)
            {
                Frame frame = Gui.Theme.GetFrame("window.close.normal");
                Frame frame2 = Gui.Theme.GetFrame("window.close.highlighted");
                Frame.Region region = frame.Regions[0];
                Close = new Button(Width - 20, 5, region.SourceRegion.Width, region.SourceRegion.Height);
                Close.SetNormalFrame(frame);
                Close.SetHighlightedFrame(frame2);
                AddChild(Close);
            }

            if (HasMinimizeButton)
            {
                Frame frame = Gui.Theme.GetFrame("window.minimize.normal");
                Frame frame2 = Gui.Theme.GetFrame("window.minimize.highlighted");
                Frame.Region region = frame.Regions[0];
                Minimize = new Button(Width - 38, 5, region.SourceRegion.Width, region.SourceRegion.Height);
                Minimize.SetNormalFrame(frame);
                Minimize.SetHighlightedFrame(frame2);
                AddChild(Minimize);
            }

            // Force maximize for taskbar actions
            HasMaximizeButton = true;

            if (HasMaximizeButton)
            {
                Maximize = new Button(Width - 60, 5, 0, 0);
                Maximize.Frame = Gui.Theme.GetFrame("window.minimize.normal");
                Maximize.Visible = false;
                Maximize.NoBackground = true;
                AddChild(Maximize);
            }
        }

        public override void Update()
        {
            if (HasCloseButton)
            {
                Close.Update();
            }

            if (HasMinimizeButton)
            {
                Minimize.Update();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (HasBorders)
            {
                TopBar.Draw(this);

                if (HasCloseButton)
                {
                    Close.Draw(this);
                }

                if (HasMinimizeButton)
                {
                    Minimize.Draw(this);
                }
            }
        }

        /// <summary>
        /// The buttons stay at the right edge and the title bar spans the new width (it keeps its
        /// context menu). Window.Draw draws them.
        /// </summary>
        protected override void OnResized(int deltaWidth)
        {
            if (HasCloseButton)
            {
                Close.X += deltaWidth;
            }

            if (HasMinimizeButton)
            {
                Minimize.X += deltaWidth;
            }

            if (HasMaximizeButton)
            {
                Maximize.X += deltaWidth;
            }

            if (HasBorders)
            {
                TopBar.SetSize(Width - 5, TopBar.Height);
            }
        }
    }
}
