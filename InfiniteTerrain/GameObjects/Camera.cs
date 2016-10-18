using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfiniteTerrain.GameObjects;
using Microsoft.Xna.Framework;

namespace InfiniteTerrain
{
    /// <summary>
    /// Handles the Cameras.
    /// </summary>
    class Camera : GameObject
    {
        private static Camera activeCamera;
        private const float acceleration = 100f;
        private const float minDampening = 0.2f;
        private const float maxDampening = 0.5f;
        private Point worldSize;

        /// <summary>
        /// The current active camera.
        /// </summary>
        public static Camera ActiveCamera
        {
            get { return activeCamera; }
            set { if (value != null) activeCamera = value; }
        }
        /// <summary>
        /// The game object to follow.
        /// </summary>
        public GameObject Follow { get; set; }
        /// <summary>
        /// Wether or not the follow logic is active
        /// </summary>
        public bool FollowActive
        {
            get { return worldSize != Point.Zero; }
        }

        /// <summary>
        /// Initializes the world camera.
        /// </summary>
        /// <param name="cameraSize">The size of the camera.</param>
        /// <param name="wSize">The size of the world, to collide with the corners. Set to Point.Zero for no collision. </param>
        public Camera(Point cameraSize, Point wSize)
        {
            Position = Vector2.Zero;
            Size = cameraSize;
            worldSize = wSize;
            OnUpdate += Camera_OnUpdate;
        }

        private bool Camera_OnUpdate(GameTime arg)
        {
            if (FollowActive)
            {
                var deltaTime = (float)arg.ElapsedGameTime.TotalSeconds;
                var dirVector = Follow.Position -
                    // Fake that our camera's position is centered
                    new Vector2(Position.X + (Size.X >> 1), Position.Y + (Size.Y >> 1));

                // Push towards the player
                Position += dirVector * acceleration *
                    // Dampen according to our distance, far distance, less dampening
                    MathHelper.Lerp(minDampening, maxDampening, 1 - 1f / dirVector.LengthSquared())
                    * deltaTime;

                // Resolve collisions with map corners
                if (Position.X < 0)
                    Position = new Vector2(0, Position.Y);
                else if (Position.X > worldSize.X - Size.X)
                    Position = new Vector2(worldSize.X - Size.X, Position.Y);
                if (Position.Y < 0)
                    Position = new Vector2(Position.X, 0);
                else if (Position.Y > worldSize.Y - Size.Y)
                    Position = new Vector2(Position.X, worldSize.Y - Size.Y);
            }

            return false; // To stop other update events to occur too
        }

        public static void Initialize(Camera camera)
        {
            ActiveCamera = camera;
        }

        /// <summary>
        /// Translates a screen position into a world position, relative to the camera.
        /// </summary>
        /// <param name="transPosition">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 ScreenToWorldPosition(Vector2 transPosition) => transPosition + ActiveCamera.Position;

        /// <summary>
        /// Translates a world position into a screen position, relative to the camera.
        /// Note that the returned Vector2's components are also rounded.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="transPosition">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 WorldToScreenPosition(Vector2 transPosition) => Round(transPosition - ActiveCamera.Position);

        /// <summary>
        /// Translates a world rectangle into a screen rectangle, relative to the camera.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="rectangle">The rectangle to translate.</param>
        /// <returns>The translated rectangle.</returns>
        public static Rectangle WorldToScreenRectangle(Rectangle rectangle) =>
            new Rectangle(rectangle.X - (int)Math.Round(ActiveCamera.Position.X),
                rectangle.Y - (int)Math.Round(ActiveCamera.Position.Y),
                rectangle.Width, rectangle.Height);

        /// <summary>
        /// Rounds a vectors components into integers.
        /// </summary>
        /// <param name="vec">The vector to round.</param>
        /// <returns>A rounded vector with integer components.</returns>
        private static Vector2 Round(Vector2 vec) => new Vector2((float)Math.Round(vec.X), (float)Math.Round(vec.Y));
    }
}
