using System;
using Microsoft.Xna.Framework;
using SpaceDodger.Entities;

namespace SpaceDodger.Movement
{
    public sealed class LerpMovement : IMovementStrategy
    {
        private const float ParkOffset = 46f;
        
        public void Move(Enemy enemy, EnemyWorld world, float dt)
        {
            float targetX = world.Bounds.Right - ParkOffset;
            
            if (enemy.Position.X > targetX + 1f && enemy.Age < 2f)
            {
                enemy.Position.X -= enemy.EffectiveSpeed * dt;
                return;
            }

            float centerY = world.Bounds.Center.Y;
            float sweep = world.Bounds.Height / 3f;
            
            float[] positionsY = new float[] 
            { 
                centerY, 
                centerY - sweep, 
                centerY + sweep, 
                centerY - sweep / 2f 
            };
            
            float cycleDuration = 4.5f;
            float totalCycle = cycleDuration * positionsY.Length;
            
            // Offset age by 2s for entry
            float activeAge = Math.Max(0, enemy.Age - 2f);
            
            float modAge = activeAge % totalCycle;
            int currentIndex = (int)(modAge / cycleDuration);
            int nextIndex = (currentIndex + 1) % positionsY.Length;
            
            float phaseTime = modAge % cycleDuration;
            
            float currentY = positionsY[currentIndex];
            float nextY = positionsY[nextIndex];
            
            float lerpDuration = 1.5f;
            float holdDuration = 3.0f;
            
            if (phaseTime < holdDuration)
            {
                enemy.Position.Y = currentY;
            }
            else
            {
                float t = (phaseTime - holdDuration) / lerpDuration;
                t = t * t * (3f - 2f * t); // smoothstep
                enemy.Position.Y = MathHelper.Lerp(currentY, nextY, t);
            }
            enemy.Position.X = targetX;
        }
    }
}
