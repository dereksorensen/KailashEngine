
using System;
using System.Drawing;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using KailashEngine.Client;


namespace KailashEngine
{
    class EngineDriver : GameWindow
    {

        private Game _game;

        public EngineDriver(Game game) :
            base(game.main_display.resolution.W, game.main_display.resolution.H,
                new GraphicsMode(new ColorFormat(32), 32, 32, 1),
                game.title,
                game.main_display.fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
                DisplayDevice.Default,
                game.config.gl_major_version, game.config.gl_minor_version,
                GraphicsContextFlags.Debug)
        {
            _game = game;

            VSync = VSyncMode.On;

            // Register Input Devices
            Keyboard.KeyUp += keyboard_KeyUp;
            Keyboard.KeyDown += keyboard_KeyDown;

            Mouse.ButtonUp += mouse_ButtonUp;
            Mouse.ButtonDown += mouse_ButtonDown;
            Mouse.WheelChanged += mouse_Wheel;

            Keyboard.KeyRepeat = game.main_player.keyboard.repeat;

        }

        //------------------------------------------------------
        // Input Handling
        //------------------------------------------------------

        // Keyboard

        protected void keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _game.main_player.keyboard.keyUp(e);
        }

        protected void keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }
            _game.main_player.keyboard.keyDown(e);
        }

        // Mouse

        protected void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            _game.main_player.mouse.buttonUp(e);
        }

        protected void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            _game.main_player.mouse.buttonDown(e);
        }

        protected void mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            _game.main_player.mouse.wheel(e);
        }

        //------------------------------------------------------
        // Game Loop Handling
        //------------------------------------------------------

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
            _game.main_player.input_Buffer();


            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.Dispose();


            Console.WriteLine("\nGoodbye...");
            Thread.Sleep(1000);
        }

    }
}
