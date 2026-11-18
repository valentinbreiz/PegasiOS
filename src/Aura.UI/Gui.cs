// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Aura.UI.Skin;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Aura.UI
{
    /// <summary>
    /// What the components of the kit share: the screen they are composed on, the default font,
    /// the theme, the icons, the pointer and the window manager. In Aura these were statics of
    /// the kernel (Kernel.font, Kernel.ThemeManager, Explorer.WindowManager); here the system
    /// that hosts the kit hands them over once, with Initialize, before it creates a component.
    /// </summary>
    public static class Gui
    {
        /// <summary>
        /// The font a component draws its text in unless it is given another. Fixed width.
        /// </summary>
        public static Font Font { get; set; }

        /// <summary>
        /// The skin the components take their frames from.
        /// </summary>
        public static ThemeManager Theme { get; private set; }

        /// <summary>
        /// The icons the components draw (a window's close button, the cursors).
        /// </summary>
        public static ResourceManager Resources { get; private set; }

        /// <summary>
        /// Orders the root components and composes them on the screen.
        /// </summary>
        public static WindowManager Windows { get; private set; }

        /// <summary>
        /// Where the pointer (a mouse, a finger) is, and which component its clicks go to.
        /// </summary>
        public static PointerManager Pointer { get; private set; }

        /// <summary>
        /// The canvas every frame is composed on.
        /// </summary>
        public static Canvas Screen { get; private set; }

        /// <summary>
        /// The size the interface is laid out at: the size of Screen.
        /// </summary>
        public static int ScreenWidth => Screen != null ? Screen.Width : 0;
        public static int ScreenHeight => Screen != null ? Screen.Height : 0;

        /// <summary>
        /// The height the host keeps for itself at the bottom of the screen (Aura's taskbar, a
        /// phone's navigation bar): a list that opens does not run under it.
        /// </summary>
        public static int BottomInset { get; set; }

        /// <summary>
        /// True once Initialize has run: components can be created.
        /// </summary>
        public static bool IsInitialized => Windows != null;

        /// <summary>
        /// Starts the kit on that screen with the default theme (Suave) and the kit's window manager.
        /// </summary>
        /// <param name="screen">The canvas the frames are composed on.</param>
        /// <param name="font">The default font, fixed width.</param>
        public static void Initialize(Canvas screen, Font font)
        {
            Initialize(screen, font, new WindowManager());
        }

        /// <summary>
        /// Starts the kit on that screen with the default theme (Suave) and a window manager of the
        /// host's own (one that also draws its applications, as Aura's does).
        /// </summary>
        public static void Initialize(Canvas screen, Font font, WindowManager windows)
        {
            ArgumentNullException.ThrowIfNull(screen);
            ArgumentNullException.ThrowIfNull(font);
            ArgumentNullException.ThrowIfNull(windows);

            Font = font;
            Screen = screen;

            Resources = new ResourceManager();
            Resources.LoadDefaults();

            Theme = new ThemeManager();
            Theme.LoadDefault();

            Windows = windows;
            Windows.SetScreen(screen);

            Pointer = new PointerManager();
        }

        /// <summary>
        /// Composes the frames on another canvas from now on (after a change of resolution or scale).
        /// The host resizes its screen-sized components itself.
        /// </summary>
        public static void SetScreen(Canvas screen)
        {
            Screen = screen;
            Windows?.SetScreen(screen);
        }
    }
}
