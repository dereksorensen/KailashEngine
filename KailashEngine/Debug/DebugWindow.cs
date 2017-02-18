using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntTweakBar;

using OpenTK;
using OpenTK.Input;

namespace KailashEngine.Debug
{
    class DebugWindow
    {

        private Context _context_debug;
        public Context context_debug
        {
            get
            {
                return _context_debug;
            }
        }


        private Bar _bar_fps;
        private FloatVariable _fps;

        private Bar _bar_timers;
        private FloatVariable _timer_1;
        private FloatVariable _timer_2;
        private FloatVariable _timer_3;


        public DebugWindow()
        {
            _context_debug = new Context(Tw.GraphicsAPI.OpenGLCore);
            _context_debug.SetFontSize(BarFontSize.Small);
            _context_debug.SetFontResizable(true);
        }


        public void load()
        {
            createWindow_FPS(_context_debug);
            createWindow_Timers(_context_debug);
        }


        public void unload()
        {
            if (_context_debug != null)
            {
                _context_debug.Dispose();
            }
        }


        //------------------------------------------------------
        // Input Handling
        //------------------------------------------------------

        public bool handle_MouseMove(Point position)
        {
            return _context_debug.HandleMouseMove(position);
        }

        public bool handle_MouseWheel(MouseWheelEventArgs e)
        {
            return _context_debug.HandleMouseWheel(e.Value);
        }

        public bool handle_MouseClick(MouseButtonEventArgs e)
        {
            IDictionary<MouseButton, Tw.MouseButton> Mapping = new Dictionary<MouseButton, Tw.MouseButton>()
            {
                { MouseButton.Left, Tw.MouseButton.Left },
                { MouseButton.Middle, Tw.MouseButton.Middle },
                { MouseButton.Right, Tw.MouseButton.Right },
            };

            var action = e.IsPressed ? Tw.MouseAction.Pressed : Tw.MouseAction.Released;

            if (Mapping.ContainsKey(e.Button))
            {
                return _context_debug.HandleMouseClick(action, Mapping[e.Button]);
            }
            else
            {
                return false;
            }
        }

        public bool handle_Keyboard(KeyPressEventArgs e)
        {
            return _context_debug.HandleKeyPress(e.KeyChar);
        }




        //------------------------------------------------------
        // FPS Window
        //------------------------------------------------------
        private void createWindow_FPS(Context context)
        {
            _bar_fps = new Bar(context);
            _bar_fps.Label = "FPS";
            _bar_fps.Contained = true;
            _bar_fps.Color = Color.Black;
            _bar_fps.Size = new Size(250, 50);
            _bar_fps.ValueColumnWidth = 90;
            _bar_fps.Position = new Point(10, 10);
            _bar_fps.RefreshRate = 1;

            _fps = new FloatVariable(_bar_fps, 0.0f);
            _fps.Label = "FPS";

        }

        //------------------------------------------------------
        // Timers Window
        //------------------------------------------------------
        public void toggleTimers()
        {
            _bar_timers.Iconified = !_bar_timers.Iconified;
        }

        private void createWindow_Timers(Context context)
        {
            _bar_timers = new Bar(context);
            _bar_timers.Label = "Timers (ms)";
            _bar_timers.Contained = true;
            _bar_timers.Color = Color.DarkRed;
            _bar_timers.Size = new Size(250, 100);
            _bar_timers.ValueColumnWidth = 90;
            _bar_timers.Position = new Point(10, 70);
            _bar_timers.RefreshRate = 1;
            _bar_timers.Iconified = true;

            _timer_1 = new FloatVariable(_bar_timers);
            _timer_2 = new FloatVariable(_bar_timers);
            _timer_3 = new FloatVariable(_bar_timers);
        }

        //------------------------------------------------------
        // Rendering
        //------------------------------------------------------
        public void resize(Size size)
        {
            _context_debug.HandleResize(size);
        }


        public void render(float current_fps)
        {
            try
            {
                _fps.Value = current_fps;

                _timer_1.Value = DebugHelper.timer_1.time;
                _timer_1.Label = DebugHelper.timer_1.name ?? "N/A";
                _timer_2.Value = DebugHelper.timer_2.time;
                _timer_2.Label = DebugHelper.timer_2.name ?? "N/A";
                _timer_3.Value = DebugHelper.timer_3.time;
                _timer_3.Label = DebugHelper.timer_3.name ?? "N/A";

                _context_debug.Draw();
            }
            catch (Exception e)
            {
                DebugHelper.logError("AntTweakBar error:", e.Message);
            }
        }


    }
}
