/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Theme manager.
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;

namespace Aura.UI.Skin
{
    /// <summary>
    /// Manages themes for the user interface, loading and applying skins.
    /// </summary>
    public class ThemeManager
    {
        /// <summary>
        /// The name of the skin the library embeds.
        /// </summary>
        public const string DefaultTheme = "Suave";

        /// <summary>
        /// The skin parser used to load and interpret theme files.
        /// </summary>
        private SkinParsing _skinParser;

        /// <summary>
        /// Loads the skin embedded in the library.
        /// </summary>
        public void LoadDefault()
        {
            Load(
                ResourceManager.ReadEmbeddedText("Themes/" + DefaultTheme + ".skin.xml"),
                contentPath => ResourceManager.ReadEmbedded("Themes/" + contentPath + ".bmp"));
        }

        /// <summary>
        /// Loads a skin from wherever the host keeps it (a file of an installed system, a package).
        /// The components created afterwards take their frames from it; those that exist keep theirs.
        /// </summary>
        /// <param name="skinXml">The skin's XML, without a byte order mark.</param>
        /// <param name="bitmapLoader">
        /// Gives the bytes of a skin bitmap (a BMP file) from the contentPath its XML names.
        /// </param>
        public void Load(string skinXml, Func<string, byte[]> bitmapLoader)
        {
            SkinParsing parser = new SkinParsing();
            parser.loadSkin(skinXml, bitmapLoader);
            _skinParser = parser;
        }

        /// <summary>
        /// Retrieves the specified frame by name from the currently loaded theme.
        /// </summary>
        /// <param name="name">The name of the frame to retrieve.</param>
        /// <returns>The requested frame.</returns>
        public Frame GetFrame(string name)
        {
            return _skinParser.GetFrame(name);
        }

        /// <summary>
        /// Gets the name of the currently loaded theme.
        /// </summary>
        /// <returns>The name of the current theme.</returns>
        public string GetThemeName()
        {
            return _skinParser.GetSkinName();
        }
    }
}
