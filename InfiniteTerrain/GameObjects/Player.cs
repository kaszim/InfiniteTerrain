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
        private float floatAmount = 200f;
        private float floatDelay = 0.5f;
        private Timer horizontalJumpTimer;
        private KeyboardState keyState;
        private bool canFloat;

        /// <summary>
        /// Checks if the player is getting horizontal movement input.
        /// </summary>
        /// <returns>Returns true if the player is getting horizontal movement input.</returns>
        public bool HorizontalMovementInput
        {
            get
            {
                return (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.Right));
            }
        }

        public Player()
        {
            OnUpdate += Player_OnUpdate;
            Position = new Vector2(300);
            Size = new Point(25, 75);
            horizontalJumpTimer = new Timer(floatDelay, (EventArgs args) =>
            {
                canFloat = true;
                 args.Repeat = true;
            });
        }

        private bool Player_OnUpdate(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Right))
            {
                Velocity = new Vector2(Velocity.X + acceleration * deltaTime,
                    canFloat ? Velocity.Y - floatAmount: Velocity.Y);
                if (canFloat)
                    canFloat = !canFloat;
            }
            if (keyState.IsKeyDown(Keys.Left))
            {
                Velocity = new Vector2(Velocity.X - acceleration * deltaTime,
                    canFloat ? Velocity.Y - floatAmount : Velocity.Y);
                if (canFloat)
                    canFloat = !canFloat;
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
