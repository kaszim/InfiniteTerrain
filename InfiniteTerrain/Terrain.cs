using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InfiniteTerrain
{
    /// <summary>
    /// Handles all terrain related logic, such as drawing and collisions.
    /// </summary>
    class Terrain
    {
        QuadTree quadTree;
        // The games graphicsdevice.
        private GraphicsDevice graphicsDevice;
        // The spritebatch for this terrain instance.
        private SpriteBatch spriteBatch;
        private int width;
        private int height;

        /// <summary>
        /// Constructs a terrain object.
        /// </summary>
        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            this.graphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;

            spriteBatch = new SpriteBatch(graphicsDevice);
            quadTree = new QuadTree(graphicsDevice, new Rectangle(0, 0, 500, 500), QuadTreeType.Texture);
        }

        /// <summary>
        /// Runs update on the terrain.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
                quadTree.Insert(new Rectangle(mouseState.X-50, mouseState.Y-50, 100, 100), QuadTreeType.Empty);
            else if (mouseState.RightButton == ButtonState.Pressed)
                quadTree.Insert(new Rectangle(mouseState.X - 50, mouseState.Y - 50, 100, 100), QuadTreeType.Texture);
        }

        /// <summary>
        /// This is called when the terrain should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            quadTree.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
