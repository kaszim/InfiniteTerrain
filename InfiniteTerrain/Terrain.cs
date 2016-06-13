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
        Texture2D texture2;
        private const int chunkWidth = 560;
        private const int chunkHeight = 300;
        // The games graphicsdevice.
        private GraphicsDevice graphicsDevice;
        // The spritebatch for this terrain instance.
        private SpriteBatch spriteBatch;
        private List<List<TerrainChunk>> chunks;
        private int width;
        private int height;

        /// <summary>
        /// A chunk of terrain.
        /// </summary>
        class TerrainChunk
        {
            private RenderTarget2D renderTarget;
            private GraphicsDevice graphicsDevice;
            private SpriteBatch spriteBatch;
            private Rectangle rectangle;
            private QuadTree quadTree;
            private Vector2 position;

            /// <summary>
            /// Creates a terrain chunk.
            /// </summary>
            /// <param name="graphicsDevice">The game's graphicsdevice.</param>
            /// <param name="rectangle">The destination rectangle of this chunk.</param>
            public TerrainChunk(GraphicsDevice graphicsDevice, Rectangle rectangle)
            {
                this.graphicsDevice = graphicsDevice;
                this.rectangle = rectangle;
                position = new Vector2(rectangle.X, rectangle.Y);
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
            /// <param name="position">The external position to modify.</param>
            /// <param name="quadTreeType"></param>
            public void Modify(Texture2D modifier, Vector2 position, QuadTreeType quadTreeType)
            {
                // Translate the external position to internal
                var iPos = position - this.position;
                // Set the rendertarget to this chunk's
                graphicsDevice.SetRenderTarget(renderTarget);
                // Begin drawing to the spritebatch, using blendsate.opaque
                // (this blendstate removes previously drawn colors, and only leaves the current drawn ones)
                spriteBatch.Begin(blendState: BlendState.Opaque);
                // Draw the modifier texture to the rendertarget.
                spriteBatch.Draw(modifier, iPos, Color.White);
                spriteBatch.End();
                // Set to the main rendertarget.
                graphicsDevice.SetRenderTarget(null);
                // Insert a modifier rectangle into the quadtree.
                quadTree.Insert(new Rectangle((int)position.X, (int)position.Y, modifier.Width, modifier.Height), quadTreeType);
            }

            /// <summary>
            /// Draws the terrain chunk to the spritebatch.
            /// </summary>
            /// <param name="spriteBatch">The spritebatch to draw to.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                // Draws this chunk of terrain to the spritebatch
                spriteBatch.Draw(renderTarget, Camera.WorldToScreenPosition(position), Color.White);
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
            texture2 = new Texture2D(graphicsDevice, 100, 100);
            Color[] colors = new Color[texture.Width * texture.Height];
            Color[] colors2 = new Color[texture.Width * texture.Height];
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors[x + y * texture.Height] = Color.Transparent;
                    colors2[x + y * texture2.Height] = Color.Green;
                }
            }
            texture.SetData(colors);
            texture2.SetData(colors2);
            // Initialize the chunk dictionary
            chunks = new List<List<TerrainChunk>>();
            for (int x = 0; x < 50; x++)
            {
                chunks.Add(new List<TerrainChunk>());
                for (int y = 0; y < 50; y++)
                {
                    chunks[x].Add(new TerrainChunk(graphicsDevice, new Rectangle(x * chunkWidth, y * chunkHeight, chunkWidth, chunkHeight)));
                }
            }
        }

        private void forEachVisibleChunk(Action<TerrainChunk> action)
        {
            var chunksHori = Math.Ceiling((double)Camera.Size.X / chunkWidth) + 1;
            var chunksVert = Math.Ceiling((double)Camera.Size.Y / chunkHeight) + 1;
            var currCHori = (int)Camera.Position.X / chunkWidth;
            var currCVert = (int)Camera.Position.Y / chunkHeight;
            var lastChunkHori = Math.Min(currCHori + chunksHori, 50);
            var lastChunkVert = Math.Min(currCVert + chunksVert, 50);
            for (int x = currCHori; x < lastChunkHori; x++)
                for (int y = currCVert; y < lastChunkVert; y++)
                    action(chunks[x][y]);
        }

        /// <summary>
        /// Runs update on the terrain.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePos = Camera.ScreenToWorldPosition(new Vector2(mouseState.X, mouseState.Y));
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (texture.Width >> 1), mousePos.Y - (texture.Height >> 1));
                forEachVisibleChunk((c) => c.Modify(texture, center, QuadTreeType.Empty));
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (texture.Width >> 1), mousePos.Y - (texture.Height >> 1));
                foreach (var xbucket in chunks)
                    foreach (var chunk in xbucket)
                    {
                        chunk.Modify(texture2, center, QuadTreeType.Texture);
                    }
            }
        }

        /// <summary>
        /// This is called when the terrain should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            forEachVisibleChunk((c) => c.Draw(spriteBatch));
            spriteBatch.End();
        }
    }
}
