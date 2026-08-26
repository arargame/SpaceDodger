using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Screens
{
    /// <summary>
    /// Stack-based screen manager (State pattern):
    /// - Only the top screen receives Update/input.
    /// - Overlay screens (pause) also draw the screens below them.
    /// </summary>
    public sealed class ScreenManager
    {
        private readonly List<IScreen> _stack = new List<IScreen>();

        public bool IsEmpty => _stack.Count == 0;

        public void Push(IScreen screen)
        {
            _stack.Add(screen);
            screen.Load();
        }

        public void Pop()
        {
            if (_stack.Count == 0)
                return;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            top.Unload();
        }

        /// <summary>Pop the top screen and push a replacement.</summary>
        public void Replace(IScreen screen)
        {
            Pop();
            Push(screen);
        }

        /// <summary>Unload everything and start fresh with one screen.</summary>
        public void Reset(IScreen screen)
        {
            while (_stack.Count > 0)
                Pop();
            Push(screen);
        }

        public void Update(float dt, in InputState input)
        {
            if (_stack.Count == 0)
                return;
            _stack[_stack.Count - 1].Update(dt, input);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_stack.Count == 0)
                return;

            // Find the lowest screen that must be drawn (skip screens fully
            // covered by a non-overlay above them).
            int first = _stack.Count - 1;
            while (first > 0 && _stack[first].IsOverlay)
                first--;

            for (int i = first; i < _stack.Count; i++)
                _stack[i].Draw(spriteBatch);
        }
    }
}
