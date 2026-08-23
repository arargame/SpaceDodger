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

        public SupportCreditsScreen(Core.GameContext context) : base(context) { }

        public override void Load()
        {
            _paintTrek = Context.Textures.Get("ui/paint_trek");
            _blocked = Context.Textures.Get("ui/blocked");
            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 120f)
                .Add("BUY ME A COFFEE", BuyCoffee)
                .Add("OPEN BLOCKED", () => Context.Platform.OpenUrl(ArarGamesApplications.BlockedGooglePlay))
                .Add("OPEN PAINT TREK", () => Context.Platform.OpenUrl(ArarGamesApplications.PaintTrekGooglePlay))
                .Add("BACK", () => Context.Screens.Pop());
        }

        private void BuyCoffee() => Context.Platform.PurchaseConsumable(ArarGamesApplications.CoffeeProductId);

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed) { Context.Screens.Pop(); return; }
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "ARAR GAMES", cx, 12, new Color(255, 220, 60), 2f);
            Context.Font.DrawCentered(spriteBatch, "APPLICATIONS", cx, 32, Color.White);
            spriteBatch.Draw(_blocked, new Rectangle(52, 50, 50, 50), Color.White);
            spriteBatch.Draw(_paintTrek, new Rectangle(218, 50, 50, 50), Color.White);
            Context.Font.DrawCentered(spriteBatch, "BLOCKED", 77, 103, new Color(150,160,190));
            Context.Font.DrawCentered(spriteBatch, "PAINT TREK", 243, 103, new Color(150,160,190));
            _menu.Draw(spriteBatch);
        }
    }
}
