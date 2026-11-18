// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Aura.UI.Components;

namespace PegasiOS.Shell.Controls;

/// <summary>
/// A component that owns the components on it: it updates them, and when one of them changed it
/// draws its background again and all of them over it. The kit draws a component into its parent
/// but leaves the parent to say when; this is the shell's answer, the same for a bar, a card and
/// a whole screen.
/// </summary>
internal class Container : Component
{
    /// <summary>What the container is cleared to before its children are drawn.</summary>
    public Color Background { get; set; } = Color.Transparent;

    public Container(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }

    /// <summary>Updates the visible children. A container has no look of its own under the pointer.</summary>
    public override void Update()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Component child = Children[i];
            if (child.Visible)
            {
                child.Update();
            }
        }
    }

    /// <summary>
    /// Marks the container as changed when anything on it changed, however deep. The shell calls
    /// it on its root containers once the frame's updates are done, before the frame is drawn.
    /// </summary>
    /// <returns>True when the container has to be drawn again.</returns>
    public bool RefreshDirty()
    {
        bool dirty = IsDirty() || ForceDirty;
        for (int i = 0; i < Children.Count; i++)
        {
            Component child = Children[i];
            if (!child.Visible)
            {
                continue;
            }

            if (child is Container container)
            {
                dirty |= container.RefreshDirty();
            }
            else
            {
                dirty |= child.IsDirty() || child.ForceDirty;
            }
        }

        if (dirty)
        {
            MarkDirty();
        }

        return dirty;
    }

    public override void Draw()
    {
        Clear(Background);
        DrawBackground();

        for (int i = 0; i < Children.Count; i++)
        {
            Component child = Children[i];
            if (child.Visible)
            {
                child.Draw(this);
                child.MarkCleaned();
            }
        }
    }

    /// <summary>Draws what sits under the children, over the cleared background. Nothing by default.</summary>
    protected virtual void DrawBackground()
    {
    }
}
