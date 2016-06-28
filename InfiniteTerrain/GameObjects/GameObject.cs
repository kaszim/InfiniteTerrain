using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain.GameObjects
{
    /// <summary>
    /// Classes which implement this are objects in the world.
    /// </summary>
    interface IGameObject
    {
        /// <summary>
        /// The world's Terrain.
        /// </summary>
        Terrain Terrain { get; }
        /// <summary>
        /// The world.
        /// </summary>
        World World { get; }
        /// <summary>
        /// The GameObject's position in the world.
        /// </summary>
        Vector2 Position { get; set; }
        /// <summary>
        /// The GameObject's velocity. In pixels/second
        /// </summary>
        Vector2 Velocity { get; set; }
        /// <summary>
        /// The GameObject's bounding rectangle.
        /// </summary>
        Rectangle Rectangle { get; }
        /// <summary>
        /// Initialized the object into the world.
        /// Consequently, the terrain field and world field is set.
        /// </summary>
        /// <param name="terrain">The World's terrain instance.</param>
        /// <param name="world">The world.</param>
        void Initialize(Terrain terrain, World world);
        /// <summary>
        /// Updates the GameObject. Applies physics and such.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void Update(GameTime gameTime);
        /// <summary>
        /// Draws the GameObject to the screen.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to draw to.</param>
        void Draw(SpriteBatch spriteBatch);
    }

    /// <summary>
    /// A game object.
    /// This class holds information about objects that are in the world.
    /// They collide with the terrain.
    /// </summary>
    abstract class GameObject : IGameObject
    {
        // The gravity factor (how much pull there is on the object)
        private float gravityFactor = 1000f;
        // The dampening factor, e.g how much friction the air is making
        private float dampeningFactor = 8f;
        // The World's terrain object
        private Terrain terrain;
        // The world
        private World world;

        /// <summary>
        /// Called before the Initialization of a game object.
        /// </summary>
        public event Action OnInitialize;
        /// <summary>
        /// Called before any other update logic.
        /// Return false to stop the rest of the update logic of running.
        /// </summary>
        public event Func<GameTime, bool> OnUpdate;
        /// <summary>
        /// Called before any other update logic.
        /// Return false to stop the rest of the update logic of running.
        /// </summary>
        public event Func<SpriteBatch, bool> OnDraw;

        /// <summary>
        /// The GameObject's size.
        /// </summary>
        public Point Size { get; set; }
        /// <summary>
        /// The GameObject's position in the world.
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// The GameObject's velocity. In pixels/second
        /// </summary>
        public Vector2 Velocity { get; set; }
        /// <summary>
        /// The GameObject's bounding rectangle.
        /// </summary>
        public Rectangle Rectangle => new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);

        Terrain IGameObject.Terrain => terrain;
        World IGameObject.World => world;

        void IGameObject.Initialize(Terrain terrain, World world)
        {
            OnInitialize?.Invoke();
            this.terrain = terrain;
            this.world = world;
        }

        /// <summary>
        /// Updates the GameObject. Applies physics and such.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void IGameObject.Update(GameTime gameTime)
        {
            var res = OnUpdate?.Invoke(gameTime);
            if (res == null || res == true)
            {
                updatePhysics(gameTime);
            }
        }

        /// <summary>
        /// Updates physics.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void updatePhysics(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity -= Velocity * dampeningFactor * deltaTime;
            // Gravity disabled for now.
            //velocity.X -= velocity.X * dampeningFactor * deltaTime;
            //velocity.Y += gravityFactor * deltaTime;

            //Collision
            Position += Velocity * deltaTime;
            var recs = terrain.GetCollidingRectangles(Rectangle, QuadTreeType.Texture);
            foreach (Rectangle other in recs)
            {
                var dCenter = Rectangle.Center - other.Center;
                float dx;
                float dy;
                // Check which side we intersect from and calculate the dx accordingly
                if (dCenter.X > 0)
                    dx = other.X + other.Size.X - Position.X;
                else
                    dx = other.X - Position.X - Rectangle.Width;
                // same thing here but with dy
                if (dCenter.Y > 0)
                    dy = other.Y + other.Size.Y - Position.Y;
                else
                    dy = other.Y - Position.Y - Rectangle.Height;
                // Whichever of the d's are smallest should be solved first, leave the other for
                // next update
                var dPos = Math.Abs(dx) > Math.Abs(dy) ? new Vector2(0, dy) : new Vector2(dx, 0);
                Position += dPos;
            }
        }

        /// <summary>
        /// Draws the GameObject to the screen.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to draw to.</param>
        void IGameObject.Draw(SpriteBatch spriteBatch)
        {
            var res = OnDraw?.Invoke(spriteBatch);
            if (res == null || res == true)
            {
                C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenRectangle(Rectangle),
                Color.BlueViolet);
            }
        }
    }
}
