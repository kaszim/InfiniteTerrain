﻿using System;
using System.Diagnostics;
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
        // The bounding rectangle of this node.
        private Rectangle rectangle;
        // The children of this node (if it is not a leaf).
        private QuadTree[] children;
        // The type of this node.
        private QuadTreeType type;
        // The area of this node.
        private int area;
        // Wether or not this node is a leaf.
        private bool isLeaf
        {
            get
            {
                return children == null;
            }
        }

        /// <summary>
        /// Constructs a quadtree.
        /// </summary>
        /// <param name="boundingRectangle">The rectangle which bounds the whole quadtree.</param>
        /// <param name="type">The inital type.</param>
        public QuadTree(Rectangle boundingRectangle, QuadTreeType type)
        {
            rectangle = boundingRectangle;
            this.type = type;
            area = rectangle.Width * rectangle.Height;
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~QuadTree()
        {
        }

        /// <summary>
        /// Splits the current node into four child nodes.
        /// </summary>
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

        /// <summary>
        /// Tries to clean the current node and all its child nodes.
        /// </summary>
        private void clean()
        {
            if(!isLeaf && children[0].isLeaf)
            {
                var firstType = children[0].type;
                for(int i = 1; i < 4; i++)
                {
                    if (!children[i].isLeaf)
                        return;
                    if(firstType != children[i].type)
                        return;
                }
                type = children[0].type;
                children = null;
            }
        }

        /// <summary>
        /// Inserts a rectangle into the quadtree.
        /// </summary>
        /// <param name="modifierRectangle">The rectangle to insert.</param>
        /// <param name="newType">The type of this rectangle.</param>
        public void Insert(Rectangle modifierRectangle, QuadTreeType newType)
        {
            if (modifierRectangle.Contains(rectangle))
            {
                type = newType;
                children = null; // No children are needed if it is fully contained.
            }
            else if(modifierRectangle.Intersects(rectangle))
            {
                // Dont go smaller than a certain value.
                if (area <= 5)
                    return;
                // If this is a leaf, split it up to contain the whole modifier rectangle.
                if(isLeaf)
                    split();
                // Modify all child nodes instead
                foreach (var child in children)
                    child.Insert(modifierRectangle, newType);
                // Try to merge quadrants
                clean();
            }
        }

        /// <summary>
        /// Draws DEBUG rectangles of the quadtree quadtrants.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to draw to.</param>
        [Conditional("DEBUG")]
        public void Draw(SpriteBatch spriteBatch)
        {
            C3.XNA.Primitives2D.DrawRectangle(spriteBatch, rectangle, Color.Black);
            if(!isLeaf)
                foreach (var child in children)
                    child.Draw(spriteBatch);
        }
    }
}
