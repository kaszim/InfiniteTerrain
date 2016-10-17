using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace InfiniteTerrain
{
    /// <summary>
    /// Handles all terrain related logic, such as drawing and collisions.
    /// </summary>
    class Terrain
    {
        #region Fields
        readonly Texture2D texture;
        readonly Texture2D texture2;
        private readonly int chunkWidth;
        private readonly int chunkHeight;
        // The maximum number of chunks that are visible horizontally
        private readonly int nChunksHorizontal;
        // The maximum number of chunks that are visible vertically
        private readonly int nChunksVertical;
        // The games graphicsdevice.
        private readonly GraphicsDevice graphicsDevice;
        // The spritebatch for this terrain instance.
        private readonly SpriteBatch spriteBatch;
        private List<List<TerrainChunk>> chunks;
        // in chunks
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Returns the number of chunks that are in this terrain object, horizontally.
        /// </summary>
        public int NumberOfChunksHorizontally => chunks.Count;
        /// <summary>
        /// Returns the number of chunks that are in this terrain object, vertically.
        /// </summary>
        public int NumberOfChunksVertically => chunks[0].Count;

        /// <summary>
        /// How wide the chunks are.
        /// </summary>
        public int ChunkWidth => chunkWidth;
        /// <summary>
        /// How high the chunks are.
        /// </summary>
        public int ChunkHeight => chunkHeight;

        /// <summary>
        /// If set to true, things like a wireframe for the quadtree will be drawn.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Returns the color of a certain pixel.
        /// </summary>
        /// <returns>The color of the specified pixel.</returns>
        public Color this[int x, int y]
        {
            get
            {
                // Get which chunk the position is in
                var cX = (int)Math.Floor((double)x / ChunkWidth);
                var cY = (int)Math.Floor((double)y / ChunkHeight);
                var chunk = chunks[cX][cY];
                // Get the internal position in the chunk
                var iX = x - cX * ChunkWidth;
                var iY = y - cY * ChunkHeight;
                return chunk[iX, iY];
            }
        }

        /// <summary>
        /// A inverse opaque blendstate. Essentially meaning that the texture removes instead
        /// of overwriting.
        /// </summary>
        public static BlendState InverseOpaque = new BlendState
        {
            AlphaSourceBlend = Blend.Zero,
            ColorSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceColor,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaBlendFunction = BlendFunction.ReverseSubtract
        };
    #endregion

    /// <summary>
    /// A chunk of terrain.
    /// </summary>
    class TerrainChunk
        {
            private readonly Terrain terrain;
            private readonly RenderTarget2D renderTarget;
            private readonly GraphicsDevice graphicsDevice;
            private readonly SpriteBatch spriteBatch;
            private readonly Rectangle rectangle;
            private readonly QuadTree quadTree;
            private readonly Vector2 position;
            private Color[] colorData;

            /// <summary>
            /// Returns the color of a certain pixel. Uses internal position of pixel.
            /// </summary>
            /// <returns>The color of the specified pixel.</returns>
            public Color this[int x, int y]
            {
                get
                {
                    if (colorData == null)
                    {
                        colorData = new Color[renderTarget.Height * renderTarget.Width];
                        renderTarget.GetData<Color>(colorData);
                    }
                    return colorData[x + y * renderTarget.Width];
                }
            }

            /// <summary>
            /// Creates a terrain chunk.
            /// </summary>
            /// <param name="graphicsDevice">The game's graphicsdevice.</param>
            /// <param name="terrain">The Terrain object.</param>
            /// <param name="rectangle">The destination rectangle of this chunk.</param>
            public TerrainChunk(GraphicsDevice graphicsDevice, Terrain terrain, Rectangle rectangle)
            {
                this.graphicsDevice = graphicsDevice;
                this.terrain = terrain;
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
            /// Modifies the chunk of terrain only updating the QuadTree.
            /// </summary>
            /// <param name="rectangle">todo: describe rectangle parameter on Modify</param>
            /// <param name="quadTreeType">todo: describe quadTreeType parameter on Modify</param>
            public void Modify(Rectangle rectangle, QuadTreeType quadTreeType)
            {
                // Insert a modifier rectangle into the quadtree.
                quadTree.Insert(rectangle, quadTreeType);
            }

            /// <summary>
            /// Modifies the chunk of terrain without updating the QuadTree.
            /// </summary>
            /// <param name="modifier"></param>
            /// <param name="position">The external position to modify.</param>
            /// <param name="blendstate"></param>
            /// <param name="effect"></param>
            public void Modify(Texture2D modifier, Vector2 position, BlendState blendstate,
                Effect effect)
            {
                // Translate the external position to internal
                var iPos = position - this.position;
                // Set the rendertarget to this chunk's
                graphicsDevice.SetRenderTarget(renderTarget);
                // Begin drawing to the spritebatch, using blendsate.opaque
                // (this blendstate removes previously drawn colors, and only leaves the current drawn ones)
                spriteBatch.Begin(blendState: blendstate, effect: effect);
                // Draw the modifier texture to the rendertarget.
                spriteBatch.Draw(modifier, iPos, Color.White);
                spriteBatch.End();
                // Set to the main rendertarget.
                graphicsDevice.SetRenderTarget(null);
                // Set colorData to null since we changed the data
                colorData = null;
            }

            /// <summary>
            /// Modifies the chunk of terrain and updates the underlying QuadTree.
            /// </summary>
            /// <param name="modifier"></param>
            /// <param name="position">The external position to modify.</param>
            /// <param name="blendstate"></param>
            /// <param name="effect"></param>
            /// <param name="quadTreeType"></param>
            public void Modify(Texture2D modifier, Vector2 position, BlendState blendstate,
                Effect effect, QuadTreeType quadTreeType)
            {
                Modify(modifier, position, blendstate, effect);
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
                if(terrain.Debug)
                    quadTree.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Constructs a terrain object.
        /// </summary>
        /// <param name="height">In pixels</param>
        /// <param name="width">In pixels</param>
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
        }

        #region private helpers
        /// <summary>
        /// Applies a method on every visible chunk.
        /// </summary>
        /// <param name="action">The method to invoke.</param>
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
        /// Applies a method on every chunk in a specified area.
        /// The area is specified in chunk coordinates.
        /// </summary>
        /// <param name="area">The area rectangle.</param>
        /// <param name="action">The method to invoke.</param>
        private void forEachChunkInArea(Rectangle area, Action<TerrainChunk> action)
        {
            var rectangles = new List<Rectangle>();
            var x = area.X; // Auto Floor because of int
            var y = area.Y;
            var xmin = Math.Max(x, 0);
            var ymin = Math.Max(y, 0);
            var xmax = Math.Min(x + area.Width, NumberOfChunksHorizontally - 1);
            var ymax = Math.Min(y + area.Height, NumberOfChunksVertically - 1);
            for (x = xmin; x <= xmax; x++)
            {
                for (y = ymin; y <= ymax; y++)
                {
                    action?.Invoke(chunks[x][y]);
                }
            }
        }
        #endregion

        #region mutators
        /// <summary>
        /// Returns all rectangles which collides with the search rectangle and
        /// has the same type as the searchType. This search is done through all quadtrees.
        /// </summary>
        /// <param name="searchRectangle">The rectangle to test against.</param>
        /// <param name="searchType">The QuadTreeType to test against.</param>
        /// <returns>A list of rectangles in the terrain which collides wiht searchRectangle.</returns>
        public List<Rectangle> GetCollidingRectangles(Rectangle searchRectangle, QuadTreeType searchType)
        {
            var rectangles = new List<Rectangle>();
            foreach(var xbucket in chunks)
                foreach(var chunk in xbucket)
                {
                    rectangles.AddRange(chunk.FindCollidingRectangles(searchRectangle, searchType));
                }
            return rectangles;
        }

        /// <summary>
        /// Returns all rectangles which collides with the search rectangle and
        /// has the same type as the searchType. Only searches within specified boundaries.
        /// For example: if supplied with the boundary {2, 2} and search rectangle position {2,2}
        /// the search would be in the following interval: X: [0, 4] Y: [0, 4].
        /// </summary>
        /// <param name="searchRectangle">The rectangle to test against.</param>
        /// <param name="searchType">The QuadTreeType to test against.</param>
        /// <param name="boundary">The specied boundary to search within.</param>
        /// <returns>A list of rectangles in the terrain which collides wiht searchRectangle.</returns>
        public List<Rectangle> GetCollidingRectangles(Rectangle searchRectangle, QuadTreeType searchType, Point boundary)
        {
            var rectangles = new List<Rectangle>();
            var x = searchRectangle.X / chunkWidth;
            var y = searchRectangle.Y / chunkHeight;
            var xmin = Math.Max(x - boundary.X, 0);
            var ymin = Math.Max(y - boundary.Y, 0);
            var xmax = Math.Min(x + boundary.X, NumberOfChunksHorizontally - 1);
            var ymax = Math.Min(y + boundary.Y, NumberOfChunksVertically - 1);
            forEachChunkInArea(new Rectangle(xmin, ymin, boundary.X, boundary.Y),
                (c) => rectangles.AddRange(c.FindCollidingRectangles(searchRectangle, searchType)));
            return rectangles;
        }

        /// <summary>
        /// Modifies the underlying collision quadtree.
        /// </summary>
        /// <param name="rectangle">The rectangle to apply.</param>
        /// <param name="type">The new type of the area modified.</param>
        public void ModifyQuadTree(Rectangle rectangle, QuadTreeType type)
        {
            // TODO: choose area according to rectangle size.
            var x = (int)rectangle.X / chunkWidth;
            var y = (int)rectangle.Y / chunkHeight;
            forEachChunkInArea(new Rectangle(x, y, 1, 1),
                c => c.Modify(rectangle, type));
        }

        /// <summary>
        /// Applies a texture to the terrain on the specified position.
        /// Without updating the underlying QuadTree's.
        /// </summary>
        /// <param name="texture">The texture to apply.</param>
        /// <param name="position">The position to apply the texture to.</param>
        /// <param name="effect"></param>
        /// <param name="blendstate"></param>
        public void ApplyTexture(Texture2D texture, Vector2 position, BlendState blendstate,
            Effect effect)
        {
            //TODO: Depending on the size of the texture, choose area accordinly
            var x = (int)position.X / chunkWidth;
            var y = (int)position.Y / chunkHeight;
            forEachChunkInArea(new Rectangle(x, y, 1, 1),
                (c) => c.Modify(texture, position, blendstate, effect));
        }

        /// <summary>
        /// Applies a texture to the terrain on the specified position.
        /// Also updates the underlying QuadTree structure with the specified type.
        /// </summary>
        /// <param name="texture">The texture to apply.</param>
        /// <param name="position">The position to apply the texture to.</param>
        /// <param name="blendstate"></param>
        /// <param name="effect"></param>
        /// <param name="type">The new type of the QuadTree.</param>
        public void ApplyTexture(Texture2D texture, Vector2 position, BlendState blendstate,
            Effect effect, QuadTreeType type)
        {
            //TODO: Depending on the size of the texture, choose area accordinly
            var x = (int)position.X / chunkWidth;
            var y = (int)position.Y / chunkHeight;
            forEachChunkInArea(new Rectangle(x, y, 1, 1),
                (c) => c.Modify(texture, position, blendstate, effect, type));
        }
        #endregion

        #region XNA methods
        /// <summary>
        /// Loads World content.
        /// </summary>
        /// <param name="content">todo: describe content parameter on LoadContent</param>
        public void LoadContent(ContentManager content)
        {
            // Initialize the chunk dictionary
            chunks = new List<List<TerrainChunk>>();
            for (int x = 0; x < this.width; x++)
            {
                chunks.Add(new List<TerrainChunk>());
                for (int y = 0; y < this.height; y++)
                {
                    chunks[x].Add(new TerrainChunk(graphicsDevice, this, new Rectangle(x * chunkWidth,
                        y * chunkHeight, chunkWidth, chunkHeight)));
                }
            }
        }

        /// <summary>
        /// Runs update on the terrain.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
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
        #endregion
    }
}
