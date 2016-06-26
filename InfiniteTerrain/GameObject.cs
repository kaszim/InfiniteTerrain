using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    class GameObject
    {
        private float acceleration = 2000f;
        private float gravityFactor = 1000f;
        private float dampeningFactor = 8f;
        private Vector2 position;
        private Vector2 velocity;
        private Point size;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
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
            updatePhysics(gameTime);
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Right))
                velocity.X += acceleration * deltaTime;
            if (keyState.IsKeyDown(Keys.Left))
                velocity.X -= acceleration * deltaTime;
            if (keyState.IsKeyDown(Keys.Down))
                velocity.Y += acceleration * deltaTime;
            if (keyState.IsKeyDown(Keys.Up))
                velocity.Y -= acceleration * deltaTime;
        }

        protected void updatePhysics(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            velocity -= velocity * dampeningFactor * deltaTime;
            // Gravity disabled for now.
            //velocity.X -= velocity.X * dampeningFactor * deltaTime;
            //velocity.Y += gravityFactor * deltaTime;
            position += velocity * deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenRectangle(Rectangle), Color.BlueViolet);
        }
    }
}
