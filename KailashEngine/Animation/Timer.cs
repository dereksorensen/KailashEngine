using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KailashEngine.Animation
{
    class Timer
    {
        private Stopwatch _stopwatch;

        private bool _paused;

        public float seconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds / 1000.0f;
            }
        }

        public float milliseconds
        {
            get
            {
                return _stopwatch.ElapsedMilliseconds;
            }
        }

        public Timer()
        {
            _stopwatch = new Stopwatch();
            _paused = true;
        }


        public void start()
        {
            _paused = false;
            _stopwatch.Start();
        }

        public void stop()
        {
            _paused = true;
            _stopwatch.Stop();
        }

        public void restart()
        {
            _stopwatch.Restart();
        }

        public void pause()
        {
            if(_paused)
            {
                start();
            }
            else
            {
                stop();
            }
        }
        
    }
}
