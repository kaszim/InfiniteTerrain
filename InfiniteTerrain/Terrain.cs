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
        Texture2D texture;
        // The games graphicsdevice.
        private GraphicsDevice graphicsDevice;
        // The spritebatch for this terrain instance.
        private SpriteBatch spriteBatch;
        private int width;
        private int height;
        private TerrainChunk tChunk;

        class TerrainChunk
        {
            private GraphicsDevice graphicsDevice;
            private RenderTarget2D renderTarget;
            private SpriteBatch spriteBatch;
            private int width;
            private int height;

            public TerrainChunk(GraphicsDevice graphicsDevice, int width, int height)
            {
                this.graphicsDevice = graphicsDevice;
                this.width = width;
                this.height = height;
                renderTarget = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                spriteBatch = new SpriteBatch(graphicsDevice);
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Green);
                graphicsDevice.SetRenderTarget(null);
            }

            public void Modify(Texture2D modifier, Vector2 position)
            {
                graphicsDevice.SetRenderTarget(renderTarget);
                spriteBatch.Begin(blendState: BlendState.Opaque);
                spriteBatch.Draw(modifier, position, Color.White);
                spriteBatch.End();
                graphicsDevice.SetRenderTarget(null);
            }

            public void Draw(SpriteBatch spriteBatch, Vector2 position)
            {
                spriteBatch.Draw(renderTarget, position, Color.White);
            }
        }

        /// <summary>
        /// Constructs a terrain object.
        /// </summary>
        public Terrain(GraphicsDevice graphicsDevice, int width, int height)
        {
            this.graphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            spriteBatch = new SpriteBatch(graphicsDevice);
            quadTree = new QuadTree(new Rectangle(0, 0, 500, 500), QuadTreeType.Texture);
            texture = new Texture2D(graphicsDevice, 100, 100);
            Color[] colors = new Color[texture.Width * texture.Height];
            Color c = Color.Transparent;
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors[x + y * texture.Height] = c;
                }
            }
            texture.SetData(colors);
            tChunk = new TerrainChunk(graphicsDevice, 500, 500);
        }

        /// <summary>
        /// Runs update on the terrain.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                quadTree.Insert(new Rectangle(mouseState.X - 50, mouseState.Y - 50, 100, 100), QuadTreeType.Empty);
                tChunk.Modify(texture, new Vector2(mouseState.X - 50, mouseState.Y - 50));
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                quadTree.Insert(new Rectangle(mouseState.X - 50, mouseState.Y - 50, 100, 100), QuadTreeType.Texture);
                tChunk.Modify(texture, new Vector2(mouseState.X - 50, mouseState.Y - 50));
            }
        }

        /// <summary>
        /// This is called when the terrain should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            tChunk.Draw(spriteBatch, Vector2.Zero);
            spriteBatch.End();
        }
    }
}
