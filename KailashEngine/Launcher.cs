using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;

namespace KailashEngine
{
    class Launcher
    {

        static void Main(string[] args)
        {

            // Setup Game Objects

            ClientConfig game_config = new ClientConfig(
                "Das Undt Gamen",                               // Game Name
                "KailashEngine",                                // Base Engine Path

                // OpenGL Config
                4, 5,                                           // OpenGL Versions

                // Rendering Config
                60.0f,                                          // Target FPS
                65.0f,                                          // Field of view
                0.1f, 1000.0f,                                  // Near / Far Planes

                // Gameplay Config
                0.2f, 0.1f,                                     // Smooth Mouse / Keyboard

                // Display Config
                1440, 900,                                      // Resolution
                true,                                          // Default to fullscreen

                // Main Player Config
                0.1f, 0.5f,                                    // Character Movement Speeds
                0.1f                                            // Character Look Sensitivity
            );


            // Initialize Game
            Game game = new Game(game_config);


            using (EngineDriver KailashEngine = new EngineDriver(game))
            {
                string version = GL.GetString(StringName.Version);
                if (version.Substring(0, 3) == game.config.gl_version_string)
                {
                    Console.WriteLine(version + "\n");
                    KailashEngine.Run(game.config.fps_target);
                }
                else
                {
                    throw new OpenTK.GraphicsException("Requested OpenGL version not available\nRequested:\t" + game.config.gl_version_string + "\nHighest Available:\t" + version);
                }
            }
        }

    }
}
