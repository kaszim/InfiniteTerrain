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
        readonly Texture2D texture;
        readonly Texture2D texture2;
        private readonly int chunkWidth;
        private readonly int chunkHeight;
        // The maximum number of chunks that fit into the screen horizontally
        private readonly int nChunksHorizontal;
        // The maximum number of chunks that fit into the screen vertically
        private readonly int nChunksVertical;
        // The games graphicsdevice.
        private readonly GraphicsDevice graphicsDevice;
        // The spritebatch for this terrain instance.
        private readonly SpriteBatch spriteBatch;
        private readonly List<List<TerrainChunk>> chunks;
        // in chunks
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// A chunk of terrain.
        /// </summary>
        class TerrainChunk
        {
            private readonly RenderTarget2D renderTarget;
            private readonly GraphicsDevice graphicsDevice;
            private readonly SpriteBatch spriteBatch;
            private readonly Rectangle rectangle;
            private readonly QuadTree quadTree;
            private readonly Vector2 position;

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
                renderTarget = new RenderTarget2D(graphicsDevice, rectangle.Width, rectangle.Height,
                    false,
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
                quadTree.Insert(new Rectangle((int)position.X, (int)position.Y, modifier.Width,
                    modifier.Height), quadTreeType);
            }

            public List<Rectangle> FindCollidingRectangles(Rectangle searchRectangle,
                QuadTreeType searchType) => quadTree.FindCollidingLeaves(searchRectangle, searchType);

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
            chunkWidth = Camera.Size.X / 2;
            chunkHeight = Camera.Size.Y / 2;
            this.width = (int)Math.Ceiling((double)width/chunkWidth);
            this.height = (int)Math.Ceiling((double)height /chunkHeight);
            spriteBatch = new SpriteBatch(graphicsDevice);
            nChunksHorizontal = (int)Math.Ceiling((double)Camera.Size.X / chunkWidth) + 1;
            nChunksVertical = (int)Math.Ceiling((double)Camera.Size.Y / chunkHeight) + 1;

            // Placeholder
            texture = new Texture2D(graphicsDevice, 100, 100);
            texture2 = new Texture2D(graphicsDevice, 100, 100);
            var colors = new Color[texture.Width * texture.Height];
            var colors2 = new Color[texture.Width * texture.Height];
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
            for (int x = 0; x < this.width; x++)
            {
                chunks.Add(new List<TerrainChunk>());
                for (int y = 0; y < this.height; y++)
                {
                    chunks[x].Add(new TerrainChunk(graphicsDevice, new Rectangle(x * chunkWidth,
                        y * chunkHeight, chunkWidth, chunkHeight)));
                }
            }
        }

        /// <summary>
        /// Does something with on every visible chunk.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        private void forEachVisibleChunk(Action<TerrainChunk> action)
        {
            var currCHori = (int)Camera.Position.X / chunkWidth;
            var currCVert = (int)Camera.Position.Y / chunkHeight;
            var lastChunkHori = currCHori + nChunksHorizontal;
            var lastChunkVert = currCVert + nChunksVertical;

            if (currCHori < 0)
                currCHori = 0;
            else if (lastChunkHori > width)
                lastChunkHori = chunks.Count;
            if (currCVert < 0)
                currCVert = 0;
            else if (lastChunkVert > height)
                lastChunkVert = chunks[0].Count;

            for (int x = currCHori; x < lastChunkHori; x++)
                for (int y = currCVert; y < lastChunkVert; y++)
                    action?.Invoke(chunks[x][y]);
        }

        /// <summary>
        /// Returns all rectangles which collides with the search rectangle and
        /// has the same type as the searchType.
        /// </summary>
        /// <param name="searchRectangle">The rectangle to test against.</param>
        /// <param name="searchType">The QuadTreeType to test against.</param>
        /// <returns>A list of rectangles in the terrain which collides wiht searchRectangle.</returns>
        public List<Rectangle> GetCollidingRectangles(Rectangle searchRectangle, QuadTreeType searchType)
        {
            //TODO: Only test against nearby terrainchunks
            var rectangles = new List<Rectangle>();
            foreach(var xbucket in chunks)
                foreach(var chunk in xbucket)
                {
                    rectangles.AddRange(chunk.FindCollidingRectangles(searchRectangle, searchType));
                }
            return rectangles;
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
                forEachVisibleChunk((c) => c.Modify(texture2, center, QuadTreeType.Texture));
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
