// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Aura.UI.Components;
using Cosmos.Kernel.System.Input;

namespace Aura.UI
{
    /// <summary>
    /// What the keys typed in a window belong to: an application in Aura, a screen in a phone
    /// shell. The window manager knows the focused one (WindowManager.FocusedOwner); a control
    /// that holds the keys reads them only while its window is the focused owner's, and hands
    /// the keys it does not use to the owner.
    /// </summary>
    public interface IKeyOwner
    {
        /// <summary>
        /// The root component (the window) the owner shows.
        /// </summary>
        Component Root { get; }

        /// <summary>
        /// A key typed in the window that the control holding the keys does not use.
        /// </summary>
        void HandleKey(KeyEvent key);
    }
}
