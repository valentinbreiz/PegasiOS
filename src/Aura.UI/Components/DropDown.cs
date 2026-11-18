/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Drop down list class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;

namespace Aura.UI.Components
{
    /// <summary>
    /// Drop down list: the face shows the selected item and an arrow, a left click opens the
    /// items under it, and picking one selects it and calls SelectionChanged. The list is a
    /// RightClick menu, so it draws above every window like the other menus. The host adds it
    /// to its children and calls Update/Draw like any Button.
    /// </summary>
    public class DropDown : Button
    {
        public List<string> Items = new List<string>();
        public Action SelectionChanged;

        private int _selectedIndex = -1;
        private RightClick _list;
        private bool _listOutdated = true;

        public DropDown(int x, int y, int width, int height) : base("", x, y, width, height)
        {
            TextAlignStyle = TextAlign.Left;

            Click = new Action(() =>
            {
                if (_list != null && _list.Opened)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            });
        }

        /// <summary>
        /// Index of the selected item in Items, -1 when none. Setting it does not call SelectionChanged.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = (value >= 0 && value < Items.Count) ? value : -1;
                Text = _selectedIndex >= 0 ? Items[_selectedIndex] : "";
                MarkDirty();
            }
        }

        /// <summary>
        /// The selected item, null when none.
        /// </summary>
        public string SelectedItem
        {
            get
            {
                return _selectedIndex >= 0 ? Items[_selectedIndex] : null;
            }
        }

        public void AddItem(string item)
        {
            Items.Add(item);
            _listOutdated = true;
        }

        public void ClearItems()
        {
            Close();
            Items.Clear();
            _listOutdated = true;
            SelectedIndex = -1;
        }

        /// <summary>
        /// Opens the list under the face, or above it when it would run under what the host keeps at
        /// the bottom of the screen (Gui.BottomInset: Aura's taskbar).
        /// </summary>
        public void Open()
        {
            RightClick contextMenu = Gui.Windows.ContextMenu;

            if (contextMenu != null && contextMenu.Opened)
            {
                contextMenu.Opened = false;
                Gui.Windows.ContextMenu = null;
            }

            if (_listOutdated)
            {
                BuildList();
            }

            if (_list == null)
            {
                return;
            }

            int screenWidth = Gui.ScreenWidth;
            int bottom = Gui.ScreenHeight - Gui.BottomInset;

            int x = Math.Max(0, Math.Min(AbsoluteX, screenWidth - _list.Width));
            int y = AbsoluteY + Height;

            if (y + _list.Height > bottom)
            {
                y = AbsoluteY - _list.Height;

                if (y < 0)
                {
                    y = Math.Max(0, bottom - _list.Height);
                }
            }

            _list.X = x;
            _list.Y = y;
            _list.Opened = true;
            Gui.Windows.ContextMenu = _list;
            Gui.Windows.BringToFront(_list);
        }

        public void Close()
        {
            if (_list == null || !_list.Opened)
            {
                return;
            }

            _list.Opened = false;

            if (Gui.Windows.ContextMenu == _list)
            {
                Gui.Windows.ContextMenu = null;
            }
        }

        /// <summary>
        /// The list takes the new width the next time it opens.
        /// </summary>
        public override void SetSize(int width, int height)
        {
            Close();
            base.SetSize(width, height);
            _listOutdated = true;
        }

        private void BuildList()
        {
            _listOutdated = false;

            if (_list != null)
            {
                _list.Dispose();
                _list = null;
            }

            if (Items.Count == 0)
            {
                return;
            }

            _list = new RightClick(0, 0, Width, Items.Count * RightClickEntry.ConstHeight);

            for (int i = 0; i < Items.Count; i++)
            {
                int index = i;

                RightClickEntry entry = new(Items[i], _list.Width, _list);
                entry.Click = new Action(() =>
                {
                    SelectedIndex = index;

                    // GEN3-GAP(null-deref): invoking a null delegate halts the kernel.
                    if (SelectionChanged != null)
                    {
                        SelectionChanged();
                    }
                });
                _list.AddEntry(entry);
            }
        }

        public override void Draw()
        {
            base.Draw();

            // Down arrow at the right edge, 7 px wide and 4 px tall.
            int arrowX = Width - 7 - 6;
            int arrowY = Height / 2 - 2;

            for (int row = 0; row < 4; row++)
            {
                DrawFilledRectangle(TextColor, arrowX + row, arrowY + row, 7 - row * 2, 1);
            }
        }

        public override void Dispose()
        {
            if (_list != null)
            {
                Close();
                _list.Dispose();
                _list = null;
            }

            base.Dispose();
        }
    }
}
