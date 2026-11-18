/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages pointer interactions, including left and right clicks and double clicks.
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using Aura.UI.Components;
using Cosmos.Kernel.System.Graphics;
using CosmosMouse = Cosmos.Kernel.System.Input.MouseManager;

namespace Aura.UI
{
    public enum CursorState
    {
        Normal,
        ResizeHorizontal,
        ResizeVertical,
        Grab
    }

    /// <summary>
    /// The pointer of the interface: where it is, which buttons are down, and which component a
    /// click goes to. It is Aura's MouseManager without the mouse: the host gives it the position
    /// and the buttons once per frame, read from the kernel's mouse (the Update() without
    /// arguments) or from anything else that points, a touchscreen for one.
    /// </summary>
    public class PointerManager
    {
        /// <summary>
        /// The position the pointer had at the last Update, in screen coordinates.
        /// </summary>
        public int X { get; private set; }
        public int Y { get; private set; }

        /// <summary>
        /// Represents the top component currently under the mouse cursor.
        /// </summary>
        public Component TopComponent;

        /// <summary>
        /// Indicates whether the left mouse button is currently being held down.
        /// </summary>
        public bool IsLeftButtonDown;

        /// <summary>
        /// True from the update the left button went down until the next one only: what a press starts
        /// (moving or resizing a window, taking a slider) checks it, so the button held from elsewhere
        /// crossing a window border or a control starts nothing.
        /// </summary>
        public bool IsLeftButtonPressed;

        /// <summary>
        /// True from the update the left button went up until the next one only: what a release ends
        /// (a tap on a control for a finger) checks it, so a control that was not updated while the
        /// button went up does not take a release it never saw.
        /// </summary>
        public bool IsLeftButtonReleased;

        /// <summary>
        /// The root component (window, taskbar, start menu, menu...) drawn on top under the mouse
        /// when the left button went down, null over none.
        /// </summary>
        public Component LeftPressRoot;

        /// <summary>
        /// Indicates whether the right mouse button is currently being held down.
        /// </summary>
        public bool IsRightButtonDown;

        /// <summary>
        /// Focused component (set by click)
        /// </summary>
        public Component FocusedComponent;

        public static CursorState CursorState = CursorState.Normal;

        /// <summary>
        /// The maximum time interval in milliseconds to detect a double click.
        /// </summary>
        private const int doubleClickTime = 500;

        /// <summary>
        /// Timestamp of the last left mouse button click. Used for double click detection.
        /// </summary>
        private DateTime _lastLeftClickTime = DateTime.MinValue;

        /// <summary>
        /// Timestamp of the last right mouse button click. Used for double click detection.
        /// </summary>
        private DateTime _lastRightClickTime = DateTime.MinValue;

        /// <summary>
        /// Flag to indicate whether the left mouse button is currently pressed.
        /// </summary>
        private bool _leftButtonPressed;

        /// <summary>
        /// Flag to indicate whether the right mouse button is currently pressed.
        /// </summary>
        private bool _rightButtonPressed;

        /// <summary>
        /// Takes the kernel's mouse as the pointer and processes its clicks. Call it once per frame,
        /// or the Update with arguments when the pointer is not the kernel's mouse.
        /// </summary>
        public void Update()
        {
            Update(CosmosMouse.X, CosmosMouse.Y, CosmosMouse.LeftButton, CosmosMouse.RightButton);
        }

        /// <summary>
        /// Takes the state of the pointer for this frame and processes its clicks: a button that
        /// went down since the last call clicks the component on top under the pointer.
        /// </summary>
        /// <param name="x">Position in screen coordinates.</param>
        /// <param name="y">Position in screen coordinates.</param>
        /// <param name="leftButton">Whether the left button is down, or the finger on the screen.</param>
        /// <param name="rightButton">Whether the right button is down.</param>
        public void Update(int x, int y, bool leftButton, bool rightButton)
        {
            X = x;
            Y = y;

            IsLeftButtonPressed = false;
            IsLeftButtonReleased = false;

            if (leftButton)
            {
                if (!_leftButtonPressed)
                {
                    // Before the click, which may close the menu that was pressed.
                    LeftPressRoot = DetermineTopRoot();
                    IsLeftButtonPressed = true;

                    ProcessLeftClick();
                    _leftButtonPressed = true;
                }
                IsLeftButtonDown = true;
            }
            else
            {
                if (_leftButtonPressed)
                {
                    _leftButtonPressed = false;
                    IsLeftButtonReleased = true;
                }
                IsLeftButtonDown = false;
            }

            if (rightButton)
            {
                if (!_rightButtonPressed)
                {
                    ProcessRightClick();
                    _rightButtonPressed = true;
                }
                IsRightButtonDown = true;
            }
            else
            {
                if (_rightButtonPressed)
                {
                    _rightButtonPressed = false;
                }
                IsRightButtonDown = false;
            }
        }

        /// <summary>
        /// Processes a left click action. Determines if the click is a single or double click based on the time elapsed since the last click.
        /// </summary>
        private void ProcessLeftClick()
        {
            if ((DateTime.Now - _lastLeftClickTime).TotalMilliseconds < doubleClickTime)
            {
                HandleLeftDoubleClick();
            }
            else
            {
                HandleLeftSingleClick();
            }

            _lastLeftClickTime = DateTime.Now;
        }

        /// <summary>
        /// Processes a right click action. Determines if the click is a single or double click based on the time elapsed since the last click.
        /// </summary>
        private void ProcessRightClick()
        {
            if ((DateTime.Now - _lastRightClickTime).TotalMilliseconds >= doubleClickTime)
            {
                HandleRightSingleClick();
            }

            _lastRightClickTime = DateTime.Now;
        }

        /// <summary>
        /// Handles the action for a single left click. Determines the top component for the click and manages the context menu's state.
        /// </summary>
        private void HandleLeftSingleClick()
        {
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleLeftClick();
            }
        }

        /// <summary>
        /// Handles the action for a double left click.
        /// </summary>
        private void HandleLeftDoubleClick()
        {
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleLeftDoubleClick();
            }
        }

        /// <summary>
        /// Handles the action for a single right click. Determines the top component for the click and manages the context menu's state.
        /// </summary>
        private void HandleRightSingleClick()
        {
            Component topComponent = DetermineTopComponent();

            if (topComponent != null)
            {
                topComponent.HandleRightClick();
            }
        }

        /// <summary>
        /// Determines the top-level component. This method checks all applications and finds the one with the highest Z index under the mouse cursor.
        /// </summary>
        private Component DetermineTopComponent()
        {
            Component topComponent = null;
            int topZIndex = -1;

            void CheckComponent(Component component)
            {
                if (component.Visible && component.IsInside(X, Y))
                {
                    if (component.zIndex > topZIndex)
                    {
                        topComponent = component;
                        topZIndex = component.zIndex;
                    }
                }

                for (int i = 0; i < component.Children.Count; i++)
                {
                    Component child = component.Children[i];
                    CheckComponent(child);
                }
            }

            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];
                CheckComponent(component);
            }

            return topComponent;
        }

        /// <summary>
        /// The visible root component under the mouse with the highest zIndex: the one drawn on top.
        /// </summary>
        private Component DetermineTopRoot()
        {
            Component top = null;

            for (int i = 0; i < Component.Components.Count; i++)
            {
                Component component = Component.Components[i];

                if (component.IsRoot && component.Visible && component.IsInside(X, Y)
                    && (top == null || component.zIndex > top.zIndex))
                {
                    top = component;
                }
            }

            return top;
        }

        /// <summary>
        /// Whether the left button went down in this update over that component, in the window (or
        /// other root component) drawn on top there.
        /// </summary>
        public bool IsLeftPressOn(Component component)
        {
            if (!IsLeftButtonPressed || !component.IsInside(X, Y))
            {
                return false;
            }

            Component root = component;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root == LeftPressRoot;
        }

        /// <summary>
        /// Whether that component's window (or other root component) is the one drawn on top under the
        /// mouse.
        /// </summary>
        public bool IsOnTop(Component component)
        {
            Component root = component;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root == DetermineTopRoot();
        }

        /// <summary>
        /// Draws the software cursor for CursorState onto the screen, at the pointer (Canvas.DrawImage
        /// blends it by alpha and clips at the screen edges). For a host whose pointer is a mouse: call
        /// it last, after the windows are drawn.
        /// </summary>
        public void DrawCursor()
        {
            Canvas screen = Gui.Screen;

            if (screen == null)
            {
                return;
            }

            string name = "00-cursor.bmp";
            int x = X;
            int y = Y;

            if (CursorState == CursorState.ResizeHorizontal)
            {
                name = "00-resize-horizontal.bmp";
                x -= 23 / 2;
            }
            else if (CursorState == CursorState.ResizeVertical)
            {
                name = "00-resize-vertical.bmp";
                y -= 23 / 2;
            }
            else if (CursorState == CursorState.Grab)
            {
                name = "00-grab.bmp";
            }

            Bitmap cursor;

            if (Gui.Resources.TryGetIcon(name, out cursor))
            {
                screen.DrawImage(cursor, x, y);
            }
        }
    }
}
