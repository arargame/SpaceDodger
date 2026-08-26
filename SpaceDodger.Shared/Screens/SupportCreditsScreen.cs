using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceDodger.Input;
using ArarGames.Core.Applications;

namespace SpaceDodger.Screens
{
    public sealed class SupportCreditsScreen : Screen
    {
        private MenuList _menu;
        private Texture2D _paintTrek;
        private Texture2D _blocked;
        private Texture2D _iconAndroid;
        private Texture2D _iconMsStore;
        
        private readonly Rectangle _blockedRect = new Rectangle(50, 42, 48, 48);
        private readonly Rectangle _paintRect = new Rectangle(210, 42, 48, 48);

        // Clickable market areas
        private readonly Rectangle _blockedAndroidBtn = new Rectangle(110, 56, 44, 20);
        
        private readonly Rectangle _paintAndroidBtn = new Rectangle(270, 44, 44, 20);
        private readonly Rectangle _paintMsStoreBtn = new Rectangle(270, 68, 44, 20);

        public SupportCreditsScreen(Core.GameContext context) : base(context) { }

        public override void Load()
        {
            _paintTrek = Context.Textures.Get("ui/paint_trek");
            _blocked = Context.Textures.Get("ui/blocked");
            _iconAndroid = Context.Textures.Get("ui/market_icons_android");
            _iconMsStore = Context.Textures.Get("ui/market_icons_microsoftstore");
            
            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 138f)
                .Add("BUY ME A COFFEE", BuyCoffee)
                .Add("BACK", () => Context.Screens.Pop());
        }

        private void BuyCoffee() => Context.Platform.PurchaseConsumable(ArarGamesApplications.CoffeeProductId);

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed) { Context.Screens.Pop(); return; }
            if (input.Tap.HasValue)
            {
                var p = input.Tap.Value;
                // Oyun logolarına tıklandığında da ana mağazaya (Android) gitsin
                if (_blockedRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (_paintRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                
                // Market icon butonları
                if (_blockedAndroidBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (_paintAndroidBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                if (_paintMsStoreBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekMicrosoftStore); return; }
            }
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "ARAR GAMES", cx, 12, new Color(255, 220, 60), 2f);
            Context.Font.DrawCentered(spriteBatch, "APPLICATIONS", cx, 32, Color.White);
            
            // Draw high-res UI elements with Linear filter so they don't look badly crushed
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.LinearClamp, blendState: BlendState.NonPremultiplied);
            
            // Draw game icons
            spriteBatch.Draw(_blocked, _blockedRect, Color.White);
            spriteBatch.Draw(_paintTrek, _paintRect, Color.White);
            
            // Draw market icons for Blocked
            spriteBatch.Draw(_iconAndroid, _blockedAndroidBtn, Color.White);
            
            // Draw market icons for Paint Trek
            spriteBatch.Draw(_iconAndroid, _paintAndroidBtn, Color.White);
            spriteBatch.Draw(_iconMsStore, _paintMsStoreBtn, Color.White);
            
            // Revert back to Point filter for pixel text and the rest of the UI
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            
            Context.Font.DrawCentered(spriteBatch, "BLOCKED", 74, 95, new Color(150,160,190));
            Context.Font.DrawCentered(spriteBatch, "PAINT TREK", 234, 95, new Color(150,160,190));
            
            _menu.Draw(spriteBatch);
        }
    }
}
