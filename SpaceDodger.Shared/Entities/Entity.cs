using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Pooling;

namespace SpaceDodger.Entities
{
    /// <summary>
    /// Base class for every moving object. Position is the entity CENTER.
    /// Implements IPoolable so all entities can live in object pools.
    /// </summary>
    public abstract class Entity : IPoolable
    {
        public Vector2 Position;
        public Vector2 Velocity;

        /// <summary>Seconds since this instance was (re)spawned.</summary>
        public float Age { get; protected set; }

        public bool Active { get; private set; }

        public abstract Rectangle Bounds { get; }

        public virtual void OnObtain()
        {
            Active = true;
            Age = 0f;
            Velocity = Vector2.Zero;
        }

        public virtual void OnRelease() => Active = false;

        /// <summary>Mark for recycling; the owning pool sweeps it up.</summary>
        public void Deactivate() => Active = false;

        public virtual void Update(float dt)
        {
            Age += dt;
            Position += Velocity * dt;
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>Helper: centered rectangle of the given size at Position.</summary>
        protected Rectangle CenteredRect(int width, int height) =>
            new Rectangle(
                (int)(Position.X - width / 2f),
                (int)(Position.Y - height / 2f),
                width, height);
    }
}
