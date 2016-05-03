
using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using KailashEngine.UI;
using KailashEngine.Role;

namespace KailashEngine
{
    class Engine : GameWindow
    {

        private Display _main_display;
        private OpenGLVersion _gl_version;
        private Player _main_player;

        public Engine(Display main_display, OpenGLVersion gl_version, Player main_player) :
            base(main_display.resolution.W, main_display.resolution.H,
                new GraphicsMode(new ColorFormat(32), 32, 32, 1),
                main_display.title,
                main_display.fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
                DisplayDevice.Default, 
                gl_version.major, gl_version.minor,
                GraphicsContextFlags.Debug)
        {
            _main_display = main_display;
            _gl_version = gl_version;

            VSync = VSyncMode.On;

            // Register Input Devices
            Keyboard.KeyUp += keyboard_KeyUp;
            Keyboard.KeyDown += keyboard_KeyDown;

            Mouse.ButtonUp += main_player.mouse.mouseUp;
            Mouse.ButtonDown += main_player.mouse.mouseDown;
            Mouse.WheelChanged += main_player.mouse.mouseWheel;

            Keyboard.KeyRepeat = main_player.keyboard.repeat;

        }


        protected void keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _main_player.keyboard.keyUp(e);

        }

        protected void keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }

            _main_player.keyboard.keyDown(e);

        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            // this is called when the window starts running
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {


            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.Dispose();
        }

    }
}
