using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceImpact.Input;

namespace SpaceImpact.Screens
{
    /// <summary>Persistent player preferences. Audio switches are ready for
    /// the music and SFX assets that will be added to the project later.</summary>
    public sealed class OptionsScreen : Screen
    {
        private MenuList _menu;

        public OptionsScreen(Core.GameContext context) : base(context) { }

        public override void Load() => BuildMenu();

        private void BuildMenu()
        {
            _menu = new MenuList(Context.Font, Context.Screen.Width / 2f, 78f)
                .Add($"MUSIC: {OnOff(Context.Save.Data.MusicEnabled)}", ToggleMusic)
                .Add($"SOUND FX: {OnOff(Context.Save.Data.SoundEnabled)}", ToggleSound)
                .Add("BACK", Back);
        }

        private static string OnOff(bool enabled) => enabled ? "ON" : "OFF";

        private void ToggleMusic()
        {
            Context.Save.Data.MusicEnabled = !Context.Save.Data.MusicEnabled;
            Context.Save.Save();
            BuildMenu();
        }

        private void ToggleSound()
        {
            Context.Save.Data.SoundEnabled = !Context.Save.Data.SoundEnabled;
            Context.Save.Save();
            BuildMenu();
        }

        private void Back() => Context.Screens.Pop();

        public override void Update(float dt, in InputState input)
        {
            if (input.BackPressed)
            {
                Back();
                return;
            }
            _menu.Update(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float cx = Context.Screen.Width / 2f;
            Context.Font.DrawCentered(spriteBatch, "OPTIONS", cx, 30, Color.White, 2f);
            Context.Font.DrawCentered(spriteBatch, "AUDIO SETTINGS", cx, 52, new Color(150, 160, 190));
            _menu.Draw(spriteBatch);
        }
    }
}
