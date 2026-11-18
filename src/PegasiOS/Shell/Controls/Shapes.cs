// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Cosmos.Kernel.System.Graphics;

namespace PegasiOS.Shell.Controls;

/// <summary>The shapes the controls are drawn with that the canvas does not have.</summary>
internal static class Shapes
{
    /// <summary>
    /// Fills a rectangle whose corners are quarter circles. A radius of half the height gives a pill.
    /// </summary>
    public static void FillRoundedRectangle(Canvas canvas, Color color, int x, int y, int width, int height, int radius)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (radius * 2 > width)
        {
            radius = width / 2;
        }

        if (radius * 2 > height)
        {
            radius = height / 2;
        }

        if (radius <= 0)
        {
            canvas.DrawFilledRectangle(color, x, y, width, height);
            return;
        }

        canvas.DrawFilledRectangle(color, x + radius, y, width - (radius * 2), height);
        canvas.DrawFilledRectangle(color, x, y + radius, width, height - (radius * 2));

        int right = x + width - radius - 1;
        int bottom = y + height - radius - 1;
        canvas.DrawFilledCircle(color, x + radius, y + radius, radius);
        canvas.DrawFilledCircle(color, right, y + radius, radius);
        canvas.DrawFilledCircle(color, x + radius, bottom, radius);
        canvas.DrawFilledCircle(color, right, bottom, radius);
    }
}
