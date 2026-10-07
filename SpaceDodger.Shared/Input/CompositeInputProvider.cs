using System;
using Microsoft.Xna.Framework;

namespace SpaceDodger.Input
{
    /// <summary>
    /// Composite input provider that aggregates touch and keyboard/mouse inputs.
    /// Implements Composite Pattern (SOLID - Open/Closed, Single Responsibility).
    /// Enables seamless hybrid play: touch controls on mobile devices,
    /// full keyboard + mouse controls on Google Play Games for PC, Chromebooks, and tablets.
    /// </summary>
    public sealed class CompositeInputProvider : IInputProvider
    {
        private readonly TouchInputProvider _touch;
        private readonly KeyboardInputProvider _keyboard;

        public InputState State { get; private set; }

        public CompositeInputProvider(TouchInputProvider touch, KeyboardInputProvider keyboard)
        {
            _touch = touch ?? throw new ArgumentNullException(nameof(touch));
            _keyboard = keyboard ?? throw new ArgumentNullException(nameof(keyboard));
        }

        public void Update()
        {
            _touch.Update();
            _keyboard.Update();

            var touchState = _touch.State;
            var keyboardState = _keyboard.State;

            // Prioritize keyboard movement (WASD/arrows) when actively pressed.
            // If no keyboard direction is held, fallback to touch joystick or mouse drag.
            Vector2 movement = keyboardState.Move != Vector2.Zero
                ? keyboardState.Move
                : touchState.Move;

            State = new InputState
            {
                Move = movement,
                Fire = keyboardState.Fire || touchState.Fire,
                ConfirmPressed = keyboardState.ConfirmPressed || touchState.ConfirmPressed,
                BackPressed = keyboardState.BackPressed || touchState.BackPressed,
                PausePressed = keyboardState.PausePressed || touchState.PausePressed,
                UpPressed = keyboardState.UpPressed || touchState.UpPressed,
                DownPressed = keyboardState.DownPressed || touchState.DownPressed,
                LeftPressed = keyboardState.LeftPressed || touchState.LeftPressed,
                RightPressed = keyboardState.RightPressed || touchState.RightPressed,
                ScrollY = Math.Abs(keyboardState.ScrollY) > 0.001f ? keyboardState.ScrollY : touchState.ScrollY,
                Tap = keyboardState.Tap ?? touchState.Tap,
            };
        }
    }
}
