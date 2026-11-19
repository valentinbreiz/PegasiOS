// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.IO;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell;

/// <summary>
/// The picture behind the home screen: the sky around 51 Pegasi, the star the system is named
/// after. It is embedded in the kernel at the size of the phone's panel and sits centered on
/// the panel, so a component shows the part of it that lies under itself.
/// </summary>
internal sealed class Wallpaper
{
    /// <summary>Who the picture is credited to. Its license asks for the credit to be shown.</summary>
    public const string Credit = "ESO/Digitized Sky Survey 2";

    /// <summary>The picture's name in the kernel's resources, given by PegasiOS.csproj.</summary>
    private const string ResourceName = "PegasiOS/51-pegasi.png";

    private readonly Image _image;

    /// <summary>Where the picture's left edge is on the panel.</summary>
    private readonly int _left;

    /// <summary>Where the picture's top edge is on the panel.</summary>
    private readonly int _top;

    private Wallpaper(Image image, int panelWidth, int panelHeight)
    {
        _image = image;
        _left = (panelWidth - image.Width) / 2;
        _top = (panelHeight - image.Height) / 2;
    }

    /// <summary>Reads the picture from the kernel's resources.</summary>
    /// <returns>
    /// The wallpaper, or null when the picture cannot be read: the home screen then keeps its
    /// plain background.
    /// </returns>
    public static Wallpaper? Load(int panelWidth, int panelHeight)
    {
        try
        {
            using Stream stream = typeof(Wallpaper).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new FileNotFoundException("Embedded resource not found", ResourceName);
            Png image = new(stream);

            Log.WriteString("[PegasiOS] wallpaper ");
            Log.WriteNumber(image.Width);
            Log.WriteString("x");
            Log.WriteNumber(image.Height);
            Log.WriteString("\n");

            return new Wallpaper(image, panelWidth, panelHeight);
        }
        catch (Exception ex)
        {
            Log.WriteString("[PegasiOS] wallpaper failed: ");
            Log.WriteString(ex.Message);
            Log.WriteString("\n");
            return null;
        }
    }

    /// <summary>Copies onto a component's canvas the part of the picture that lies under it.</summary>
    /// <param name="target">The canvas of the component.</param>
    /// <param name="panelX">Where the canvas's left edge is on the panel.</param>
    /// <param name="panelY">Where the canvas's top edge is on the panel.</param>
    public void DrawUnder(Canvas target, int panelX, int panelY)
    {
        // The picture is opaque: its rows are copied, nothing is blended.
        target.DrawArray(_image.RawData, _left - panelX, _top - panelY, _image.Width, _image.Height);
    }
}
