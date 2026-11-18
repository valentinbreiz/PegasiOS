// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cosmos.Kernel.System.Graphics;

namespace Aura.UI
{
    /// <summary>
    /// The icons of the kit, by name ("16-close.bmp", "00-cursor.bmp"), and the reader of the files
    /// the library embeds (the default theme, the icons). A host adds its own icons with AddIcon.
    /// </summary>
    public class ResourceManager
    {
        private const string ResourcePrefix = "Aura.UI/";

        private static readonly string[] s_defaultIcons =
        {
            "16-close.bmp",
            "16-minimize.bmp",
            "16-program.bmp",
            "00-cursor.bmp",
            "00-grab.bmp",
            "00-resize-horizontal.bmp",
            "00-resize-vertical.bmp",
        };

        private readonly Dictionary<string, Bitmap> _icons = new Dictionary<string, Bitmap>();

        /// <summary>
        /// Loads the icons the components themselves draw.
        /// </summary>
        public void LoadDefaults()
        {
            foreach (string name in s_defaultIcons)
            {
                if (!_icons.ContainsKey(name))
                {
                    _icons.Add(name, new Bitmap(ReadEmbedded("Icons/" + name)));
                }
            }
        }

        /// <summary>
        /// Adds an icon, or replaces the one of that name.
        /// </summary>
        public void AddIcon(string name, Bitmap icon)
        {
            _icons[name] = icon;
        }

        /// <summary>
        /// The icon of that name.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No icon has that name.</exception>
        public Bitmap GetIcon(string key)
        {
            Bitmap bitmap;

            if (key != null && _icons.TryGetValue(key, out bitmap))
            {
                return bitmap;
            }

            throw new KeyNotFoundException(key + " not found");
        }

        /// <summary>
        /// The icon of that name, false when there is none.
        /// </summary>
        public bool TryGetIcon(string key, out Bitmap icon)
        {
            icon = null;
            return key != null && _icons.TryGetValue(key, out icon);
        }

        /// <summary>
        /// A file embedded in the library, by its path under Resources/ ("Themes/SuaveSheet.bmp").
        /// </summary>
        /// <exception cref="FileNotFoundException">No such embedded file.</exception>
        public static byte[] ReadEmbedded(string relativePath)
        {
            string name = ResourcePrefix + relativePath.Replace('\\', '/');
            Stream stream = typeof(ResourceManager).Assembly.GetManifestResourceStream(name)
                ?? throw new FileNotFoundException("Embedded resource not found: " + name, name);

            byte[] data = new byte[stream.Length];
            int offset = 0;

            // Read may return fewer bytes than asked for.
            while (offset < data.Length)
            {
                int read = stream.Read(data, offset, data.Length - offset);

                if (read <= 0)
                {
                    break;
                }

                offset += read;
            }

            stream.Dispose();

            if (offset != data.Length)
            {
                throw new EndOfStreamException("Embedded resource truncated: " + name);
            }

            return data;
        }

        /// <summary>
        /// An embedded text file, UTF-8 with its byte order mark removed (the skin has one, which
        /// the XML parser rejects).
        /// </summary>
        public static string ReadEmbeddedText(string relativePath)
        {
            byte[] bytes = ReadEmbedded(relativePath);
            int skip = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            return Encoding.UTF8.GetString(bytes, skip, bytes.Length - skip);
        }
    }
}
