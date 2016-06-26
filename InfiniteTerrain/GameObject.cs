using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain
{
    /// <summary>
    /// A game object.
    /// This class holds information about objects that are in the world.
    /// They collide with the terrain.
    /// </summary>
    class GameObject
    {
        // Acceleration of this game object
        private float acceleration = 2000f;
        // The gravity factor (how much pull there is on the object)
        private float gravityFactor = 1000f;
        // The dampening factor, e.g how much friction the air is making
        private float dampeningFactor = 8f;
        // Position of the object
        private Vector2 position;
        // The object's velocity
        private Vector2 velocity;
        // Size of the object
        private Point size;

        /// <summary>
        /// The GameObject's position in the world.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        /// <summary>
        /// The GameObject's velocity. In pixels/second
        /// </summary>
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        /// <summary>
        /// The GameObject's bounding rectangle.
        /// </summary>
        public Rectangle Rectangle
        {
            get { return new Rectangle((int)position.X, (int)position.Y, size.X, size.Y); }
        }

        public GameObject(Vector2 position, Point size)
        {
            this.position = position;
            this.size = size;
        }

        /// <summary>
        /// Updates the GameObject. Applies physics and such.
        /// </summary>
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

        /// <summary>
        /// Updates physics.
        /// </summary>
        protected void updatePhysics(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            velocity -= velocity * dampeningFactor * deltaTime;
            // Gravity disabled for now.
            //velocity.X -= velocity.X * dampeningFactor * deltaTime;
            //velocity.Y += gravityFactor * deltaTime;
            position += velocity * deltaTime;
        }

        /// <summary>
        /// Draws the GameObject to the screen.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to draw to.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenRectangle(Rectangle),
                Color.BlueViolet);
        }
    }
}
