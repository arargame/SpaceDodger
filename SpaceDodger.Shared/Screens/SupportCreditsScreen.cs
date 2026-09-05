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
        
        private readonly Rectangle _blockedRect = new Rectangle(36, 42, 48, 48);
        private readonly Rectangle _paintRect = new Rectangle(186, 42, 48, 48);

        // Clickable market areas
        private readonly Rectangle _blockedAndroidBtn = new Rectangle(90, 44, 44, 20);
        private readonly Rectangle _blockedMsStoreBtn = new Rectangle(90, 68, 44, 20);
        
        private readonly Rectangle _paintAndroidBtn = new Rectangle(240, 44, 44, 20);
        private readonly Rectangle _paintMsStoreBtn = new Rectangle(240, 68, 44, 20);

        public SupportCreditsScreen(Core.GameContext context) : base(context) { }

        public override void Load()
        {
            _paintTrek = Context.Textures.Get("ui/paint_trek");
            _blocked = Context.Textures.Get("ui/blocked");
            _iconAndroid = Context.Textures.Get("ui/market_icons_android");
            _iconMsStore = Context.Textures.Get("ui/market_icons_microsoftstore");
            
            string removeAdsLabel = Context.Save.Data.AdsRemoved ? "ADS REMOVED (ACTIVE)" : "REMOVE ADS";

            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 116f)
                .Add("BUY ME A COFFEE", BuyCoffee)
                .Add(removeAdsLabel, BuyRemoveAds)
                .Add("RESTORE PURCHASES", RestorePurchases)
                .Add("BACK", () => Context.Screens.Pop());
        }

        private void BuyCoffee() => Context.Platform.PurchaseConsumable(ArarGamesApplications.CoffeeProductId);

        private void BuyRemoveAds()
        {
            if (Context.Save.Data.AdsRemoved) return;
            Context.Platform.PurchaseNonConsumable(ArarGamesApplications.RemoveAdsProductId);
        }

        private void RestorePurchases() => Context.Platform.RestorePurchases();

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
                if (_blockedMsStoreBtn.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedMicrosoftStore); return; }
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
            
            Context.Font.DrawCentered(spriteBatch, "BLOCKED", 60, 95, new Color(150,160,190));
            Context.Font.DrawCentered(spriteBatch, "PAINT TREK", 210, 95, new Color(150,160,190));
            
            _menu.Draw(spriteBatch);
        }

        /// <summary>Draw high-res game posters and market icons at real screen resolution.</summary>
        public override void DrawHighRes(SpriteBatch spriteBatch, Graphics.VirtualScreen screen)
        {
            // Map virtual rectangles to physical backbuffer coordinates
            spriteBatch.Draw(_blocked, screen.ToPhysical(_blockedRect), Color.White);
            spriteBatch.Draw(_paintTrek, screen.ToPhysical(_paintRect), Color.White);
            spriteBatch.Draw(_iconAndroid, screen.ToPhysical(_blockedAndroidBtn), Color.White);
            spriteBatch.Draw(_iconMsStore, screen.ToPhysical(_blockedMsStoreBtn), Color.White);
            spriteBatch.Draw(_iconAndroid, screen.ToPhysical(_paintAndroidBtn), Color.White);
            spriteBatch.Draw(_iconMsStore, screen.ToPhysical(_paintMsStoreBtn), Color.White);
        }
    }
}
