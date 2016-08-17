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
            var colors = new Color[texture.Width * texture.Height];
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors[x + y * texture.Height] = Color.White;
                }
            }
            texture.SetData(colors);
        }

        public void GenerateChunk(Vector2 location)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            terrainShader.Parameters["camPos"].SetValue(location);
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
        /// 
        /// </summary>
        /// <param name="rectangle">inclusive</param>
        public void GenerateArea(Rectangle rectangle)
        {
            for (float x = rectangle.X; x <= rectangle.Width; x++)
                for (float y = rectangle.Y; y <= rectangle.Height; y++)
                {
                    GenerateChunk(new Vector2(x, y));
                }
        }
    }
}
