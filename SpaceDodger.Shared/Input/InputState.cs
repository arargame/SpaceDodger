using Microsoft.Xna.Framework;

namespace SpaceDodger.Input
{
    /// <summary>
    /// Platform-agnostic snapshot of player intent for one frame.
    /// Gameplay code only ever reads this struct — it never touches
    /// Keyboard/Mouse/TouchPanel directly (Interface Segregation).
    /// </summary>
    public struct InputState
    {
        /// <summary>Normalized movement direction (-1..1 on both axes).</summary>
        public Vector2 Move;

        /// <summary>Fire button currently held.</summary>
        public bool Fire;

        // One-shot presses (true only on the frame the key/tap goes down).
        public bool ConfirmPressed;
        public bool BackPressed;
        public bool PausePressed;
        public bool UpPressed;
        public bool DownPressed;
        public bool LeftPressed;
        public bool RightPressed;

        /// <summary>Vertical touch drag or mouse-wheel distance for scrollable screens.</summary>
        public float ScrollY;

        /// <summary>Tap/click position in virtual coordinates, if any this frame.</summary>
        public Vector2? Tap;
    }
}
