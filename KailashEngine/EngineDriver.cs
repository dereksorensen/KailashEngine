
using System;
using System.Drawing;
using System.Threading;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Cgen.Audio;

using KailashEngine.Output;
using KailashEngine.Client;
using KailashEngine.Render;
using KailashEngine.Render.Shader;

namespace KailashEngine
{
    class EngineDriver : GameWindow
    {

        private Game _game;
        private Audio _audio;

        private Sound _sound;

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


            Keyboard.KeyRepeat = game.keyboard.repeat;
        }

        //------------------------------------------------------
        // Helpers
        //------------------------------------------------------



        //------------------------------------------------------
        // Input Handling
        //------------------------------------------------------

        //Centres the mouse
        private void centerMouse()
        {
            if (_game.mouse.locked)
            {
                Point windowCenter = new Point(
                (base.Width) / 2,
                (base.Height) / 2);

                System.Windows.Forms.Cursor.Position = base.PointToScreen(windowCenter);
            }
            _game.mouse.position_previous = base.PointToClient(System.Windows.Forms.Cursor.Position);
        }

        // Process input per frame
        private void inputBuffer()
        {
            keyboard_Buffer();
            mouse_ButtonBuffer();
            mouse_MoveBuffer();
        }

        // Keyboard

        protected void keyboard_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            _game.keyboard.keyUp(e);
            switch (e.Key)
            {

            }
        }

        protected void keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            _game.keyboard.keyDown(e);
            switch (e.Key)
            {
                case Key.P:
                    
                    _sound.Play();
                    break;

                case Key.CapsLock:
                    _game.mouse.locked = !_game.mouse.locked;
                    if (_game.mouse.locked)
                    {
                        System.Windows.Forms.Cursor.Hide();
                        centerMouse();
                    }
                    else
                    {
                        System.Windows.Forms.Cursor.Show();
                    }
                    break;

                case Key.Escape:
                    Exit();
                    break;
            }
        }

        private void keyboard_Buffer()
        {
            //------------------------------------------------------
            // Smooth Player Movement
            //------------------------------------------------------
            _game.player.character.position_current = _game.player.character.spatial.position - _game.player.character.position_current;
            _game.player.character.position_current = EngineHelper.lerp(_game.player.character.position_previous, _game.player.character.position_current, _game.config.smooth_keyboard_delay);
            _game.player.character.position_previous = _game.player.character.position_current;
            _game.player.character.spatial.position += _game.player.character.position_current;
            _game.player.character.position_current = _game.player.character.spatial.position;


            //------------------------------------------------------
            // Player Movement
            //------------------------------------------------------

            if (_game.keyboard.getKeyPress(Key.W))
            {
                Console.WriteLine("Forward");
                _game.player.character.moveForeward();
            }

            if (_game.keyboard.getKeyPress(Key.S))
            {
                Console.WriteLine("Backward");
                _game.player.character.moveBackward();
            }

            if (_game.keyboard.getKeyPress(Key.A))
            {
                Console.WriteLine("Left");
                _game.player.character.strafeLeft();
            }

            if (_game.keyboard.getKeyPress(Key.D))
            {
                Console.WriteLine("Right");
                _game.player.character.strafeRight();
            }

            if (_game.keyboard.getKeyPress(Key.Space))
            {
                Console.WriteLine("Jump");
                _game.player.character.moveUp();
            }

            if (_game.keyboard.getKeyPress(Key.ControlLeft))
            {
                Console.WriteLine("Crouch");
                _game.player.character.moveDown();
            }

            if (_game.keyboard.getKeyPress(Key.ShiftLeft))
            {
                Console.WriteLine("Run");
            }

            if (_game.keyboard.getKeyPress(Key.AltLeft))
            {
                Console.WriteLine("Sprint");
            }
        }

        // Mouse

        protected void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            _game.mouse.buttonUp(e);
            switch (e.Button)
            {

            }
        }

        protected void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            _game.mouse.buttonDown(e);
            switch (e.Button)
            {

            }
        }

        protected void mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            _game.mouse.wheel(e);


        }

        private void mouse_ButtonBuffer()
        {
            if (_game.mouse.getButtonPress(MouseButton.Left))
            {
                Console.WriteLine("Left Click");
            }

            if (_game.mouse.getButtonPress(MouseButton.Right))
            {
                Console.WriteLine("Right Click");
            }

            if (_game.mouse.getButtonPress(MouseButton.Middle))
            {
                Console.WriteLine("Middle Click");
            }
        }

        private void mouse_MoveBuffer()
        {
            _game.mouse.position_current = (_game.mouse.locked || _game.mouse.getButtonPress(MouseButton.Left)) ? base.PointToClient(System.Windows.Forms.Cursor.Position) : _game.mouse.position_previous;

            // Get Deltas
            Vector3 temp_delta_total = new Vector3();
            Vector3 temp_delta_current = new Vector3(
                (_game.mouse.position_current.Y - _game.mouse.position_previous.Y) * _game.mouse.sensitivity,
                (_game.mouse.position_current.X - _game.mouse.position_previous.X) * _game.mouse.sensitivity,
                (_game.mouse.position_current.X - _game.mouse.position_previous.X) * _game.mouse.sensitivity);
            Vector3 temp_delta_previous = _game.mouse.delta_previous;

            // Interpolate for smooth mouse       
            temp_delta_current.Xy = EngineHelper.lerp(temp_delta_previous.Xy, temp_delta_current.Xy, _game.config.smooth_mouse_delay);
            temp_delta_current.Z = MathHelper.Clamp(temp_delta_current.Z, -9.0f, 9.0f);
            temp_delta_current.Z = EngineHelper.lerp(temp_delta_previous.Z, temp_delta_current.Z, _game.config.smooth_mouse_delay * 1.5f);

            // Calculate total delta (which is rotation angle for character / camera)
            temp_delta_total.X += (_game.mouse.delta_total.X + temp_delta_current.X);
            temp_delta_total.Y += (_game.mouse.delta_total.Y + temp_delta_current.Y);
            float z_mod = (float)Math.Cos(temp_delta_current.X * Math.PI / 180.0f) * _game.config.smooth_mouse_delay;
            temp_delta_total.Z = temp_delta_current.Z * z_mod;

            // Prevent looking beyond top and bottom
            if (temp_delta_total.X < -90.0f)
            {
                temp_delta_total.X = -90.0f;
            }
            if (temp_delta_total.X > 90.0f)
            {
                temp_delta_total.X = 90.0f;
            }

            _game.mouse.delta_previous = temp_delta_current;
            _game.mouse.delta_total = temp_delta_total;

            // Rotate main character from mouse movement
            _game.player.character.rotate(_game.mouse.delta_total.X, _game.mouse.delta_total.Y, _game.mouse.delta_total.Z);

            // Recenter
            centerMouse();
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
            centerMouse();
            // this is called when the window starts running

            Program test = new Program(_game.config.glsl_version,
                new ShaderFile[] {
                    new ShaderFile(ShaderType.VertexShader, _game.config.path_glsl_base + "test.vert", null) });

            //_game.config.path_base + "Output/test1.ogg"

            //SoundSystem sound_system = new SoundSystem();
            SoundSystem.Instance.Initialize();
            _sound = new Sound(_game.config.path_base + "Output/test1.ogg");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            SoundSystem.Instance.Update(e.Time);
            _sound.Position3D = new float[] { _game.player.character.spatial.position.X, _game.player.character.spatial.position.Y, _game.player.character.spatial.position.Z };

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            inputBuffer();

            //Console.WriteLine(_game.player.character.spatial.position);
            //Console.WriteLine(_game.player.character.spatial.look);




            Debug.DebugHelper.logGLError();

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.Dispose();

            _game.unload();

            Console.WriteLine("\nGoodbye...");
            Thread.Sleep(500);
        }

    }
}
