using Kadro.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Kadro.Extensions;
using Kadro;

namespace Asteroids.Shared
{
    public class Minimap : Panel
    {
        private Dictionary<Point, Color> items = new Dictionary<Point, Color>();
        Vector2 mapSize = new Vector2(192, 108);

        public Minimap() : base()
        {
            this.Alignment = Alignment.TopRight;
            this.PreferredSize = mapSize.ToPoint();
            this.PreferredPosition = new Point(-25, 25);
            this.Border.Color = Color.White;
            this.Border.Thickness = 1;
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public void Add(Vector2 vector, Color color)
        {
            //convert from realworld coordinates to minimap coordinates
            Point mapPos = ((vector - GameConfig.PlayArea.Location.ToVector2()) / GameConfig.PlayArea.Size.ToVector2() * mapSize).ToPoint() + this.ActualPosition;
            this.items[mapPos] = color;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (var item in this.items)
            {
                spriteBatch.DrawPixel(item.Key.ToVector2() / WindowSettings.UnitScale, item.Value);
            }
        }
    }
}