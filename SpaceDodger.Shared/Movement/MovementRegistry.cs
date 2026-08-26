using System;
using System.Collections.Generic;

namespace SpaceDodger.Movement
{
    /// <summary>
    /// Maps movement names used in level JSON to shared strategy instances.
    /// Adding a new pattern = one class + one line here (Open/Closed).
    /// </summary>
    public static class MovementRegistry
    {
        private static readonly Dictionary<string, IMovementStrategy> Strategies =
            new Dictionary<string, IMovementStrategy>(StringComparer.OrdinalIgnoreCase)
            {
                ["straight"] = new StraightMovement(),
                ["sine"] = new SineMovement(),
                ["zigzag"] = new ZigZagMovement(),
                ["chase"] = new ChaseMovement(),
                ["boss"] = new BossMovement(),
                ["lerp_boss"] = new LerpMovement(),
                ["orbit_boss"] = new OrbitMovement(),
                ["drift_boss"] = new DriftMovement(),
                ["sine_boss"] = new SineBossMovement(),
            };

        public static IMovementStrategy Get(string name) =>
            Strategies.TryGetValue(name, out var s) ? s : Strategies["straight"];
    }
}
