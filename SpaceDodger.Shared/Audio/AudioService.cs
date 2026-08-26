using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using SpaceDodger.Persistence;

namespace SpaceDodger.Audio
{
    public sealed class AudioService : IDisposable
    {
        private readonly Dictionary<string, SoundEffect> _effects = new Dictionary<string, SoundEffect>();
        private readonly SaveData _settings;
        public AudioService(SaveData settings) => _settings = settings;

        public void Play(string name, float volume)
        {
            if (!_settings.SoundEnabled) return;
            try
            {
                if (!_effects.TryGetValue(name, out var effect))
                {
                    using var stream = TitleContainer.OpenStream($"Content/sounds/{name}.wav");
                    effect = SoundEffect.FromStream(stream);
                    _effects[name] = effect;
                }
                effect.Play(MathHelper.Clamp(volume, 0f, 1f), 0f, 0f);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            foreach (var effect in _effects.Values) effect.Dispose();
            _effects.Clear();
        }
    }
}
