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
    class TerrainTheme
    {
        public string FillTileLocation { get; set; }
        public string BorderTextureLocation { get; set; }
        public int DistanceBetweenBorders { get; set; }
        public int OffsetBetweenBorders { get; set; }
        public Point BorderOffsetTerrain { get; set; }
        public int DistanceBetweenBorderReads { get; set; }
    }

    class TerrainGenerator
    {
        private static Random random = new Random();
        private static OpenSimplexNoise noise = new OpenSimplexNoise();

        private GraphicsDevice graphicsDevice;
        private RenderTarget2D renderTarget;
        private TerrainTheme terrainTheme;
        private SpriteBatch spriteBatch;
        private Texture2D tile;
        private Texture2D border;

        public Terrain Terrain { get; set; }

        /// <summary>
        /// Constructs the world generator object.
        /// </summary>
        /// <param name="terrain">The world's terrain.</param>
        /// <param name="graphicsDevice">The GraphicsDevice.</param>
        /// <param name="terrainTheme">The theme to use for the terrain generator.</param>
        public TerrainGenerator(Terrain terrain, GraphicsDevice graphicsDevice, 
            TerrainTheme terrainTheme)
        {
            this.graphicsDevice = graphicsDevice;
            this.Terrain = terrain;
            this.terrainTheme = terrainTheme;
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
            tile = content.Load<Texture2D>(terrainTheme.FillTileLocation);
            border = content.Load<Texture2D>(terrainTheme.BorderTextureLocation);
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
            var heightMap = new float[renderTarget.Width];
            for (int x = 0; x < renderTarget.Width; x += 1)
            {
                var pn = noise.Evaluate(0.001f * (x + location.X * Terrain.ChunkWidth), 1f) * 500f + 1000f;
                heightMap[x] = (int)pn;
                // Carve out terrain
                int y;
                for (y = (int)location.Y * Terrain.ChunkHeight; y < pn; y++)
                {
                    colorData[x + y * renderTarget.Width] = Color.Transparent;
                }

            }
            renderTarget.SetData<Color>(colorData);
            colorData = null;

            // Draw borders
            drawBorders(location, heightMap);
            graphicsDevice.SetRenderTarget(null);

            // Create the collider
            createCollider(location);

            Terrain.ApplyTexture(renderTarget, new Vector2(location.X*Terrain.ChunkWidth,
                location.Y*Terrain.ChunkHeight), BlendState.Opaque, null);
        }

        private void drawBorders(Vector2 location, float[] heightMap)
        {
            spriteBatch.Begin();
            var X = renderTarget.Width - 1;
            var startY = (int)location.Y * Terrain.ChunkHeight;
            while (X - terrainTheme.DistanceBetweenBorderReads > 0 && heightMap[X] > startY)
            {
                var ang = (float)Math.Atan2(
                    heightMap[X] - heightMap[X - terrainTheme.DistanceBetweenBorderReads],
                    terrainTheme.DistanceBetweenBorderReads);
                var vec = new Vector2(terrainTheme.DistanceBetweenBorders, 0);
                var mat = Matrix.CreateRotationZ(ang);
                vec = Vector2.Transform(vec, mat);
                var diff = (int)Math.Round(vec.X) - terrainTheme.DistanceBetweenBorders;
                spriteBatch.Draw(border,
                    position: new Vector2(X + terrainTheme.BorderOffsetTerrain.X,
                    heightMap[X] + terrainTheme.BorderOffsetTerrain.Y),
                    rotation: ang, origin: new Vector2(border.Width / 2f, 0));
                X -= diff + terrainTheme.DistanceBetweenBorders + terrainTheme.OffsetBetweenBorders;
            }
            // Draw the leftmost border
            if (heightMap[0] > startY)
            {
                var ang2 = (float)Math.Atan2(
                        heightMap[terrainTheme.DistanceBetweenBorderReads] - heightMap[0],
                        terrainTheme.DistanceBetweenBorderReads);
                spriteBatch.Draw(border,
                    position: new Vector2(terrainTheme.BorderOffsetTerrain.X,
                    heightMap[0] + terrainTheme.BorderOffsetTerrain.Y),
                    rotation: ang2, origin: new Vector2(border.Width / 2f, 0));
            }
            spriteBatch.End();

        }

        private void createCollider(Vector2 location)
        {
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
            colorData = null;
        }

    }
}
