using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Input;
using ArarGames.Core.Applications;

namespace SpaceImpact.Screens
{
    public sealed class SupportCreditsScreen : Screen
    {
        private MenuList _menu;
        private Texture2D _paintTrek;
        private Texture2D _blocked;
        private Texture2D _markets;
        private readonly Rectangle _blockedRect = new Rectangle(32, 48, 54, 54);
        private readonly Rectangle _paintRect = new Rectangle(234, 48, 54, 54);

        public SupportCreditsScreen(Core.GameContext context) : base(context) { }

        public override void Load()
        {
            _paintTrek = Context.Textures.Get("ui/paint_trek");
            _blocked = Context.Textures.Get("ui/blocked");
            _markets = Context.Textures.Get("ui/market_icons");
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
                if (_blockedRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (_paintRect.Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                if (new Rectangle(95, 48, 52, 18).Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay); return; }
                if (new Rectangle(160, 48, 52, 18).Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay); return; }
                if (new Rectangle(160, 92, 52, 18).Contains((int)p.X, (int)p.Y)) { Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekMicrosoftStore); return; }
            }
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "ARAR GAMES", cx, 12, new Color(255, 220, 60), 2f);
            Context.Font.DrawCentered(spriteBatch, "APPLICATIONS", cx, 32, Color.White);
            spriteBatch.Draw(_blocked, _blockedRect, Color.White);
            spriteBatch.Draw(_paintTrek, _paintRect, Color.White);
            DrawMarket(spriteBatch, 95, 48, 0); DrawMarket(spriteBatch, 95, 70, 2); DrawMarket(spriteBatch, 95, 92, 1);
            DrawMarket(spriteBatch, 160, 48, 0); DrawMarket(spriteBatch, 160, 70, 2); DrawMarket(spriteBatch, 160, 92, 1);
            Context.Font.DrawCentered(spriteBatch, "BLOCKED", 59, 105, new Color(150,160,190));
            Context.Font.DrawCentered(spriteBatch, "PAINT TREK", 261, 105, new Color(150,160,190));
            _menu.Draw(spriteBatch);
        }

        private void DrawMarket(SpriteBatch batch, int x, int y, int index) =>
            batch.Draw(_markets, new Rectangle(x, y, 52, 18), new Rectangle(index * 124, 0, 124, 83), Color.White);
    }
}
