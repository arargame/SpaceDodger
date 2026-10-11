using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace SpaceDodger.Input
{
    /// <summary>
    /// Bridges Android hardware key events into the platform-neutral keyboard
    /// provider. MonoGame's Keyboard.GetState can be empty on Android hosts
    /// such as Google Play Games on PC, while it remains authoritative on
    /// desktop builds.
    /// </summary>
    public static class HardwareKeyboardBridge
    {
        private static readonly object Sync = new object();
        private static readonly HashSet<Keys> PressedKeys = new HashSet<Keys>();

        public static void Press(Keys key)
        {
            if (key == Keys.None)
                return;

            lock (Sync)
                PressedKeys.Add(key);
        }

        public static void Release(Keys key)
        {
            if (key == Keys.None)
                return;

            lock (Sync)
                PressedKeys.Remove(key);
        }

        public static void Clear()
        {
            lock (Sync)
                PressedKeys.Clear();
        }

        /// <summary>Merges Android-injected keys with the native MonoGame keyboard state.</summary>
        public static KeyboardState Merge(KeyboardState nativeState)
        {
            lock (Sync)
            {
                if (PressedKeys.Count == 0)
                    return nativeState;

                var merged = new HashSet<Keys>(nativeState.GetPressedKeys());
                merged.UnionWith(PressedKeys);
                var keys = new Keys[merged.Count];
                merged.CopyTo(keys);
                return new KeyboardState(keys);
            }
        }
    }
}
