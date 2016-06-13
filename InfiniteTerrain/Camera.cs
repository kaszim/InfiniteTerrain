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
        /// <param name="position">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 ScreenToWorldPosition(Vector2 transPosition)
        {
            return transPosition + position;
        }

        /// <summary>
        /// Translates a world position into a screen position, relative to the camera.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="position">The position to translate.</param>
        /// <returns>The translated position.</returns>
        public static Vector2 WorldToScreenPosition(Vector2 transPosition)
        {
            return transPosition - position;
        }

        /// <summary>
        /// Translates a world rectangle into a screen rectangle, relative to the camera.
        /// Use this function when drawing to the screen.
        /// </summary>
        /// <param name="rectangle">The rectangle to translate.</param>
        /// <returns>The translated rectangle.</returns>
        public static Rectangle WorldToScreenRectangle(Rectangle rectangle)
        {
            return new Rectangle(rectangle.X - (int)position.X, rectangle.Y - (int)position.Y,
                rectangle.Width, rectangle.Height);
        }
    }
}
