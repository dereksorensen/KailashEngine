using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;
using KailashEngine.UI;
using KailashEngine.World.Role;

namespace KailashEngine
{
    class Launcher
    {

        static void Main(string[] args)
        {

            ClientConfig game_config = new ClientConfig(
                "Das Undt Gamen",
                4, 5,
                60.0f,
                0.01f, 1000.0f);

            Game game = new Game(
                game_config,
                new Display(game_config.title, 1440, 900, false),
                new Player("laydai"));


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
