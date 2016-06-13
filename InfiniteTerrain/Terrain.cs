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
            private Rectangle rectangle;
            private QuadTree quadTree;

            /// <summary>
            /// Creates a terrain chunk.
            /// </summary>
            /// <param name="graphicsDevice">The game's graphicsdevice.</param>
            /// <param name="rectangle">The destination rectangle of this chunk.</param>
            public TerrainChunk(GraphicsDevice graphicsDevice, Rectangle rectangle)
            {
                this.graphicsDevice = graphicsDevice;
                this.rectangle = rectangle;
                this.quadTree = new QuadTree(rectangle, QuadTreeType.Texture);
                renderTarget = new RenderTarget2D(graphicsDevice, rectangle.Width, rectangle.Height, false,
                    SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
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
            /// <param name="quadTreeType"></param>
            public void Modify(Texture2D modifier, Vector2 position, QuadTreeType quadTreeType)
            {
                // Set the rendertarget to this chunk's
                graphicsDevice.SetRenderTarget(renderTarget);
                // Begin drawing to the spritebatch, using blendsate.opaque
                // (this blendstate removes previously drawn colors, and only leaves the current drawn ones)
                spriteBatch.Begin(blendState: BlendState.Opaque);
                // Draw the modifier texture to the rendertarget.
                spriteBatch.Draw(modifier, position, Color.White);
                spriteBatch.End();
                // Set to the main rendertarget.
                graphicsDevice.SetRenderTarget(null);
                // Insert a modifier rectangle into the quadtree.
                quadTree.Insert(new Rectangle((int)position.X, (int)position.Y, modifier.Width, modifier.Height), QuadTreeType.Empty);
            }

            /// <summary>
            /// Draws the terrain chunk to the spritebatch.
            /// </summary>
            /// <param name="spriteBatch">The spritebatch to draw to.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                // Draws this chunk of terrain to the spritebatch
                spriteBatch.Draw(renderTarget, rectangle, Color.White);
                quadTree.Draw(spriteBatch);
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
            tChunk = new TerrainChunk(graphicsDevice, new Rectangle(0,0, 512, 512));
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
                // Get the center of the modifier
                var center = new Vector2(mouseState.X - (texture.Width >> 1), mouseState.Y - (texture.Height >> 1));
                tChunk.Modify(texture, center, QuadTreeType.Empty);
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mouseState.X - (texture.Width >> 1), mouseState.Y - (texture.Height >> 1));
                tChunk.Modify(texture, center, QuadTreeType.Texture);
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
            spriteBatch.End();
        }
    }
}
