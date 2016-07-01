using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntTweakBar;

using OpenTK;


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


        public DebugWindow()
        {
            _context_debug = new Context(Tw.GraphicsAPI.OpenGLCore);
        }


        public void load()
        {
            createWindow_FPS(_context_debug);
        }


        public void unload()
        {
            if (_context_debug != null)
            {
                _context_debug.Dispose();
            }
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
            _bar_fps.Size = new Size(370, 100);
            _bar_fps.ValueColumnWidth = 120;
            _bar_fps.Position = new Point(10, 10);


            _fps = new FloatVariable(_bar_fps, 0.0f);
            _fps.Label = "FPS";

            new Separator(_bar_fps);

            FloatVariable test = new FloatVariable(_bar_fps, 10.0f);
            test.Label = "Timing - gBuffer";
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
