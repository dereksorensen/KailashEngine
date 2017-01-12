
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
using KailashEngine.Physics;

namespace KailashEngine
{
    class EngineDriver : GameWindow
    {

        // Engine Objects
        private RenderDriver _render_driver;
        private float _fps;

        // Physics Objects
        private PhysicsDriver _physics_driver;

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
        private Vector3 getWorldSpaceRay()
        {
            float x = (2.0f * _game.mouse.position_current.X) / _game.display.resolution.W - 1.0f;
            float y = 1.0f - (2.0f * _game.mouse.position_current.Y) / _game.display.resolution.H;
            float z = 1.0f;
            Vector3 mouse_ray = new Vector3(x, y, z);
            Vector4 ray_clip = new Vector4(mouse_ray.X, mouse_ray.Y, -1.0f, 1.0f);
            Vector4 ray_eye = Vector4.Transform(ray_clip, Matrix4.Invert(_game.player.camera.spatial.perspective));
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);

            return Vector3.Normalize(Vector4.Transform(ray_eye, Matrix4.Invert(_game.player.camera.spatial.model_view)).Xyz);
        }

        private Vector3[] getPickingVectors()
        {
            Vector3 ray_world = getWorldSpaceRay();
            Vector3 start = -_game.player.camera.spatial.position;

            ray_world = Vector3.TransformPosition(ray_world * _game.config.near_far.Y, Matrix4.CreateTranslation(start));

            return new Vector3[]{
                start,
                ray_world
            };
        }


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

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            _debug_window.handle_Keyboard(e);
        }

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
                case Key.F:
                    _game.scene.toggleFlashlight(_game.player.toggleFlashlight());
                    break;
                case Key.O:
                    Thread.Sleep(2000);
                    break;
                case Key.P:
                    _physics_driver.pause();
                    _game.scene.pauseAnimation();
                    break;
                case Key.R:
                    _physics_driver.reset();
                    _game.scene.resetAnimation();
                    break;
                case Key.X:
                    _physics_driver.throwObject(EngineHelper.otk2bullet(getWorldSpaceRay()));
                    break;
                case Key.Z:
                    _physics_driver.zoomPickedObject();
                    break;
                case Key.Tab:
                    _game.player.togglePhysical();
                    break;
                case Key.CapsLock:
                    _game.mouse.toggleLock();
                    if (_game.mouse.locked)
                    {
                        centerMouse();
                    }
                    break;
                case Key.F8:
                    _debug_window.toggleTimers();
                    break;
                case Key.F9:
                    _render_driver.toggleDebugViews();
                    break;
                case Key.F10:
                    _render_driver.toggleWireframe();
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
            _game.player.smoothMovement(_game.config.smooth_keyboard_delay);


            //------------------------------------------------------
            // Player Movement
            //------------------------------------------------------

            // Running
            _game.player.run(_game.keyboard.getKeyPress(Key.ShiftLeft));

            // Sprinting
            _game.player.sprint(_game.keyboard.getKeyPress(Key.AltLeft));


            if (_game.keyboard.getKeyPress(Key.W))
            {
                //Console.WriteLine("Forward");
                _game.player.moveForeward();
            }

            if (_game.keyboard.getKeyPress(Key.S))
            {
                //Console.WriteLine("Backward");
                _game.player.moveBackward();
            }

            if (_game.keyboard.getKeyPress(Key.A))
            {
                //Console.WriteLine("Left");
                _game.player.strafeLeft();
            }

            if (_game.keyboard.getKeyPress(Key.D))
            {
                //Console.WriteLine("Right");
                _game.player.strafeRight();
            }

            if (_game.keyboard.getKeyPress(Key.Space))
            {
                //Console.WriteLine("Jump");
                _game.player.moveUp();
            }

            if (_game.keyboard.getKeyPress(Key.ControlLeft))
            {
                //Console.WriteLine("Crouch");
                _game.player.moveDown();
            }

            // Update Physics Character Position
            _game.player.updatePhysicalPosition();


        }

        // Mouse

        protected void mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_debug_window.handle_MouseClick(e))
            {
                return;
            }

            _game.mouse.buttonUp(e);
            switch (e.Button)
            {
                case MouseButton.Left:
                    _physics_driver.releaseObject();
                    break;
            }
        }

        protected void mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_debug_window.handle_MouseClick(e))
            {
                return;
            }

            _game.mouse.buttonDown(e);
            switch (e.Button)
            {
                case MouseButton.Left:

                    Vector3[] picking_vectors = getPickingVectors();
                    _physics_driver.pickObject(EngineHelper.otk2bullet(picking_vectors[0]), EngineHelper.otk2bullet(picking_vectors[1]), !_game.keyboard.getKeyPress(Key.AltLeft));

                    break;
                case MouseButton.Right:
                    //_sound_cow.Play();
                    _sound_goat.Play();
                    break;
            }
        }

        protected void mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            if (_debug_window.handle_MouseWheel(e))
            {
                return;
            }

            _game.mouse.wheel(e);

            _game.scene.circadian_timer.time += e.Delta / 10.0f;

        }

        private void mouse_ButtonBuffer()
        {
            // Zoom camera
            _game.player.camera.zoom(_game.mouse.getButtonPress(MouseButton.Right), _fps);

            if (_game.mouse.getButtonPress(MouseButton.Left))
            {
                //Console.WriteLine("Left Click");
            }

            if (_game.mouse.getButtonPress(MouseButton.Right))
            {
                //Console.WriteLine("Right Click");   
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

            // Don't move game if we are using the tweak bar
            if (!_debug_window.handle_MouseMove(_game.mouse.position_current))
            {
                // Rotate main character from mouse movement
                _game.player.rotate(
                    _game.mouse.position_delta, 
                    _game.config.smooth_mouse_delay
                );

                // Move Picked Object
                Vector3[] picking_vectors = getPickingVectors();
                _physics_driver.moveObject(EngineHelper.otk2bullet(picking_vectors[0]), EngineHelper.otk2bullet(picking_vectors[1]));
            }


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
                new ProgramLoader(_game.config.glsl_version),
                new StaticImageLoader(EngineHelper.path_resources_textures_static),
                _game.display.resolution
            );
            _debug_window = new DebugWindow();
            _physics_driver = new PhysicsDriver();


            // Load Objects
            _game.load(_physics_driver.physics_world);
            _render_driver.load();        
            _debug_window.load();

            // Load static UBOs
            _render_driver.updateUBO_GameConfig(
                _game.config.near_far_full,
                _game.config.fps_target);

            // Load Sound System
            SoundSystem.Instance.Initialize();
            _sound_cow = new Sound(EngineHelper.path_resources_audio + "cow.ogg");
            _sound_cow.IsRelativeToListener = false;
            _sound_goat = new Sound(EngineHelper.path_resources_audio + "test1.ogg");

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            _physics_driver.update((float)e.Time, _game.config.fps_target, _fps);

            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _fps = (float)(1.0d / e.Time);

            inputBuffer();




            // Flashlight stuff

            Matrix4 tempMat = _game.scene.flashlight.bounding_unique_mesh.transformation;

            _game.scene.flashlight.unique_mesh.transformation = Matrix4.Invert(_game.player.character.spatial.transformation);
            _game.scene.flashlight.bounding_unique_mesh.transformation = tempMat * Matrix4.Invert(_game.player.character.spatial.transformation);
            _game.scene.flashlight.spatial.position = -_game.player.character.spatial.position;
            _game.scene.flashlight.spatial.rotation_matrix = Matrix4.Transpose(_game.player.character.spatial.rotation_matrix);


            // Update Scene
            _game.scene.update(_game.player.camera.spatial);

            // Update Dynamic UBOs
            _render_driver.updateUBO_Camera(
                _game.player.camera.spatial.transformation,
                _game.player.camera.spatial.perspective,
                Matrix4.Invert(_game.player.camera.spatial.model_view.ClearTranslation() * _game.player.camera.spatial.perspective),
                _game.player.camera.previous_view_perspective,
                Matrix4.Invert(_game.player.camera.previous_view_matrix.ClearTranslation() * _game.player.camera.previous_perspective_matrix),
                _game.player.camera.spatial.position,
                _game.player.camera.spatial.look);
            _render_driver.handle_MouseState(_game.mouse.locked);


            // Set camera's previous MVP matrix
            _game.player.camera.previous_view_matrix = _game.player.camera.spatial.model_view;
            _game.player.camera.previous_perspective_matrix = _game.player.camera.spatial.perspective;


            


            _render_driver.render(_game.scene, _game.player.camera.spatial);


            _game.scene.flashlight.bounding_unique_mesh.transformation = tempMat;



            SoundSystem.Instance.Update(e.Time, _game.player.character.spatial.position, _game.player.character.spatial.look, _game.player.character.spatial.up);

            _debug_window.render(_fps);


            DebugHelper.logGLError();
            SwapBuffers();

        }

        protected override void OnUnload(EventArgs e)
        {
            base.Dispose();

            _game.unload();
            _debug_window.unload();
            _physics_driver.unload();
            
            DebugHelper.logInfo(0, "\n\nExiting...", "BAYEEE\n\n");
            Thread.Sleep(500);
        }

    }
}
