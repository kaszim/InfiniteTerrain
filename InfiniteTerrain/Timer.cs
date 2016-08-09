using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace InfiniteTerrain
{
    class EventArgs
    {
        public bool Repeat; // default false
        public GameTime GameTime;
    }

    /// <summary>
    /// Times an action and invokes it after a specified amount of time.
    /// If the pass EventArgs classe's property Repeat is set to true, the event will
    /// be repeated once more.
    /// Uses GameTime.
    /// </summary>
    class Timer
    {
        private static HashSet<Timer> timers = new HashSet<Timer>();
        private static HashSet<Timer> timerDeathBucket = new HashSet<Timer>();

        private float sumDelta;
        private float delay;
        private event Action<EventArgs> action;
        
        /// <summary>
        /// Updates all timers.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            // First update all timers
            foreach (var timer in timers)
                timer.update(gameTime);

            // Then destroy the timers in the bucket.
            foreach (var timer in timerDeathBucket)
                timers.Remove(timer);

            // Clear the temporary bucket
            timerDeathBucket.Clear();
        }

        /// <summary>
        /// Constructs a timer instance and initializes it.
        /// </summary>
        /// <param name="delay">Seconds</param>
        /// <param name="action"></param>
        public Timer(float delay, Action<EventArgs> action)
        {
            this.delay = delay;
            this.action = action;
            timers.Add(this);
        }

        /// <summary>
        /// Checks if this timer should be invoked. If the set delay has been reached
        /// action property is invoked.
        /// </summary>
        /// <param name="gameTime"></param>
        private void update(GameTime gameTime)
        {
            sumDelta += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (sumDelta >= delay)
            {
                var args = new EventArgs
                {
                    GameTime = gameTime
                };
                action?.Invoke(args);

                if (args.Repeat)
                    sumDelta -= delay;
                else
                    destroy();
            }
        }

        /// <summary>
        /// Destroys this timer.
        /// </summary>
        private void destroy()
        {
            timerDeathBucket.Add(this);
        }
    }
}
