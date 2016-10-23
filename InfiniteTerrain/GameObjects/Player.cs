using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace InfiniteTerrain.GameObjects
{
    class Player : GameObject
    {
        // Acceleration of this player
        private float acceleration = 200f;
        private Texture2D modifier;
        private Texture2D playerSprite;

        public Player()
        {
            OnPostUpdate += Player_OnUpdate;
            OnLoadContent += Player_OnLoadContent;
            OnDraw += Player_OnDraw;
            Position = new Vector2(300);
            DistanceFromTerrain = 20f;
        }

        private void Player_OnLoadContent(ContentManager content)
        {
            modifier = content.Load<Texture2D>("grassCenter_rounded");
            playerSprite = content.Load<Texture2D>("p1_stand");
            Size = new Point(playerSprite.Width, playerSprite.Height - 20);
        }

        private void Player_OnUpdate(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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

            if (keyState.IsKeyDown(Keys.Z))
            {
                // Get the center of the modifier
                var center = new Vector2(Position.X - (Size.X >> 1),
                      Position.Y + (modifier.Height));
                fillTerrain(modifier, center, TerrainType.Texture);
            }

            if (keyState.IsKeyDown(Keys.X))
            {
                // Get the center of the modifier
                var center = new Vector2(Position.X - (Size.X >> 1),
                      Position.Y + (modifier.Height));
                removeTerrain(modifier, center);
            }

            var mouseState = Mouse.GetState();
            var mousePos = Camera.ScreenToWorldPosition(new Vector2(mouseState.X, mouseState.Y));
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (modifier.Width >> 1),
                    mousePos.Y - (modifier.Height >> 1));
                removeTerrain(modifier, center);
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (modifier.Width >> 1),
                    mousePos.Y - (modifier.Height >> 1));
                fillTerrain(modifier, center, TerrainType.Texture);

            }

            //return true;
        }

        private bool Player_OnDraw(SpriteBatch arg)
        {
            arg.Draw(playerSprite, Camera.WorldToScreenPosition(Position), Color.White);
            //C3.XNA.Primitives2D.FillRectangle(arg, Camera.WorldToScreenPosition(Position), new Vector2(Size.X, Size.Y), Color.BlueViolet);
            return true;
        }

    }
}
