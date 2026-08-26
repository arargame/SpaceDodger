using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Entities;

namespace SpaceDodger.Pooling
{
    /// <summary>
    /// Specialization of <see cref="ObjectPool{T}"/> for game entities:
    /// drives Update/Draw for active instances and reclaims any entity that has
    /// deactivated itself (Open/Closed — pooling behaviour extended without
    /// modifying the base pool).
    /// </summary>
    public sealed class EntityPool<T> : ObjectPool<T> where T : Entity
    {
        public EntityPool(System.Func<T> factory, int preallocate = 0)
            : base(factory, preallocate)
        {
        }

        public void Update(float dt)
        {
            var items = Items;
            for (int i = 0; i < items.Count; i++)
            {
                var entity = items[i];

                if (entity.Active)
                    entity.Update(dt);

                // Sweep up anything that deactivated itself — either just now in
                // Update, or earlier via collision resolution. Without this second
                // case, entities killed by collisions would never return to the pool.
                if (!entity.Active && !IsFree(entity))
                    Release(entity);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var items = Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Active)
                    items[i].Draw(spriteBatch);
            }
        }
    }
}
