using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    class World
    {
        private readonly GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private readonly Terrain terrain;
        private readonly GameObject o;

        /// <summary>
        /// Creates and initializes a World.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice from Game class.</param>
        public World(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            terrain = new Terrain(graphicsDevice, 10000, 5000);
            o = new GameObject(new Vector2(500, 500), new Point(25, 75));

        }

        /// <summary>
        /// Loads World content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            terrain.Update(gameTime);
            o.Update(gameTime);
            var recs = terrain.GetCollidingRectangles(o.Rectangle, QuadTreeType.Texture);
            foreach (Rectangle other in recs)
            {
                var dCenter = o.Rectangle.Center - other.Center;
                float dx;
                float dy;
                // Check which side we intersect from and calculate the dx accordingly
                if (dCenter.X > 0)
                    dx = other.X + other.Size.X - o.Position.X;
                else
                    dx = other.X - o.Position.X - o.Rectangle.Width;
                // same thing here but with dy
                if (dCenter.Y > 0)
                    dy = other.Y + other.Size.Y - o.Position.Y;
                else
                    dy = other.Y - o.Position.Y - o.Rectangle.Height;
                // Whichever of the d's are smallest should be solved first, leave the other for
                // next update
                var dPos = Math.Abs(dx) > Math.Abs(dy) ? new Vector2(0, dy) : new Vector2(dx, 0);
                o.Position += dPos;
            }
        }

        /// <summary>
        /// Draws the world and all objects in the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            terrain.Draw(gameTime);
            spriteBatch.Begin();
            o.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
