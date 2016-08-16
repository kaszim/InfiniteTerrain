using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain.GameObjects
{
    class Player : GameObject
    {
        // Acceleration of this player
        private float acceleration = 2000f;

        public Player()
        {
            OnUpdate += Player_OnUpdate;
            Position = new Vector2(300);
            Size = new Point(25, 75);
        }

        private bool Player_OnUpdate(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var y = ((float)DistanceToTerrain) / 50f;
            var x = (int)MathHelper.LerpPrecise(-gravityFactor, gravityFactor, y);
            Velocity = new Vector2(Velocity.X, Velocity.Y + x * deltaTime);
            Game1.gWindow.Title = Velocity.ToString();

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Right))
            {
                Velocity = new Vector2(Velocity.X + acceleration * deltaTime, Velocity.Y);
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                Velocity = new Vector2(Velocity.X - acceleration * deltaTime, Velocity.Y);
            }
            if (keyState.IsKeyDown(Keys.Down))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y + acceleration * deltaTime);
            }
            if (keyState.IsKeyDown(Keys.Up))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y - acceleration * deltaTime);
            }

            return true;
        }
    }
}
