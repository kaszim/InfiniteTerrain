using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
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
        /// Loads the gameobjects content.
        /// </summary>
        /// <param name="Content">The game's content manager.</param>
        void LoadContent(ContentManager Content);
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
        protected float gravityFactor = 1000f;
        // The dampening factor, e.g how much friction the air is making
        private float dampeningFactor = 8f;
        // The World's terrain object
        private Terrain terrain;
        // The world
        private World world;
        // The distance to the terrain.
        private int distanceToTerrain;

        /// <summary>
        /// Called before the Initialization of a game object.
        /// </summary>
        public event Action OnInitialize;
        /// <summary>
        /// Called before the ContentLoad of a game object.
        /// </summary>
        public event Action<ContentManager> OnLoadContent;
        /// <summary>
        /// Called before any other update logic.
        /// Return false to stop the rest of the update logic of running.
        /// This also stops other events within the OnUpdate from invoking.
        /// </summary>
        public event Func<GameTime, bool> OnUpdate;
        /// <summary>
        /// Called before any other update logic.
        /// Return false to stop the rest of the update logic of running.
        /// This also stops other events within the OnDraw from invoking.
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
        /// <summary>
        /// Wether or not to measure the distance to the terrain. Default is false.
        /// </summary>
        public bool MeasureDistanceToTerrain { get; set; }
        /// <summary>
        /// The distance to the terrain from the gameobject's feet. If the distance is 50, it means
        /// it is actually 50 or more. If the measuring is off, the distance will be set to -1.
        /// </summary>
        public int DistanceToTerrain
        {
            get
            {
                if (MeasureDistanceToTerrain)
                    return distanceToTerrain;
                else
                    return -1;
            }

            private set
            {
                distanceToTerrain = value;
            }
        }

        Terrain IGameObject.Terrain => terrain;
        World IGameObject.World => world;

        void IGameObject.Initialize(Terrain terrain, World world)
        {
            this.terrain = terrain;
            this.world = world;
            this.MeasureDistanceToTerrain = false;
            OnInitialize?.Invoke();
        }

        void IGameObject.LoadContent(ContentManager Content)
        {
            OnLoadContent?.Invoke(Content);
        }

        /// <summary>
        /// Updates the GameObject. Applies physics and such.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void IGameObject.Update(GameTime gameTime)
        {
            if (OnUpdate != null)
                foreach (Func<GameTime, bool> delg in OnUpdate.GetInvocationList())
                    if (!delg.Invoke(gameTime))
                        return;
            updatePhysics(gameTime);
        }

        /// <summary>
        /// Updates physics.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void updatePhysics(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity = new Vector2(
                Velocity.X - Velocity.X * dampeningFactor * deltaTime,
                (Velocity.Y - Velocity.Y * dampeningFactor * deltaTime + gravityFactor * deltaTime));

            // Collision
            Position += Velocity * deltaTime;
            // TODO: Change search quadrants
            var recs = terrain.GetCollidingRectangles(Rectangle, QuadTreeType.Texture, new Point(1));
            foreach (Rectangle other in recs)
            {
                solveCollsion(other);
            }

            // Distance to terrain from feet
            // TODO: Better distance measuring
            if (MeasureDistanceToTerrain)
            {
                int i;
                for (i = 1; i < 50; i++)
                {
                    recs = terrain.GetCollidingRectangles(
                        new Rectangle(Rectangle.X + (Rectangle.Width >> 1), Rectangle.Y + Rectangle.Height, 1, i),
                        QuadTreeType.Texture, new Point(1));
                    if (recs.Count > 0)
                        break;
                }
                DistanceToTerrain = i;
            }
        }

        private void solveCollsion(Rectangle other)
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
            var absdx = Math.Abs(dx);
            var absdy = Math.Abs(dy);
            Vector2 dPos;
            if (absdx > absdy)
            {
                dPos = new Vector2(0, dy);
                Velocity = new Vector2(Velocity.X, 0);
            }
            else
            {
                // if the collider is less than an amoutn step onto it
                if (absdy <= 1f)
                {
                    dPos = new Vector2(0, dy);
                }
                else
                {
                    dPos = new Vector2(dx, 0);
                    //Velocity = new Vector2(0, Velocity.Y); cant remember :(
                }
            }
            Position += dPos;
        }

        /// <summary>
        /// Draws the GameObject to the screen.
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to draw to.</param>
        void IGameObject.Draw(SpriteBatch spriteBatch)
        {
            if(OnDraw != null)
                foreach (Func<SpriteBatch, bool> delg in OnDraw.GetInvocationList())
                    if (!delg.Invoke(spriteBatch))
                        return;
            C3.XNA.Primitives2D.FillRectangle(spriteBatch, Camera.WorldToScreenPosition(Position), new Vector2(Size.X, Size.Y), Color.BlueViolet);
            
        }
    }
}
