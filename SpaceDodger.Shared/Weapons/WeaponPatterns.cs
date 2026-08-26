using Microsoft.Xna.Framework;

namespace SpaceDodger.Weapons
{
    /// <summary>Single straight shot — weapon level 1.</summary>
    public sealed class SingleShot : IWeaponPattern
    {
        public int ShotCount => 1;
        public int Damage => 1;
        public bool Plasma => false;

        public Vector2 OffsetFor(int index) => Vector2.Zero;

        public Vector2 VelocityFor(int index, float speed, float directionX) =>
            new Vector2(speed * directionX, 0f);
    }

    /// <summary>Twin parallel shots — weapon level 2.</summary>
    public sealed class DoubleShot : IWeaponPattern
    {
        public int ShotCount => 2;
        public int Damage => 1;
        public bool Plasma => false;

        public Vector2 OffsetFor(int index) => new Vector2(0f, index == 0 ? -3f : 3f);

        public Vector2 VelocityFor(int index, float speed, float directionX) =>
            new Vector2(speed * directionX, 0f);
    }

    /// <summary>Three-way spread — weapon level 3.</summary>
    public sealed class SpreadShot : IWeaponPattern
    {
        private const float SpreadY = 55f;

        public int ShotCount => 3;
        public int Damage => 1;
        public bool Plasma => false;

        public Vector2 OffsetFor(int index) => new Vector2(0f, (index - 1) * 2f);

        public Vector2 VelocityFor(int index, float speed, float directionX) =>
            new Vector2(speed * directionX, (index - 1) * SpreadY);
    }

    /// <summary>
    /// Twin forward plasma plus an angled pair — weapon level 4.
    /// Shots 0/1 run parallel, shots 2/3 fan out.
    /// </summary>
    public sealed class HeavySpread : IWeaponPattern
    {
        private const float SpreadY = 62f;

        public int ShotCount => 4;
        public int Damage => 2;
        public bool Plasma => true;

        public Vector2 OffsetFor(int index) =>
            index < 2
                ? new Vector2(0f, index == 0 ? -3f : 3f)
                : new Vector2(-2f, index == 2 ? -5f : 5f);

        public Vector2 VelocityFor(int index, float speed, float directionX) =>
            index < 2
                ? new Vector2(speed * directionX, 0f)
                : new Vector2(speed * directionX * 0.9f, index == 2 ? -SpreadY : SpreadY);
    }

    /// <summary>Five-way plasma storm — weapon level 5 (maximum).</summary>
    public sealed class StormShot : IWeaponPattern
    {
        private const float SpreadY = 48f;

        public int ShotCount => 5;
        public int Damage => 2;
        public bool Plasma => true;

        public Vector2 OffsetFor(int index) => new Vector2(0f, (index - 2) * 2.5f);

        public Vector2 VelocityFor(int index, float speed, float directionX) =>
            new Vector2(speed * directionX, (index - 2) * SpreadY);
    }

    /// <summary>Maps a weapon level (1..5) to its pattern (shared instances).</summary>
    public static class WeaponRegistry
    {
        private static readonly IWeaponPattern[] ByLevel =
        {
            new SingleShot(),   // level 1
            new DoubleShot(),   // level 2
            new SpreadShot(),   // level 3
            new HeavySpread(),  // level 4
            new StormShot(),    // level 5
        };

        public static int MaxLevel => ByLevel.Length;

        public static IWeaponPattern ForLevel(int level)
        {
            int index = MathHelper.Clamp(level, 1, ByLevel.Length) - 1;
            return ByLevel[index];
        }
    }
}
