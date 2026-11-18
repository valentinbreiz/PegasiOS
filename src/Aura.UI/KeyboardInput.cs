/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Manages keyboard interactions
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Input;
using CosmosKeyboard = Cosmos.Kernel.System.Input.KeyboardManager;

namespace Aura.UI
{
    /// <summary>
    /// The keys of the interface. Update drains the kernel's key queue once per frame, offers each
    /// key to the host first (its global shortcuts) and queues the others; the controls read them
    /// with TryGetKey. A host that types keys itself (an on-screen keyboard) queues them with Enqueue.
    /// </summary>
    /// <remarks>
    /// Update is meant to be the only caller of the kernel's KeyboardManager.TryReadKey while the
    /// interface runs. The Console input APIs (ReadKey, KeyAvailable, ReadLine) must not be used
    /// then: they move the whole kernel queue into the console's own buffer.
    /// </remarks>
    public static class KeyboardInput
    {
        private static readonly Queue<KeyEvent> s_keyEvents = new Queue<KeyEvent>();

        /// <summary>
        /// Sees every key before the controls: return true to keep it from them (a global shortcut
        /// the host handled). Null to let every key through.
        /// </summary>
        public static Func<KeyEvent, bool> Filter;

        /// <summary>
        /// Moves the keys typed since the last call from the kernel's queue to the interface's.
        /// </summary>
        public static void Update()
        {
            while (CosmosKeyboard.TryReadKey(out KeyEvent keyEvent))
            {
                Enqueue(keyEvent);
            }
        }

        /// <summary>
        /// Queues a key as if it had been typed.
        /// </summary>
        public static void Enqueue(KeyEvent keyEvent)
        {
            if (keyEvent == null)
            {
                return;
            }

            if (Filter != null && Filter(keyEvent))
            {
                return;
            }

            s_keyEvents.Enqueue(keyEvent);
        }

        public static bool TryGetKey(out KeyEvent keyEvent)
        {
            if (s_keyEvents.Count > 0)
            {
                keyEvent = s_keyEvents.Dequeue();
                return true;
            }

            keyEvent = default;
            return false;
        }

        /// <summary>
        /// Whether the key is that letter with Ctrl held (Ctrl+C), not a character AltGr types, which
        /// also reports Ctrl.
        /// </summary>
        public static bool IsShortcut(KeyEvent keyEvent, Key letter)
        {
            char c = keyEvent.KeyChar;
            bool typesCharacter = char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ';
            return keyEvent.Key == letter && !typesCharacter
                && (CosmosKeyboard.ControlPressed || (keyEvent.Modifiers & ConsoleModifiers.Control) != 0);
        }

        /// <summary>
        /// Whether Shift was held with the key.
        /// </summary>
        public static bool IsShiftHeld(KeyEvent keyEvent)
        {
            return CosmosKeyboard.ShiftPressed || (keyEvent.Modifiers & ConsoleModifiers.Shift) != 0;
        }
    }
}
