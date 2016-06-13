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

        /// <summary>
        /// A chunk of terrain.
        /// </summary>
        class TerrainChunk
        {
            private GraphicsDevice graphicsDevice;
            private RenderTarget2D renderTarget;
            private SpriteBatch spriteBatch;
            private Vector2 position;
            private int width;
            private int height;

            /// <summary>
            /// Creates a terrain chunk.
            /// </summary>
            /// <param name="graphicsDevice">The game's graphicsdevice.</param>
            /// <param name="position">The world position of the chunk.</param>
            /// <param name="width">Width of the chunk.</param>
            /// <param name="height">Height of the chunk.</param>
            public TerrainChunk(GraphicsDevice graphicsDevice, Vector2 position, int width, int height)
            {
                this.graphicsDevice = graphicsDevice;
                this.position = position;
                this.width = width;
                this.height = height;
                renderTarget = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                spriteBatch = new SpriteBatch(graphicsDevice);
                // Initialize the rendertarget.
                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Green);
                graphicsDevice.SetRenderTarget(null);
            }

            /// <summary>
            /// Modifies the chunk of terrain.
            /// </summary>
            /// <param name="modifier"></param>
            /// <param name="position"></param>
            public void Modify(Texture2D modifier, Vector2 position)
            {
                // Set the rendertarget to this chunk's
                graphicsDevice.SetRenderTarget(renderTarget);
                // Begin drawing to the spritebatch, using blendsate.opaque (this blendstate removes previously drawn colors, and only leaves the current drawn ones)
                spriteBatch.Begin(blendState: BlendState.Opaque);
                // Draw the modifier texture to the rendertarget.
                spriteBatch.Draw(modifier, position, Color.White);
                spriteBatch.End();
                // Set to the main rendertarget.
                graphicsDevice.SetRenderTarget(null);
            }

            /// <summary>
            /// Draws the terrain chunk to the spritebatch.
            /// </summary>
            /// <param name="spriteBatch">The spritebatch to draw to.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                // Draws this chunk of terrain to the spritebatch
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
            tChunk = new TerrainChunk(graphicsDevice, Vector2.Zero, 500, 500);
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
            tChunk.Draw(spriteBatch);
            quadTree.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
