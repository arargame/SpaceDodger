using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Screens
{
    /// <summary>
    /// A single game state (menu, gameplay, pause...). Together with
    /// <see cref="ScreenManager"/> this implements the State pattern.
    /// </summary>
    public interface IScreen
    {
        /// <summary>Overlay screens (pause) let the screen underneath keep drawing.</summary>
        bool IsOverlay { get; }

        void Load();
        void Unload();
        void Update(float dt, in InputState input);
        void Draw(SpriteBatch spriteBatch);
    }
}
