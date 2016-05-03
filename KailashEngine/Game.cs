using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KailashEngine.UI;
using KailashEngine.Role;

namespace KailashEngine
{

    class Game
    {

        static void Main(string[] args)
        {

            OpenGLVersion gl_version = new OpenGLVersion(4, 5);

            Display main_display = new Display("Das Undt Gamen", 1440, 900, false);


            Player player1 = new Player("laydai");



            using (Engine KailashEngine = new Engine(main_display, gl_version, player1))
            {
                KailashEngine.Run(60.0);
            }
        }
    }
}
