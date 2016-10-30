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

        private Bar _bar_logs;
        private StringVariable _logs;


        public DebugWindow()
        {
            _context_debug = new Context(Tw.GraphicsAPI.OpenGLCore);
            _context_debug.SetFontSize(BarFontSize.Small);
            _context_debug.SetFontResizable(true);
        }


        public void load()
        {
            createWindow_FPS(_context_debug);
            createWindow_Logs(_context_debug);
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
            _bar_fps.Size = new Size(300, 70);
            _bar_fps.ValueColumnWidth = 120;
            _bar_fps.Position = new Point(10, 10);

            _fps = new FloatVariable(_bar_fps, 0.0f);
            _fps.Label = "FPS";

            new Separator(_bar_fps);
        }

        //------------------------------------------------------
        // Logs Window
        //------------------------------------------------------.
        private void createWindow_Logs(Context context)
        {
            _bar_logs = new Bar(context);
            _bar_logs.Iconified = true;
            _bar_logs.RefreshRate = 1;    
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
                //_bar_debug.Iconified = GV.display_debugWindow;

                //_bar_logs.Help = DebugHelper.current_log;
                _fps.Value = current_fps;
                _context_debug.Draw();
            }
            catch (Exception e)
            {
                DebugHelper.logError("AntTweakBar error:", e.Message);
            }
        }


    }
}
