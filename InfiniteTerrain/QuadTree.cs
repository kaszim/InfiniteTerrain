using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    enum QuadTreeType
    {
        Empty,
        Texture
    }

    class QuadTree
    {
        private GraphicsDevice graphicsDevice;
        private Rectangle rectangle;
        private Texture2D texture;
        private QuadTree[] children;
        private QuadTreeType internalType;
        private QuadTreeType type
        {
            get
            {
                return internalType;
            }
            set
            {
                
                internalType = value;
            }
        }
        private bool isLeaf
        {
            get
            {
                return children == null;
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                return rectangle;
            }
        }

        public QuadTree(GraphicsDevice graphicsDevice, Rectangle boundingRectangle, QuadTreeType type)
        {
            this.graphicsDevice = graphicsDevice;
            rectangle = boundingRectangle;
            texture = new Texture2D(graphicsDevice, rectangle.Width, rectangle.Height);
            this.type = type;
            Color[] colors = new Color[texture.Width * texture.Height];
            Color c = type == QuadTreeType.Empty ? Color.Transparent : Color.Green;
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors[x + y * texture.Height] = c;
                }
            }
            texture.SetData<Color>(colors);
        }

        ~QuadTree()
        {
            texture.Dispose();
        }

        public void Insert(Rectangle modifierRectangle, QuadTreeType newType)
        {
            if (modifierRectangle.Contains(rectangle))
            {
                type = newType;
                children = null; // No children are needed if it is fully contained.
                Color[] colors = new Color[texture.Width * texture.Height];
                Color c = type == QuadTreeType.Empty ? Color.Transparent : Color.Green;
                for (int x = 0; x < texture.Width; x++)
                {
                    for (int y = 0; y < texture.Height; y++)
                    {
                        colors[x + y * texture.Height] = c;
                    }
                }
                texture.SetData<Color>(colors);
                return;
            }
            else if(modifierRectangle.Intersects(rectangle))
            {
                // Dont go smaller than a certain value.
                if (rectangle.Width <= 5)
                    return;
                // If this is a leaf, split it up to contain the whole modifier rectangle.
                if(isLeaf)
                    split();

                foreach (var child in children)
                    child.Insert(modifierRectangle, newType);
            }
        }

        private void split()
        {
            children = new QuadTree[4];
            var halfWidth = (int)Math.Ceiling((double)rectangle.Width / 2);
            var halfHeight = (int)Math.Ceiling((double)rectangle.Height / 2);
            children[0] = new QuadTree(graphicsDevice, new Rectangle(rectangle.X, rectangle.Y, halfWidth, halfHeight), type);
            children[1] = new QuadTree(graphicsDevice, new Rectangle(rectangle.X + halfWidth, rectangle.Y, halfWidth, halfHeight), type);
            children[2] = new QuadTree(graphicsDevice, new Rectangle(rectangle.X, rectangle.Y + halfHeight, halfWidth, halfHeight), type);
            children[3] = new QuadTree(graphicsDevice, new Rectangle(rectangle.X + halfWidth, rectangle.Y + halfHeight, halfWidth, halfHeight), type);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(isLeaf)
                spriteBatch.Draw(texture, rectangle, Color.White);
            else
            {
                foreach (var child in children)
                    child.Draw(spriteBatch);
            }
        }
    }
}
