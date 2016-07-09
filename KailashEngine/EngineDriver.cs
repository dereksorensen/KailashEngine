
using System;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Cgen.Audio;

using KailashEngine.Debug;
using KailashEngine.Output;
using KailashEngine.Client;
using KailashEngine.Render;
using KailashEngine.Render.Shader;

namespace KailashEngine
{
    class EngineDriver : GameWindow
    {

        // Engine Objects
        private RenderDriver _render_driver;
        private float _fps;

        // Game Objects
        private Game _game;

        // Debug Objects
        private DebugWindow _debug_window;



        private Sound _sound_cow;
        private Sound _sound_goat;


        public EngineDriver(Game game) :
            base(game.display.resolution.W, game.display.resolution.H,
                new GraphicsMode(new ColorFormat(32), 32, 32, 1),
                game.title,
                game.display.fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
                DisplayDevice.Default,
                game.config.gl_major_version, game.config.gl_minor_version,
                GraphicsContextFlags.Default)
        {
            _game = game;

            // Display Settings
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

                case Key.CapsLock:
                    _game.mouse.toggleLock();
                    if (_game.mouse.locked)
                    {
                        centerMouse();
                    }
                    break;
                case Key.F:
                    _game.player.enable_flashlight = !_game.player.enable_flashlight;
                    _game.scene.toggleFlashlight(_game.player.enable_flashlight);
                    break;
                case Key.Escape:
                    Exit();
                    break;
            }
        }

        private void keyboard_Buffer()
        {
            //------------------------------------------------------
            // Smooth Camera Movement
            //------------------------------------------------------
            _game.player.camera.smoothMovement(_game.config.smooth_keyboard_delay);

            //------------------------------------------------------
            // Player Movement
            //------------------------------------------------------

            if (_game.keyboard.getKeyPress(Key.W))
            {
                //Console.WriteLine("Forward");
                _game.player.character.moveForeward();
            }

            if (_game.keyboard.getKeyPress(Key.S))
            {
                //Console.WriteLine("Backward");
                _game.player.character.moveBackward();
            }

            if (_game.keyboard.getKeyPress(Key.A))
            {
                //Console.WriteLine("Left");
                _game.player.character.strafeLeft();
            }

            if (_game.keyboard.getKeyPress(Key.D))
            {
                //Console.WriteLine("Right");
                _game.player.character.strafeRight();
            }

            if (_game.keyboard.getKeyPress(Key.Space))
            {
                //Console.WriteLine("Jump");
                _game.player.character.moveUp();
            }

            if (_game.keyboard.getKeyPress(Key.ControlLeft))
            {
                //Console.WriteLine("Crouch");
                _game.player.character.moveDown();
            }

            // Running
            _game.player.character.running = _game.keyboard.getKeyPress(Key.ShiftLeft);

            // Sprinting
            _game.player.character.sprinting = _game.keyboard.getKeyPress(Key.AltLeft);

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
                case MouseButton.Left:

                    _sound_cow.Play();
                    break;
                case MouseButton.Right:

                    _sound_goat.Play();
                    break;
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
                //Console.WriteLine("Left Click");
            }

            if (_game.mouse.getButtonPress(MouseButton.Right))
            {
                //Console.WriteLine("Right Click");
                _game.player.camera.zoom(true, _fps);
            }
            else
            {
                _game.player.camera.zoom(false, _fps);
            }

            if (_game.mouse.getButtonPress(MouseButton.Middle))
            {
                //Console.WriteLine("Middle Click");
            }
        }

        private void mouse_MoveBuffer()
        {
            // Calculate mouse current position
            _game.mouse.position_current = (_game.mouse.locked || _game.mouse.getButtonPress(MouseButton.Left)) ? base.PointToClient(System.Windows.Forms.Cursor.Position) : _game.mouse.position_previous;

            // Set character angles based on mouse position delta 
            Vector3 temp_angles = _game.player.character.spatial.rotation_angles + _game.mouse.position_delta;
            temp_angles.X = MathHelper.Clamp(temp_angles.X, -90.0f, 90.0f);
            _game.player.character.spatial.rotation_angles = temp_angles;

            // Rotate main character from mouse movement
            _game.player.character.rotate(_game.player.character.spatial.rotation_angles.X, _game.player.character.spatial.rotation_angles.Y, 0.0f, _game.config.smooth_mouse_delay);

            // Recenter
            centerMouse();
        }


        //------------------------------------------------------
        // Game Loop Handling
        //------------------------------------------------------

        protected override void OnResize(EventArgs e)
        {
            //_game.display.resolution.W = this.Width;
            //_game.display.resolution.H = this.Height;
            GL.Viewport(0, 0, this.Width, this.Height);
            _debug_window.resize(ClientSize);
        }

        protected override void OnLoad(EventArgs e)
        {
            centerMouse();


            // Create Engine Objects
            _render_driver = new RenderDriver(
                new ProgramLoader(_game.config.glsl_version, _game.config.path_glsl_base, _game.config.path_glsl_common, _game.config.path_glsl_common_helpers),
                _game.display.resolution
            );
            _debug_window = new DebugWindow();



            // Load Objects
            _game.load();
            _render_driver.load();        
            _debug_window.load();
            
            
 
            SoundSystem.Instance.Initialize();
            _sound_cow = new Sound(_game.config.path_base + "Output/cow.ogg");
            _sound_cow.IsRelativeToListener = false;
            _sound_goat = new Sound(_game.config.path_base + "Output/test1.ogg");

            
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _fps = (float)(1.0d / e.Time);

            inputBuffer();

            _render_driver.updateUBO_GameConfig(
                _game.config.near_far_full,
                _game.config.fps_target);
            _render_driver.updateUBO_Camera(
                _game.player.camera.spatial.transformation,
                _game.player.camera.spatial.perspective,
                _game.player.camera.spatial.position,
                _game.player.camera.spatial.look);





            Matrix4 tempMat = _game.scene.flashlight.bounding_unique_mesh.transformation;

            _game.scene.flashlight.unique_mesh.transformation = Matrix4.Invert(_game.player.character.spatial.transformation);
            _game.scene.flashlight.bounding_unique_mesh.transformation = tempMat * Matrix4.Invert(_game.player.character.spatial.transformation);
            _game.scene.flashlight.spatial.position = -_game.player.character.spatial.position;
            _game.scene.flashlight.spatial.rotation_matrix =  Matrix4.Transpose(_game.player.character.spatial.rotation_matrix);


            _render_driver.render(_game.scene);


            _game.scene.flashlight.bounding_unique_mesh.transformation = tempMat;






            SoundSystem.Instance.Update(e.Time, -_game.player.character.spatial.position, _game.player.character.spatial.look, _game.player.character.spatial.up);


            _debug_window.render(_fps);

            Debug.DebugHelper.logGLError();
            SwapBuffers();
            
        }

        protected override void OnUnload(EventArgs e)
        {
            base.Dispose();

            _game.unload();
            _debug_window.unload();

            Console.WriteLine("\nBaaiiii...");
            Thread.Sleep(500);
        }

    }
}
