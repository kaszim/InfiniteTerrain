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
    class WorldGenerator
    {
        private static Random random = new Random();
        private GraphicsDevice graphicsDevice;
        // the texture to use for chunkGenerating
        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private Effect terrainShader;
        private RenderTarget2D renderTarget;
        private Vector2 offset;

        public Terrain Terrain { get; set; }

        /// <summary>
        /// Constructs the world generator object.
        /// </summary>
        /// <param name="terrain">The world's terrain.</param>
        /// <param name="graphicsDevice">The GraphicsDevice.</param>
        public WorldGenerator(Terrain terrain, GraphicsDevice graphicsDevice)
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
            texture = new Texture2D(graphicsDevice, Terrain.ChunkWidth, Terrain.ChunkHeight);
            renderTarget = new RenderTarget2D(graphicsDevice, Terrain.ChunkWidth,
                Terrain.ChunkHeight);
            terrainShader = content.Load<Effect>("TerrainGeneration");
        }

        /// <summary>
        /// Generates the specified chunk.
        /// </summary>
        /// <param name="location">Which chunk to generate.</param>
        public void GenerateChunk(Vector2 location)
        {
            // Get the texture for this part
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            terrainShader.Parameters["camPos"].SetValue(location + offset);
            terrainShader.Parameters["amp"].SetValue(5f);
            terrainShader.Parameters["freq"].SetValue(0.4f);
            terrainShader.Parameters["yOffset"].SetValue(3f);
            terrainShader.Parameters["snowFalloff"].SetValue(0.2f);
            terrainShader.Parameters["octaves"].SetValue(8);
            terrainShader.Parameters["scale"].SetValue(0.5f);
            terrainShader.Parameters["persistence"].SetValue(0.05f);
            spriteBatch.Begin(effect: terrainShader);
            {
                spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);

            // Create the collider
            var colorData = new Color[renderTarget.Width * renderTarget.Height];
            renderTarget.GetData<Color>(colorData);
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
