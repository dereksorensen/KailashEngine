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

            string game_name = "Das Udnt Gamen";

            Game game = new Game(
                game_name,
                new OpenGLVersion(4, 5),
                new Display(game_name, 1440, 900, false),
                new Player("laydai"));


            using (EngineDriver KailashEngine = new EngineDriver(game))
            {
                string version = GL.GetString(StringName.Version);
                if (version.Substring(0, 3) == game.gl_version.version)
                {
                    Console.WriteLine(version);
                    KailashEngine.Run(60.0);
                }
                else
                {
                    throw new OpenTK.GraphicsException("Requested OpenGL version not available\nRequested:\t" + game.gl_version.version + "\nHighest Available:\t" + version);
                }
            }
        }

    }
}
