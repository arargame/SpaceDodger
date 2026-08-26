using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpaceDodger.Graphics;

namespace SpaceDodger.Input
{
    /// <summary>Keyboard (WASD/arrows + Space/Enter/Esc/P) and mouse input.</summary>
    public sealed class KeyboardInputProvider : IInputProvider
    {
        private readonly VirtualScreen _screen;

        private KeyboardState _keyboard;
        private KeyboardState _previousKeyboard;
        private MouseState _mouse;
        private MouseState _previousMouse;

        public InputState State { get; private set; }

        public KeyboardInputProvider(VirtualScreen screen) => _screen = screen;

        public void Update()
        {
            _previousKeyboard = _keyboard;
            _previousMouse = _mouse;
            _keyboard = Keyboard.GetState();
            _mouse = Mouse.GetState();

            var move = Vector2.Zero;
            if (Down(Keys.Left) || Down(Keys.A)) move.X -= 1;
            if (Down(Keys.Right) || Down(Keys.D)) move.X += 1;
            if (Down(Keys.Up) || Down(Keys.W)) move.Y -= 1;
            if (Down(Keys.Down) || Down(Keys.S)) move.Y += 1;
            if (move != Vector2.Zero)
                move.Normalize();

            Vector2? tap = null;
            if (_mouse.LeftButton == ButtonState.Released &&
                _previousMouse.LeftButton == ButtonState.Pressed)
                tap = _screen.ToVirtual(new Vector2(_mouse.X, _mouse.Y));

            State = new InputState
            {
                Move = move,
                Fire = Down(Keys.Space) || Down(Keys.J),
                ConfirmPressed = Pressed(Keys.Enter) || Pressed(Keys.Space),
                BackPressed = Pressed(Keys.Escape),
                PausePressed = Pressed(Keys.P) || Pressed(Keys.Escape),
                UpPressed = Pressed(Keys.Up) || Pressed(Keys.W),
                DownPressed = Pressed(Keys.Down) || Pressed(Keys.S),
                LeftPressed = Pressed(Keys.Left) || Pressed(Keys.A),
                RightPressed = Pressed(Keys.Right) || Pressed(Keys.D),
                ScrollY = _mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue,
                Tap = tap,
            };
        }

        private bool Down(Keys key) => _keyboard.IsKeyDown(key);

        private bool Pressed(Keys key) =>
            _keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
    }
}
