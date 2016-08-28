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
        private GraphicsDevice graphicsDevice;
        private Terrain terrain;
        // the texture to use for chunkGenerating
        private Texture2D texture;
        private SpriteBatch spriteBatch;
        private Effect terrainShader;
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Constructs the world generator object.
        /// </summary>
        /// <param name="terrain">The world's terrain.</param>
        /// <param name="graphicsDevice">The GraphicsDevice.</param>
        public WorldGenerator(Terrain terrain, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.terrain = terrain;
        }

        /// <summary>
        /// Loads World Generator content.
        /// </summary>
        /// <param name="content">todo: describe content parameter on LoadContent</param>
        public void LoadContent(ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
            texture = new Texture2D(graphicsDevice, terrain.ChunkWidth, terrain.ChunkHeight);
            renderTarget = new RenderTarget2D(graphicsDevice, terrain.ChunkWidth,
                terrain.ChunkHeight);
            terrainShader = content.Load<Effect>("TerrainGeneration");
        }

        /// <summary>
        /// Generates the specified chunk.
        /// </summary>
        /// <param name="location">Which chunk to generate.</param>
        public void GenerateChunk(Vector2 location)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            terrainShader.Parameters["camPos"].SetValue(location);
            terrainShader.Parameters["amp"].SetValue(5f);
            terrainShader.Parameters["freq"].SetValue(0.5f);
            terrainShader.Parameters["yOffset"].SetValue(1f);
            terrainShader.Parameters["snowFalloff"].SetValue(0.2f);
            terrainShader.Parameters["octaves"].SetValue(8);
            terrainShader.Parameters["persistence"].SetValue(0.1f);
            spriteBatch.Begin(effect: terrainShader);
            {
                spriteBatch.Draw(texture, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
            terrain.ApplyTexture(renderTarget, new Vector2(location.X*terrain.ChunkWidth,
                location.Y*terrain.ChunkHeight));
        }

        /// <summary>
        /// Generate an area of terrain.
        /// </summary>
        /// <param name="rectangle">The surrounding rectangle (inclusive).</param>
        public void GenerateArea(Rectangle rectangle)
        {
            for (int x = rectangle.X; x <= (rectangle.X + rectangle.Width); x++)
            {
                for (int y = rectangle.Y; y <= (rectangle.Y + rectangle.Height); y++)
                {
                    GenerateChunk(new Vector2(x, y));
                }
            }
        }
    }
}
