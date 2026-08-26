using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using SpaceDodger.Graphics;

namespace SpaceDodger.Input
{
    /// <summary>
    /// Touch input for Android:
    /// - Drag anywhere on the left side to move the ship.
    /// - The ship fires automatically, so no second fire touch is required.
    /// - Quick touches produce Tap events for menus.
    /// - The hardware back button maps to BackPressed/PausePressed.
    /// </summary>
    public sealed class TouchInputProvider : IInputProvider
    {
        private const float JoystickRadius = 22f;   // virtual px for full deflection
        private const float TapMaxDuration = 0.25f; // seconds
        private const float TapMaxDistance = 6f;    // virtual px

        private readonly VirtualScreen _screen;

        private int _joystickId = -1;
        private Vector2 _joystickOrigin;

        private int _tapCandidateId = -1;
        private Vector2 _tapStart;
        private float _tapTime;

        private bool _backWasDown;
        private bool _backRequested;
        private int _scrollId = -1;
        private Vector2 _scrollLast;

        public InputState State { get; private set; }

        public TouchInputProvider(VirtualScreen screen) => _screen = screen;

        /// <summary>Called by the Android activity for the system back gesture.</summary>
        public void RequestBack() => _backRequested = true;

        public void Update()
        {
            var touches = TouchPanel.GetState();
            var state = new InputState(); // Fire is false by default

            bool joystickAlive = false;

            foreach (var touch in touches)
            {
                var pos = _screen.ToVirtual(touch.Position);
                bool notHeader = pos.Y > 10f; // Everything below the top header

                if (touch.State == TouchLocationState.Pressed)
                {
                    if (_scrollId == -1)
                    {
                        _scrollId = touch.Id;
                        _scrollLast = pos;
                    }
                    // Ship movement applies anywhere on screen except the top 10px header
                    if (notHeader && _joystickId == -1)
                    {
                        _joystickId = touch.Id;
                        _joystickOrigin = pos;
                    }
                    _tapCandidateId = touch.Id;
                    _tapStart = pos;
                    _tapTime = 0f;
                }

                if (touch.Id == _scrollId && touch.State == TouchLocationState.Moved)
                {
                    state.ScrollY += pos.Y - _scrollLast.Y;
                    _scrollLast = pos;
                }
                else if (touch.Id == _scrollId && touch.State == TouchLocationState.Released)
                {
                    _scrollId = -1;
                }

                if (touch.Id == _joystickId &&
                    (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved))
                {
                    joystickAlive = true;
                    state.Fire = true; // Actively moving/holding means we also want to fire
                    var delta = (pos - _joystickOrigin) / JoystickRadius;
                    if (delta.LengthSquared() > 1f)
                        delta.Normalize();
                    state.Move = delta;
                }

                if (touch.Id == _tapCandidateId)
                {
                    if (touch.State == TouchLocationState.Moved)
                    {
                        _tapTime += 1f / 60f;
                        if (Vector2.Distance(pos, _tapStart) > TapMaxDistance || _tapTime > TapMaxDuration)
                            _tapCandidateId = -1;
                    }
                    else if (touch.State == TouchLocationState.Released)
                    {
                        state.Tap = pos;
                        state.ConfirmPressed = true;
                        _tapCandidateId = -1;
                    }
                }
            }

            if (!joystickAlive)
                _joystickId = -1;

            // Android hardware back button arrives via GamePad.
            bool backDown = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;
            if (_backRequested || (backDown && !_backWasDown))
            {
                state.BackPressed = true;
                state.PausePressed = true;
                _backRequested = false;
            }
            _backWasDown = backDown;

            State = state;
        }
    }
}
