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
        private float acceleration = 2000f;
        private Texture2D modifier;
        private Effect modifierEffect;

        public Player()
        {
            OnUpdate += Player_OnUpdate;
            OnInitialize += Player_OnInitialize;
            OnLoadContent += Player_OnLoadContent;
            OnDraw += Player_OnDraw;
            Position = new Vector2(300);
            Size = new Point(25, 75);
        }


        private void Player_OnInitialize()
        {
            MeasureDistanceToTerrain = true;
        }

        private void Player_OnLoadContent(ContentManager content)
        {
            modifier = content.Load<Texture2D>("grassCenter_rounded");
            modifyTerrain(modifier, new Vector2(600,1600), Terrain.InverseOpaque, null, TerrainType.Empty);
        }

        private bool Player_OnUpdate(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var y = ((float)DistanceToTerrain) / 50f;
            var x = (int)MathHelper.LerpPrecise(-gravityFactor, gravityFactor, y) * 2f;
            Velocity = new Vector2(Velocity.X, Velocity.Y + x * deltaTime - gravityFactor * deltaTime);

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
                modifyTerrain(modifier, center, BlendState.AlphaBlend, null, TerrainType.Texture);
            }

            if (keyState.IsKeyDown(Keys.X))
            {
                // Get the center of the modifier
                var center = new Vector2(Position.X - (Size.X >> 1),
                      Position.Y + (modifier.Height));
                modifyTerrain(modifier, center, Terrain.InverseOpaque, null, TerrainType.Empty);
            }

            var mouseState = Mouse.GetState();
            var mousePos = Camera.ScreenToWorldPosition(new Vector2(mouseState.X, mouseState.Y));
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (modifier.Width >> 1),
                    mousePos.Y - (modifier.Height >> 1));
                modifyTerrain(modifier, center, Terrain.InverseOpaque, null, TerrainType.Empty);
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                // Get the center of the modifier
                var center = new Vector2(mousePos.X - (modifier.Width >> 1),
                    mousePos.Y - (modifier.Height >> 1));
                modifyTerrain(modifier, center, BlendState.AlphaBlend, null, TerrainType.Texture);
            }

            return true;
        }

        private bool Player_OnDraw(SpriteBatch arg)
        {
            C3.XNA.Primitives2D.FillRectangle(arg, Camera.WorldToScreenPosition(Position), new Vector2(Size.X, Size.Y), Color.BlueViolet);
            return true;
        }

    }
}
