using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace InfiniteTerrain
{
    class TerrainGenerator
    {
        private static Random random = new Random();
        private static OpenSimplexNoise noise = new OpenSimplexNoise();
        private const int distanceBetweenBorders = 23;

        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        private Vector2 offset;
        private Texture2D tile;
        private Texture2D border;
        private float[] heightMap;

        public Terrain Terrain { get; set; }

        /// <summary>
        /// Constructs the world generator object.
        /// </summary>
        /// <param name="terrain">The world's terrain.</param>
        /// <param name="graphicsDevice">The GraphicsDevice.</param>
        public TerrainGenerator(Terrain terrain, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.Terrain = terrain;
            offset = new Vector2(10000f / random.Next(5000), 0);
        }

        /// <summary>
        /// Loads World Generator content.
        /// </summary>
        /// <param name="content">todo: describe content parameter on LoadContent</param>
        public void LoadContent(ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
            renderTarget = new RenderTarget2D(graphicsDevice, Terrain.ChunkWidth,
                Terrain.ChunkHeight);
            tile = content.Load<Texture2D>("grassCenter");
            border = content.Load<Texture2D>("h3");
        }

        /// <summary>
        /// Generates the specified chunk.
        /// </summary>
        /// <param name="location">Which chunk to generate.</param>
        public void GenerateChunk(Vector2 location)
        {
            // Fill the terrain with a tile
            graphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin();
            {
                for (int x = 0; x <= renderTarget.Width; x += tile.Width)
                    for (int y = 0; y <= renderTarget.Height; y += tile.Height)
                        spriteBatch.Draw(tile, new Vector2(x, y), Color.White);

            }
            spriteBatch.End();

            // Carve out terrain
            var colorData = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData<Color>(colorData);
            heightMap = new float[renderTarget.Width];
            for (int x = 0; x < renderTarget.Width; x += 1)
            {
                var pn = noise.Evaluate(0.001f * (x + location.X * Terrain.ChunkWidth), 1f) * 500f + 1000f;
                heightMap[x] = (int)pn;
                // Carve out terrain
                int y;
                for (y = (int)location.Y*Terrain.ChunkHeight; y < pn; y++)
                {
                    colorData[x + y * renderTarget.Width] = Color.Transparent;
                }

            }
            renderTarget.SetData<Color>(colorData);

            // Draw borders
            //TODO: left-most border is not drawn
            spriteBatch.Begin();
            var X = renderTarget.Width-1;
            var startY = (int)location.Y * Terrain.ChunkHeight;
            while (X - 10 > 0 && heightMap[X] > startY)
            {
                var ang = (float)Math.Atan2(heightMap[X] - heightMap[X-10], 10);
                var vec = new Vector2(distanceBetweenBorders, 0);
                var mat = Matrix.CreateRotationZ(ang);
                vec = Vector2.Transform(vec, mat);
                var diff = (int)Math.Round(vec.X) - distanceBetweenBorders;
                spriteBatch.Draw(border, position: new Vector2(X, heightMap[X] - 5), rotation: ang,
                    origin: new Vector2(border.Width/2f, 0));
                X -= diff + distanceBetweenBorders + 1;
            }
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            // Create the collider
            for (int x = 0; x < renderTarget.Width; x += 3)
            {
                // Find the pixel which is not a color
                int y;
                for (y = 0; y < renderTarget.Height; y += 1)
                {
                    if (colorData[x + y * renderTarget.Width] != Color.Transparent)
                        break;
                }
                var rec = new Rectangle(((int)location.X) * Terrain.ChunkWidth + x, ((int)location.Y) * Terrain.ChunkHeight, 3, y);
                Terrain.ModifyQuadTree(rec, TerrainType.Empty);
            }
            colorData = null;

            Terrain.ApplyTexture(renderTarget, new Vector2(location.X*Terrain.ChunkWidth,
                location.Y*Terrain.ChunkHeight), BlendState.Opaque, null);
        }

        /// <summary>
        /// Generate an area of terrain.
        /// </summary>
        /// <param name="rectangle">The surrounding rectangle (inclusive).</param>
        public void GenerateArea(Rectangle rectangle)
        {
            for (int x = rectangle.X; x < (rectangle.X + rectangle.Width); x++)
            {
                for (int y = rectangle.Y; y < (rectangle.Y + rectangle.Height); y++)
                {
                    GenerateChunk(new Vector2(x, y));
                }
            }
        }
    }
}
