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
        protected float gravityFactor = 200f;
        // The dampening factor, e.g how much friction the air is making
        private float dampeningFactor = 0.5f;
        // The World's terrain object
        private Terrain terrain;
        // The world
        private World world;
        /// <summary>
        /// The distance to the terrain from the gameobject's feet. If the distance is 50, it means
        /// it is actually 50 or more. 
        /// </summary>
        private int distanceToTerrain;
        // The distance from the feet to the terrain we want
        private float distanceFromTerrain;

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
        /// This also stops other events within the OnPreUpdate from invoking.
        /// </summary>
        public event Func<GameTime, bool> OnPreUpdate;
        /// <summary>
        /// Called after any other update logic.
        /// Can be stopped if OnPreUpdate has returned false.
        /// </summary>
        public event Action<GameTime> OnPostUpdate;
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
        /// The distance from the gameobject's feet to the terrain we want.
        /// </summary>
        public float DistanceFromTerrain
        {
            get
            {
                return distanceFromTerrain;
            }

            set
            {
                if (value > 50)
                    distanceFromTerrain = 50;
                else
                    distanceFromTerrain = value;
            }
        }

        Terrain IGameObject.Terrain => terrain;
        World IGameObject.World => world;


        void IGameObject.Initialize(Terrain terrain, World world)
        {
            this.terrain = terrain;
            this.world = world;
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
            if (OnPreUpdate != null)
                foreach (Func<GameTime, bool> delg in OnPreUpdate.GetInvocationList())
                    if (!delg.Invoke(gameTime))
                        return;
            updatePhysics(gameTime);
            OnPostUpdate?.Invoke(gameTime);
        }

        /// <summary>
        /// Updates physics.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void updatePhysics(GameTime gameTime)
        {
            // Collision
            // TODO: Change search quadrants
            var recs = terrain.GetCollidingRectangles(Rectangle, TerrainType.Texture, new Point(1));
            foreach (Rectangle other in recs)
            {
                solveCollsion(other);
            }

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            Velocity = new Vector2(
                Velocity.X,
                Velocity.Y + gravityFactor * deltaTime);

            Velocity *= (float)Math.Pow(dampeningFactor, deltaTime);

            if (distanceToTerrain < distanceFromTerrain)
            {
                //var y = ((float)DistanceToTerrain) / 25f;
                //var x = (int)MathHelper.LerpPrecise(-2f * gravityFactor, 0f, y);
                //var F = (DistanceToTerrain - 20) * 100f;

                //Velocity = new Vector2(Velocity.X, Velocity.Y + F * deltaTime);
                Position = new Vector2(Position.X, Position.Y + (distanceToTerrain - distanceFromTerrain) * deltaTime * 100);
            }

            Position += Velocity * deltaTime;
            // Distance to terrain from feet
            // TODO: Better distance measuring
            int i;
            for (i = 0; i < 50; i++)
            {
                recs = terrain.GetCollidingRectangles(
                    new Rectangle(Rectangle.X, Rectangle.Y + Rectangle.Height, Rectangle.Width, i + 1),
                    TerrainType.Texture, new Point(1));
                if (recs.Count > 0)
                    break;
            }
            distanceToTerrain = i;
            
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
            //dx = (float)Math.Round(dx);
            // same thing here but with dy
            if (dCenter.Y > 0)
                dy = other.Y + other.Size.Y - Position.Y;
            else
                dy = other.Y - Position.Y - Rectangle.Height;
            //dy = (float)Math.Round(dy);
            // Whichever of the d's are smallest should be solved first, leave the other for
            // next update
            var absdx = Math.Abs(dx);
            var absdy = Math.Abs(dy);
            var dPos = absdx > absdy ? new Vector2(0, dy) : new Vector2(dx, 0);
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
        }

        /// <summary>
        /// Removes terrain with a specified texture. Filled colors equals removed pixels.
        /// </summary>
        /// <param name="texture">The texture to use.</param>
        /// <param name="pos">Where the removal will be applied.</param>
        protected void removeTerrain(Texture2D texture, Vector2 pos)
        {
            // five times to achieve sought effect
            for (int i = 0; i < 6; i++)
            {
                modifyTerrain(texture, pos, Terrain.InverseOpaque, null, TerrainType.Empty);
            }
        }

        /// <summary>
        /// Fills the terrain with the specified texture.
        /// </summary>
        /// <param name="texture">The texture to apply.</param>
        /// <param name="pos">Where the texture will be applied.</param>
        /// <param name="type">What type the area will have.</param>
        protected void fillTerrain(Texture2D texture, Vector2 pos, TerrainType type)
        {
            modifyTerrain(texture, pos, BlendState.AlphaBlend, null, type);

        }

        /// <summary>
        /// Modifies the terrain if this gameobject is capable of it.
        /// </summary>
        /// <param name="texture">The texture to apply to the terrain.</param>
        /// <param name="pos">The position to apply it to.</param>
        /// <param name="blendstate"></param>
        /// <param name="effect"></param>
        /// <param name="type">The type of the terrain.</param>
        private void modifyTerrain(Texture2D texture, Vector2 pos, BlendState blendstate,
            Effect effect, TerrainType type)
        {
            terrain.ApplyTexture(texture, pos, blendstate, effect, type);
        }
    }
}
