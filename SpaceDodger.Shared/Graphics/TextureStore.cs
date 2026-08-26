using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDodger.Graphics
{
    /// <summary>
    /// Loads and caches raw PNG textures through <see cref="TitleContainer"/>
    /// (works from the executable folder on desktop and from APK assets on
    /// Android — no MGCB content pipeline required).
    /// Premultiplies alpha at load time so the default AlphaBlend state works.
    /// </summary>
    public sealed class TextureStore : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly Dictionary<string, Texture2D> _cache =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        private Texture2D _pixel;

        public TextureStore(GraphicsDevice device) => _device = device;

        /// <summary>1x1 white texture for rectangles, bars and dim overlays.</summary>
        public Texture2D Pixel
        {
            get
            {
                if (_pixel == null)
                {
                    _pixel = new Texture2D(_device, 1, 1);
                    _pixel.SetData(new[] { Color.White });
                }
                return _pixel;
            }
        }

        /// <summary>Get a texture by content-relative name, e.g. "sprites/player".</summary>
        public Texture2D Get(string name)
        {
            if (_cache.TryGetValue(name, out var cached))
                return cached;

            Texture2D texture;
            using (var stream = TitleContainer.OpenStream($"Content/{name}.png"))
            {
                texture = Texture2D.FromStream(_device, stream);
            }

            PremultiplyAlpha(texture);
            _cache[name] = texture;
            return texture;
        }

        private static void PremultiplyAlpha(Texture2D texture)
        {
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.FromNonPremultiplied(data[i].R, data[i].G, data[i].B, data[i].A);
            texture.SetData(data);
        }

        public void Dispose()
        {
            foreach (var t in _cache.Values)
                t.Dispose();
            _cache.Clear();
            _pixel?.Dispose();
        }
    }
}
