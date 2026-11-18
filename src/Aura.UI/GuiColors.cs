// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;

namespace Aura.UI
{
    /// <summary>
    /// The colors the components draw with besides their skin (Aura's Kernel.WhiteColor,
    /// Kernel.DarkGray...). A host may assign others before it creates its components.
    /// </summary>
    public static class GuiColors
    {
        public static Color White = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        public static Color Black = Color.FromArgb(0xff, 0x00, 0x00, 0x00);
        public static Color Gray = Color.FromArgb(0xff, 0xdf, 0xdf, 0xdf);
        public static Color DarkGray = Color.FromArgb(0xff, 0x80, 0x80, 0x80);

        /// <summary>
        /// Under a selected list row or text, written in white.
        /// </summary>
        public static Color Selection = Color.FromArgb(0xff, 0x31, 0x6a, 0xc5);
    }
}
