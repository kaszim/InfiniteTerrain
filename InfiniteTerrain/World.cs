﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        private readonly HashSet<IGameObject> gameObjects;

        /// <summary>
        /// Creates and initializes a World.
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice from Game class.</param>
        public World(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            terrain = new Terrain(graphicsDevice, 10000, 5000);
            gameObjects = new HashSet<IGameObject>();
        }

        /// <summary>
        /// Loads World content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphicsDevice);
            for(int i = 0; i < 500; i++)
                Add(new Player());
        }

        /// <summary>
        /// Updates the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            terrain.Update(gameTime);
            foreach (var gObject in gameObjects)
            {
                gObject.Update(gameTime);
            }
        }

        /// <summary>
        /// Draws the world and all objects in the world.
        /// </summary>
        /// <param name="gameTime">A snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            terrain.Draw(gameTime);
            spriteBatch.Begin();
            foreach (var gObject in gameObjects)
            {
                gObject.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public void Add(GameObject o)
        {
            var interfaceO = (IGameObject)o;
            interfaceO.Initialize(terrain, this);
            gameObjects.Add(interfaceO);
        }
    }
}