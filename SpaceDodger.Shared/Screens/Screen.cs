using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Core;
using SpaceDodger.Graphics;
using SpaceDodger.Input;

namespace SpaceDodger.Screens
{
    /// <summary>Convenience base class holding the shared <see cref="GameContext"/>.</summary>
    public abstract class Screen : IScreen
    {
        protected GameContext Context { get; }

        protected Screen(GameContext context) => Context = context;

        public virtual bool IsOverlay => false;

        public virtual void Load() { }
        public virtual void Unload() { }

        public abstract void Update(float dt, in InputState input);
        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>Override to draw high-res UI on the real backbuffer.</summary>
        public virtual void DrawHighRes(SpriteBatch spriteBatch, VirtualScreen screen) { }
    }
}
