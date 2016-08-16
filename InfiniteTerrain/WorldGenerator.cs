using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    class WorldGenerator
    {
        private Terrain terrain;
        private Texture2D pixel;

        public WorldGenerator(Terrain terrain, Texture2D pixel)
        {
            this.terrain = terrain;
            this.pixel = pixel;
        }

        public void GenerateArea(Rectangle rectangle)
        {
            for (var x = rectangle.X; x < rectangle.X + rectangle.Width; x++)
            {
                var height = 50*Math.Sin(x);
                for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height; y++)
                {
                    if (y < height)
                        terrain.ApplyTexture(pixel, new Vector2(x, y), QuadTreeType.Texture);
                }
            }
        }
    }
}
