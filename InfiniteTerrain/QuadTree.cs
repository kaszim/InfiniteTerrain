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
        private Rectangle rectangle;
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

        public QuadTree(Rectangle boundingRectangle, QuadTreeType type)
        {
            rectangle = boundingRectangle;
            this.type = type;
        }

        ~QuadTree()
        {
        }

        public void Insert(Rectangle modifierRectangle, QuadTreeType newType)
        {
            if (modifierRectangle.Contains(rectangle))
            {
                type = newType;
                children = null; // No children are needed if it is fully contained.
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
            children[0] = new QuadTree(new Rectangle(rectangle.X, rectangle.Y, halfWidth, halfHeight), type);
            children[1] = new QuadTree(new Rectangle(rectangle.X + halfWidth, rectangle.Y, halfWidth, halfHeight), type);
            children[2] = new QuadTree(new Rectangle(rectangle.X, rectangle.Y + halfHeight, halfWidth, halfHeight), type);
            children[3] = new QuadTree(new Rectangle(rectangle.X + halfWidth, rectangle.Y + halfHeight, halfWidth, halfHeight), type);
        }
    }
}
