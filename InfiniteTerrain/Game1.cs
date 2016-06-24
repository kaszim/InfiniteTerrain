using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace InfiniteTerrain
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Terrain terrain;
        GameObject o;
        FrameCounter fc;

        public static GameWindow gWindow;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1680;
            graphics.PreferredBackBufferHeight = 900;
            gWindow = Window;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            fc = new FrameCounter();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Camera.Initialize(new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            o = new GameObject(new Vector2(500, 500), new Point(25, 75));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            terrain = new Terrain(GraphicsDevice, 10000,5000);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.D))
                Camera.Position = new Vector2(Camera.Position.X + 300*deltaTime, Camera.Position.Y);
            if (keyState.IsKeyDown(Keys.A))
                Camera.Position = new Vector2(Camera.Position.X - 300*deltaTime, Camera.Position.Y);
            if (keyState.IsKeyDown(Keys.S))
                Camera.Position = new Vector2(Camera.Position.X, Camera.Position.Y + 300*deltaTime);
            if (keyState.IsKeyDown(Keys.W))
                Camera.Position = new Vector2(Camera.Position.X, Camera.Position.Y - 300 * deltaTime);


            terrain.Update(gameTime);
            o.Update(gameTime);
            var recs = terrain.GetCollidingRectangles(o.Rectangle, QuadTreeType.Texture);
            foreach (Rectangle other in recs)
            {
                var dCenter = o.Rectangle.Center - other.Center;
                float dx;
                float dy;
                // Check which side we intersect from and calculate the dx accordingly
                if (dCenter.X > 0)
                    dx = other.X + other.Size.X - o.Position.X;
                else
                    dx = other.X - o.Position.X - o.Rectangle.Width;
                // same thing here but with dy
                if (dCenter.Y > 0)
                    dy = other.Y + other.Size.Y - o.Position.Y;
                else
                    dy = other.Y - o.Position.Y - o.Rectangle.Height;
                // Whichever of the d's are smallest should be solved first, leave the other for next update
                Vector2 dPos = Math.Abs(dx) > Math.Abs(dy) ? new Vector2(0, dy) : new Vector2(dx, 0);
                o.Position += dPos;
            }
            fc.Update(deltaTime);
            Window.Title = "FPS " + fc.AverageFramesPerSecond;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //Window.Title = gameTime.IsRunningSlowly.ToString();

            terrain.Draw(gameTime);
            spriteBatch.Begin();
            o.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
