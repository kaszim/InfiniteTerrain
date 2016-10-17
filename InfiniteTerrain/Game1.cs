using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using InfiniteTerrain.GameObjects;

namespace InfiniteTerrain
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly FrameCounter fc;
        private World world;

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
            var wSize = new Point(10000, 5000);
            var c = new Camera(new Point(graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight), wSize);
            Camera.Initialize(c);
            world = new World(GraphicsDevice, wSize);
            var plr = new Player();
            world.Add(plr);
            c.Follow = plr;
            world.Add(c);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            world.LoadContent(Content);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Update Timers
            Timer.Update(gameTime);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Q))
                world.Debug = !world.Debug;

            world.Update(gameTime);
            fc.Update(deltaTime);
            //Window.Title = $"FPS {fc.AverageFramesPerSecond}";
            Window.Title = $" {Camera.ActiveCamera.Position}";
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
            world.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
