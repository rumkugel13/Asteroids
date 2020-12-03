using Kadro.UI;
using Kadro;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroids.Shared
{
    public static class GameConfig
    {
        private static Point PlayAreaSize => new Point(3840, 2160);//(3840, 2160); //size of active world where entities can move in, also worldbounds
        private static Point PlayAreaPosition => new Point((WorldSize.X - PlayAreaSize.X) / 2, (WorldSize.Y - PlayAreaSize.Y) / 2);
        public static Rectangle PlayArea => new Rectangle(PlayAreaPosition, PlayAreaSize);

        public static Point MinWindowSize => new Point(960, 540);

        public static Point WorldSize => NumCells * CellSize;// new Point(10240, 6144);//  (5120, 3072); //multiple of cellsize, for spatial hash grid
        public static Point CellSize => new Point(170);
        //TODO: remove unnecessary references (like numcells)
        //note: 2*cellsize is for border around playarea
        public static Point NumCells => new Point((int)Math.Ceiling((PlayAreaSize.X + 2 * CellSize.X) / 170f), (int)Math.Ceiling((PlayAreaSize.Y + 2 * CellSize.Y) / 170f));

        public static int Countdown => 3;
        public static int AsteroidAmount => 40; //10; for 3840x2160 playareasize

        public struct Fonts
        {
            public static SpriteFont VerySmall;
            public static SpriteFont Small;
            public static SpriteFont Medium;
            public static SpriteFont Large;
            public static SpriteFont VeryLarge;
        }

        public struct Folders
        {
            public static string Textures = "Textures";
            public static string Particles = "Particles";
            public static string SpriteSheets = "SpriteSheets";
            public static string Fonts = "Fonts";
            public static string VertexLists = "VertexListData";
        }

        public struct Spaceship
        {
            public static float Size => 32f;//32f;  // at scale 1.0f
            public static float MaxVelocity => 200f;//200f;
            public static float MaxAngularVelocity => 5f;
            public static float AccelerationRate => 400f;//400f;
            public static float AngularAccelerationRate => 40f;
            public static float GunCooldown => 0.3f;
            public static Vector2 GunPosition => new Vector2(0, -0.75f * Size); // -y is up
            public static float RespawnTimeout => 3;
            public static int Lives => 3;
            public static int Score => 0;
            public static string DefaultTexture => "spaceship_type_1";
            public static Vector2 Origin => new Vector2(191, 358); // with spaceship_type_1
            public static string CollisionLayer => "Player";
            public static string Tag => "Spaceship";
            public static string FlameAnimationSheet => "flame_animation_sheet";
            public static string FlameAnimation => "flame_animation";

            // hack: hardcode collider values
            public static Vector2[] PolygonCollider = new Vector2[]
            {
                new Vector2(0f, -0.69921875f), new Vector2(0.373046875f, 0f), new Vector2(0.19921875f, 0.30078125f), new Vector2(-0.19921875f, 0.30078125f), new Vector2(-0.373046875f, 0f)
            };
        }

        public struct Asteroid
        {
            public static float Large => 86f;//384f;//128f;
            public static float Medium => 54f;//192f;//64f;
            public static float Small => 32f;//32f;
            public static float MinScale => Small / Large;
            public static float Velocity => 70f;//50f;
            public static float AngularVelocity => 1;
            public static string DefaultTexture => "asteroid_new2-512";
            public static string[] Textures => new string[] { "asteroid_new-512", "asteroid_new2-512" };
            public static string CollisionLayer => "Asteroid";
            public static string Tag => "Asteroid";
        }

        public struct Projectile
        {
            public static float Size => 4f;//8f;
            public static float Lifetime => 2f;
            public static float Velocity => 400f;//750f;//300f;
            public static string DefaultTexture => "triangle_filled-32";
            public static string CollisionLayer => "Projectile";
            public static string Tag => "Projectile";
        }

        public struct Border
        {
            public static string CollisionLayer => "Border";
            public static string Tag => "Border";
        }
    }
}
