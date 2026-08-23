using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceImpact.Graphics
{
    /// <summary>
    /// Fixed low-resolution render target scaled to any physical screen size
    /// with integer-friendly point sampling and letterboxing.
    /// Also converts physical (touch/mouse) coordinates back to virtual space.
    /// </summary>
    public sealed class VirtualScreen
    {
        public int Width { get; }
        public int Height { get; }

        private readonly GraphicsDevice _device;
        private readonly RenderTarget2D _target;

        private Rectangle _destination;

        public VirtualScreen(GraphicsDevice device, int width, int height)
        {
            _device = device;
            Width = width;
            Height = height;
            _target = new RenderTarget2D(device, width, height);
        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public void BeginCapture()
        {
            _device.SetRenderTarget(_target);
        }

        public void EndCapture(SpriteBatch spriteBatch)
        {
            _device.SetRenderTarget(null);
            _device.Clear(Color.Black);

            _destination = ComputeDestination();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_target, _destination, Color.White);
            spriteBatch.End();
        }

        /// <summary>Physical screen position (pixels) to virtual coordinates.</summary>
        public Vector2 ToVirtual(Vector2 physical)
        {
            var dest = _destination.Width > 0 ? _destination : ComputeDestination();
            float scaleX = (float)dest.Width / Width;
            float scaleY = (float)dest.Height / Height;
            return new Vector2(
                (physical.X - dest.X) / scaleX,
                (physical.Y - dest.Y) / scaleY);
        }

        private Rectangle ComputeDestination()
        {
            int screenW = _device.PresentationParameters.BackBufferWidth;
            int screenH = _device.PresentationParameters.BackBufferHeight;

            float scale = System.Math.Min((float)screenW / Width, (float)screenH / Height);
            int w = (int)(Width * scale);
            int h = (int)(Height * scale);
            return new Rectangle((screenW - w) / 2, (screenH - h) / 2, w, h);
        }
    }
}
