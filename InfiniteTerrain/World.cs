using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using InfiniteTerrain.GameObjects;

namespace InfiniteTerrain
{
    class World
    {
        private readonly GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private readonly Terrain terrain;
        private readonly HashSet<IGameObject> gameObjects;

        /// <summary>
        /// Creates and initializes a World.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice from Game class.</param>
        public World(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            terrain = new Terrain(graphicsDevice, 10000, 5000);
            gameObjects = new HashSet<IGameObject>();
        }

        /// <summary>
        /// Loads World content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
            for(int i = 0; i < 50; i++)
                gameObjects.Add(new Player());
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            terrain.Update(gameTime);
            foreach (var gObject in gameObjects)
            {
                gObject.Update(gameTime);
                var recs = terrain.GetCollidingRectangles(gObject.Rectangle, QuadTreeType.Texture);
                foreach (Rectangle other in recs)
                {
                    var dCenter = gObject.Rectangle.Center - other.Center;
                    float dx;
                    float dy;
                    // Check which side we intersect from and calculate the dx accordingly
                    if (dCenter.X > 0)
                        dx = other.X + other.Size.X - gObject.Position.X;
                    else
                        dx = other.X - gObject.Position.X - gObject.Rectangle.Width;
                    // same thing here but with dy
                    if (dCenter.Y > 0)
                        dy = other.Y + other.Size.Y - gObject.Position.Y;
                    else
                        dy = other.Y - gObject.Position.Y - gObject.Rectangle.Height;
                    // Whichever of the d's are smallest should be solved first, leave the other for
                    // next update
                    var dPos = Math.Abs(dx) > Math.Abs(dy) ? new Vector2(0, dy) : new Vector2(dx, 0);
                    gObject.Position += dPos;
                }
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
            foreach (var gObject in gameObjects)
            {
                gObject.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public void Add(GameObject o)
        {
            gameObjects.Add(o);
        }
    }
}
