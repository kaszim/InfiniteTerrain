using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace InfiniteTerrain
{
    /// <summary>
    /// Handles the Camera.
    /// </summary>
    static class Camera
    {
        private const float acceleration = 500f;
        private const float minDampening = 0.1f;
        private const float maxDampening = 0.3f;
        private static Vector2 position;
        private static Point size;
        private static Point worldSize;

        public static Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public static Point Size => size;

        /// <summary>
        /// Initializes the world camera.
        /// </summary>
        /// <param name="cameraSize">The size of the camera.</param>
        public static void Initialize(Point cameraSize)
        {
            position = Vector2.Zero;
            size = cameraSize;
        }

        /// <summary>
        /// Translates a screen position into a world position, relative to the camera.
        /// </summary>
        /// <param name="transPosition">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 ScreenToWorldPosition(Vector2 transPosition) => transPosition + position;

        /// <summary>
        /// Translates a world position into a screen position, relative to the camera.
        /// Note that the returned Vector2's components are also rounded.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="transPosition">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 WorldToScreenPosition(Vector2 transPosition) => Round(transPosition - position);

        /// <summary>
        /// Translates a world rectangle into a screen rectangle, relative to the camera.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="rectangle">The rectangle to translate.</param>
        /// <returns>The translated rectangle.</returns>
        public static Rectangle WorldToScreenRectangle(Rectangle rectangle) =>
            new Rectangle(rectangle.X - (int)Math.Round(position.X),
                rectangle.Y - (int)Math.Round(position.Y),
                rectangle.Width, rectangle.Height);

        /// <summary>
        /// Rounds a vectors components into integers.
        /// </summary>
        /// <param name="vec">The vector to round.</param>
        /// <returns>A rounded vector with integer components.</returns>
        private static Vector2 Round(Vector2 vec) => new Vector2((float)Math.Round(vec.X), (float)Math.Round(vec.Y));
    }
}
