using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    class Object
    {
        private Vector2 position;
        private Point size;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Rectangle Rectangle
        {
            get { return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y); }
        }

        public Object(Vector2 position, Point size)
        {
            this.position = position;
            this.size = size;
        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenRectangle(Rectangle), Color.BlueViolet);
        }
    }
}
