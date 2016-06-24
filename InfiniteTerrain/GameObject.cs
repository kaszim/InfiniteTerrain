using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    class GameObject
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

        public GameObject(Vector2 position, Point size)
        {
            this.position = position;
            this.size = size;
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Right))
                position = new Vector2(position.X + 300 * deltaTime, position.Y);
            if (keyState.IsKeyDown(Keys.Left))
                position = new Vector2(position.X - 300 * deltaTime, position.Y);
            if (keyState.IsKeyDown(Keys.Down))
                position = new Vector2(position.X, position.Y + 300 * deltaTime);
            if (keyState.IsKeyDown(Keys.Up))
                position = new Vector2(position.X, position.Y - 300 * deltaTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenRectangle(Rectangle), Color.BlueViolet);
        }
    }
}
