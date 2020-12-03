using Kadro.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kadro;

namespace Asteroids.Shared
{
    public class Scoreboard : Panel
    {
        private Dictionary<uint, ScoreboardItem> items = new Dictionary<uint, ScoreboardItem>();
        private SpriteFont font;
        private int i = 0;
        private int gap = 25;
        private ScoreboardItem myScore;
        private uint myEntityId;
        private int offsetY;
        private bool showLatency;

        public Scoreboard(SpriteFont spriteFont, bool showLatency) : base()
        {
            this.showLatency = showLatency;
            this.font = spriteFont;
            this.Color = Color.Black;
            this.Opacity = 0.75f;
            this.Alignment = Alignment.Center;
            this.PreferredSize = new Point(500);
            this.AddChild(new Border(this));

            TextBlock header = new TextBlock(this.font, "Scoreboard");
            header.Alignment = Alignment.Top;
            header.PreferredPosition = new Point(0, 20);
            this.AddChild(header);
            this.offsetY = 50;
        }

        public void Initialize()
        {
            foreach (ScoreboardItem s in this.items.Values)
            {
                this.RemoveChild(s);
            }
            this.items.Clear();
            if (myScore != null)
            {
                this.RemoveChild(myScore);
            }
        }

        public void AddOrUpdate(uint entityId, int score, int lives)
        {
            if (!this.items.ContainsKey(entityId))
            {
                this.items.Add(entityId, new ScoreboardItem(this.font));
                this.items[entityId].EntityIdText.Text = "Id: " + entityId;
                this.items[entityId].Opacity = 0f;
                this.items[entityId].PreferredPosition += (Vector2.UnitY * (i++ * this.gap + offsetY)).ToPoint();

                if (!this.showLatency)
                {
                    this.items[entityId].PreferredSize = new Point(360, 40);
                    this.items[entityId].Latency.SetVisible(false);
                }

                this.AddChild(this.items[entityId]);
            }

            this.items[entityId].ScoreText.Text = "Score: " + score;
            this.items[entityId].LivesText.Text = "Lives: " + lives;

            if (entityId == myEntityId && myScore != null)
            {
                myScore.ScoreText.Text = "Score: " + score;
                myScore.LivesText.Text = "Lives: " + lives;
            }
        }

        public void AddOrUpdate(uint entityId, float ping)
        {
            if (!this.items.ContainsKey(entityId))
            {
                this.items.Add(entityId, new ScoreboardItem(this.font));
                this.items[entityId].EntityIdText.Text = "Id: " + entityId;
                this.items[entityId].Opacity = 0f;
                this.items[entityId].PreferredPosition += (Vector2.UnitY * (i++ * this.gap + this.offsetY)).ToPoint();

                if (!this.showLatency)
                {
                    this.items[entityId].PreferredSize = new Point(360, 40);
                    this.items[entityId].Latency.SetVisible(false);
                }

                this.AddChild(this.items[entityId]);
            }

            if (this.showLatency)
            {
                this.items[entityId].Latency.Text = $"Ping: {ping:N1}";
            }

            ////if (entityId == myEntityId && myScore != null)
            ////{
            ////    myScore.Latency.Text = $"Ping: {ping:N1}";
            ////}
        }

        public void SetMyEntity(uint entityId)
        {
            myEntityId = entityId;
            myScore = new ScoreboardItem(this.font);
            myScore.Opacity = 0.75f;
            myScore.EntityIdText.Text = "Id: " + entityId;

            // do not show latency
            myScore.PreferredSize = new Point(360, 40);
            myScore.Latency.SetVisible(false);
        }

        protected override void OnUpdate()
        {
            myScore?.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            myScore?.Draw(spriteBatch);
        }
    }
}
