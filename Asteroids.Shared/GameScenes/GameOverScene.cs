using Asteroids.Shared;
using Kadro.Input;
using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids.Shared
{
    public class GameOverScene : GameScene
    {
        private TextBlock text;
        private GUIScene screen;
        private Button exit;

        public GameOverScene(Game game) : base(game)
        {
            this.screen = new GUIScene();

            this.text = new TextBlock(GameConfig.Fonts.Large, "Game Over! Press Escape to get back to main menu");
            this.text.Alignment = Alignment.Center;
            this.text.PreferredPosition = new Point(0, -30);
            this.screen.AddChild(this.text);

            this.exit = new Button(GameConfig.Fonts.Medium, "Exit");
            this.exit.Alignment = Alignment.Center;
            this.exit.PreferredPosition = new Point(0, 30);
            this.screen.AddChild(this.exit);
        }

        protected override void OnEnter()
        {
            this.Game.IsMouseVisible = true;
            GUISceneManager.SwitchScene(this.screen);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (Kadro.Input.KeyboardInput.OnKeyUp(Keys.Escape) || this.exit.OnClick())
            {
                SwitchScene<MainMenuScene>();
                return;
            }
        }
    }
}