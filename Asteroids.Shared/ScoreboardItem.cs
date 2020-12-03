using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Shared
{
    public class ScoreboardItem : Panel
    {
        public TextBlock EntityIdText;

        public TextBlock ScoreText;

        public TextBlock LivesText;

        public TextBlock Latency;

        public ScoreboardItem(SpriteFont spriteFont) : base()
        {
            EntityIdText = new TextBlock(spriteFont, "Id: ");
            EntityIdText.Alignment = Alignment.Left;
            EntityIdText.PreferredPosition = new Point(20, 0);
            this.AddChild(EntityIdText);

            ScoreText = new TextBlock(spriteFont, "Score: ");
            ScoreText.Alignment = Alignment.Left;
            ScoreText.PreferredPosition = new Point(130, 0);
            this.AddChild(ScoreText);

            LivesText = new TextBlock(spriteFont, "Lives: ");
            LivesText.Alignment = Alignment.Left;
            LivesText.PreferredPosition = new Point(260, 0);
            this.AddChild(LivesText);

            Latency = new TextBlock(spriteFont, "Ping: ");
            Latency.Alignment = Alignment.Left;
            Latency.PreferredPosition = new Point(380, 0);
            this.AddChild(Latency);

            this.Color = Color.Black;
            this.PreferredSize = new Point(480, 40);
        }
    }
}
