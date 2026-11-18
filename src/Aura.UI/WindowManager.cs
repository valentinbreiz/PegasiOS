/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window Manager
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using System.Drawing;
using Aura.UI.Components;
using Cosmos.Kernel.System.Graphics;

namespace Aura.UI
{
    /// <summary>
    /// Keeps the root components in drawing order and composes them on the screen. It is the part
    /// of Aura's window manager that knows components only; a host with more to draw between the
    /// components and the context menu (Aura's applications) derives from it and overrides
    /// DrawHostLayers.
    /// </summary>
    public class WindowManager
    {
        public static bool WindowMoving = false;

        /// <summary>
        /// What the keys go to: the owner of the focused window, null for none.
        /// </summary>
        public IKeyOwner FocusedOwner { get; set; }

        public RightClick ContextMenu { get; set; }

        /// <summary>
        /// Outlines what was redrawn, what can be clicked and what resizes, for debugging a layout.
        /// </summary>
        public bool Debug { get; set; }

        public List<Rectangle> ClipRects = new List<Rectangle>();
        public List<Rectangle> ClickRects = new List<Rectangle>();
        public List<Rectangle> RefreshRects = new List<Rectangle>();
        public List<Rectangle> ResizeRects = new List<Rectangle>();

        private int _highestZIndex = -1;
        private Canvas _screen;

        public void AddComponent(Component component)
        {
            component.zIndex = ++_highestZIndex;
            Component.Components.Add(component);
            InsertionSortByZIndex(Component.Components);
        }

        public void BringToFront(Component component)
        {
            if (component.zIndex < _highestZIndex)
            {
                component.zIndex = ++_highestZIndex;
                InsertionSortByZIndex(Component.Components);
            }

            foreach (Component child in component.Children)
            {
                BringToFront(child);
            }
        }

        private void InsertionSortByZIndex(List<Component> components)
        {
            for (int i = 1; i < components.Count; i++)
            {
                Component key = components[i];
                int j = i - 1;

                while (j >= 0 && components[j].zIndex > key.zIndex)
                {
                    components[j + 1] = components[j];
                    j = j - 1;
                }
                components[j + 1] = key;
            }
        }

        /// <summary>
        /// True when a visible component has to be drawn again: a host that composes a frame only
        /// when something changed asks this first.
        /// </summary>
        public bool HasDirtyComponents()
        {
            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];

                if (component.Visible && (component.IsDirty() || component.ForceDirty))
                {
                    return true;
                }
            }

            return ContextMenu != null && ContextMenu.Opened;
        }

        /// <summary>
        /// Draws a frame: the components that changed into their buffers, then every visible root
        /// component onto the screen, in z order, then the host's layers and the open context menu.
        /// </summary>
        public void DrawWindows()
        {
            if (Debug)
            {
                ClipRects.Clear();
                ClickRects.Clear();
                RefreshRects.Clear();
                ResizeRects.Clear();
            }

            // Sort z index
            InsertionSortByZIndex(Component.Components);

            DrawComponents();

            DrawHostLayers();

            DrawContextMenu();

            if (Debug)
            {
                DrawRects(ResizeRects, Color.Blue);
                DrawRects(RefreshRects, Color.Green);
                DrawRects(ClickRects, Color.Red);
            }
        }

        /// <summary>
        /// Called by DrawWindows once the components are composed, before the context menu. Nothing
        /// by default.
        /// </summary>
        protected virtual void DrawHostLayers()
        {
        }

        public void DrawComponents()
        {
            // Draw components
            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];

                if (component.IsRoot && component.Visible)
                {
                    if (component.IsDirty() || component.ForceDirty)
                    {
                        component.Draw();
                        component.MarkCleaned();

                        if (Debug)
                        {
                            RefreshRects.Add(component.GetRectangle());
                        }
                    }

                    if (Debug)
                    {
                        AddClickRect(component, component.GetRectangle());
                    }
                }

                for (int j = 0; j < component.Children.Count; j++)
                {
                    Component child = component.Children[j];

                    if (child.Visible)
                    {
                        if (child.IsDirty() || child.ForceDirty)
                        {
                            child.Draw(child.Parent);
                            child.MarkCleaned();

                            if (Debug)
                            {
                                RefreshRects.Add(child.GetAbsoluteRectangle());
                            }
                        }

                        if (Debug)
                        {
                            AddClickRect(child, child.GetAbsoluteRectangle());
                        }
                    }
                }

                if (component.IsRoot && component.Visible)
                {
                    DrawComponent(component);
                }
            }
        }

        public void DrawContextMenu()
        {
            RightClick contextMenu = ContextMenu;
            if (contextMenu != null && contextMenu.Opened)
            {
                contextMenu.Update();
                contextMenu.Draw();
                _screen.DrawCanvas(contextMenu.GetBuffer(), contextMenu.X, contextMenu.Y);
            }
        }

        /// <summary>
        /// Composes a root component onto the screen by its pixels' alpha, faded by its Opacity.
        /// </summary>
        public void DrawComponent(Component component)
        {
            _screen.DrawCanvas(component.GetBuffer(), component.X, component.Y, component.Opacity);
        }

        public void DrawRect(int x, int y, int width, int height, Color color)
        {
            _screen.DrawRectangle(color, x, y, width, height);
        }

        /// <summary>
        /// The canvas every frame is composed on (Gui.Screen).
        /// </summary>
        public void SetScreen(Canvas screen)
        {
            _screen = screen;
        }

        private void AddClickRect(Component component, Rectangle rectangle)
        {
            if (component is Button button && button.Click != null)
            {
                ClickRects.Add(rectangle);
            }
        }

        private void DrawRects(List<Rectangle> rectangles, Color color)
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                Rectangle rectangle = rectangles[i];
                DrawRect(rectangle.Left, rectangle.Top, rectangle.Right - rectangle.Left + 1, rectangle.Bottom - rectangle.Top + 1, color);
            }
        }
    }
}
