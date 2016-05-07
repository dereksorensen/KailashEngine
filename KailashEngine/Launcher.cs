using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.UI;

namespace KailashEngine
{
    class Launcher
    {

        static void Main(string[] args)
        {

            // Setup Game Objects

            ClientConfig game_config = new ClientConfig(
                "Das Undt Gamen",
                "KailashEngine",
                4, 5,
                60.0f,
                0.01f, 1000.0f);
            Display main_display = new Display(
                game_config.title, 
                1440, 900, 
                false);
            Player main_player = new Player(
                new World.Role.PlayableCharacter(
                    "laydai",
                    0.2f,
                    0.2f)
                );


            // Initialize Game
            Game game = new Game(
                game_config,
                main_display,
                main_player);


            using (EngineDriver KailashEngine = new EngineDriver(game))
            {
                string version = GL.GetString(StringName.Version);
                if (version.Substring(0, 3) == game.config.gl_version_string)
                {
                    Console.WriteLine(version);
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
