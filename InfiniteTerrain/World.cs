using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using InfiniteTerrain.GameObjects;

namespace InfiniteTerrain
{
    class World
    {
        private readonly GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private readonly Terrain terrain;
        private readonly Terrain backgroundTerrain;
        private readonly HashSet<IGameObject> gameObjects;
        private TerrainGenerator worldGenerator;

        /// <summary>
        /// Wether or not to have debug mode on.
        /// </summary>
        public bool Debug
        {
            get
            {
                return terrain.Debug;
            }
            set
            {
                terrain.Debug = value;
            }
        }

        /// <summary>
        /// Creates and initializes a World.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice from Game class.</param>
        /// <param name="size">Size of the world.</param>
        public World(GraphicsDevice graphicsDevice, Point size)
        {
            this.graphicsDevice = graphicsDevice;
            terrain = new Terrain(graphicsDevice, size.X, size.Y);
            backgroundTerrain = new Terrain(graphicsDevice, size.X, size.Y);
            gameObjects = new HashSet<IGameObject>();
            var terrainTheme = new TerrainTheme
            {
                DistanceBetweenBorderReads = 10,
                DistanceBetweenBorders = 23,
                BorderOffsetTerrain = new Point(0, -5),
                BorderTextureLocation = "h3",
                OffsetBetweenBorders = 1,
                FillTileLocation = "grassCenter"
            };
            worldGenerator = new TerrainGenerator(backgroundTerrain, graphicsDevice, terrainTheme);
        }

        /// <summary>
        /// Loads World content.
        /// </summary>
        /// <param name="content">todo: describe content parameter on LoadContent</param>
        public void LoadContent(ContentManager content)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
            backgroundTerrain.LoadContent(content);
            terrain.LoadContent(content);
            forEachIGameObject((gObject) => gObject.LoadContent(content));
            //set background color to gray
            backgroundTerrain.Color = Color.DarkSlateGray;
            worldGenerator.LoadContent(content);
            // First generate backgroundterrain
            worldGenerator.GenerateArea(
                new Rectangle(0, 0,
                backgroundTerrain.NumberOfChunksHorizontally,
                backgroundTerrain.NumberOfChunksVertically));
            // Then the "real" one
            worldGenerator.Terrain = terrain;
            worldGenerator.GenerateArea(new Rectangle(0, 0,
                terrain.NumberOfChunksHorizontally, terrain.NumberOfChunksVertically));
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            backgroundTerrain.Update(gameTime);
            terrain.Update(gameTime);
            forEachIGameObject((gObject) => gObject.Update(gameTime));
        }

        /// <summary>
        /// Draws the world and all objects in the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            backgroundTerrain.Draw(gameTime);
            terrain.Draw(gameTime);
            spriteBatch.Begin();
            forEachIGameObject((gObject) => gObject.Draw(spriteBatch));
            spriteBatch.End();
        }

        /// <summary>
        /// Adds a gameobject to the world and initializes it.
        /// </summary>
        /// <param name="o">The gameobject to add.</param>
        public void Add(GameObject o)
        {
            var interfaceO = (IGameObject)o;
            interfaceO.Initialize(terrain, this);
            gameObjects.Add(interfaceO);
        }

        private void forEachIGameObject(Action<IGameObject> action)
        {
            foreach(var gObject in gameObjects)
            {
                action?.Invoke(gObject);
            }
        }
    }
}
