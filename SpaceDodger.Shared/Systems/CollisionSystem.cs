using System.Collections.Generic;
using SpaceDodger.Entities;

namespace SpaceDodger.Systems
{
    /// <summary>
    /// Brute-force AABB collision resolution between two groups.
    /// At this scale (a few dozen entities) a spatial grid would be
    /// premature optimization; the API stays the same if one is added later.
    /// </summary>
    public static class CollisionSystem
    {
        /// <summary>Notify both sides of every overlapping pair across two groups.</summary>
        public static void Resolve<TA, TB>(IReadOnlyList<TA> groupA, IReadOnlyList<TB> groupB)
            where TA : class, ICollidable
            where TB : class, ICollidable
        {
            for (int i = 0; i < groupA.Count; i++)
            {
                var a = groupA[i];
                if (!a.Active)
                    continue;

                var boundsA = a.Bounds;

                for (int j = 0; j < groupB.Count; j++)
                {
                    var b = groupB[j];
                    if (!b.Active || ReferenceEquals(a, b))
                        continue;

                    if (!boundsA.Intersects(b.Bounds))
                        continue;

                    a.OnCollision(b);
                    b.OnCollision(a);

                    // 'a' may have been destroyed by that hit.
                    if (!a.Active)
                        break;
                }
            }
        }

        /// <summary>Notify both sides of every overlap between one entity and a group.</summary>
        public static void Resolve<TB>(ICollidable single, IReadOnlyList<TB> group)
            where TB : class, ICollidable
        {
            if (single == null || !single.Active)
                return;

            var bounds = single.Bounds;

            for (int j = 0; j < group.Count; j++)
            {
                var b = group[j];
                if (!b.Active)
                    continue;

                if (!bounds.Intersects(b.Bounds))
                    continue;

                single.OnCollision(b);
                b.OnCollision(single);

                if (!single.Active)
                    return;
            }
        }
    }
}
