using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using OpenTK.Graphics.OpenGL;

using KailashEngine.Client;

using KailashEngine.Scripting;

namespace KailashEngine
{
    class Launcher
    {

        static void Main(string[] args)
        {

            // Setup Game Objects

            ClientConfig game_config = new ClientConfig(
                "Das Undt Gamen",                               // Game Name

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

            //*************************************************************************************
            //** Scripting Tests begin Here 
            //*************************************************************************************

            try
            {
                //Initialize ScriptBase (static class) and add initial test environment ("testbed")
                //TODO: Make ScriptBase non-static so multiple instances can be created for multithreading
                var scriptBase = new LuaScriptBase();
                scriptBase.CreateEnvironment("testbed");

                //Add a script named "test" with the Lua source "return a + b", which accepts two arguments (int a, int b).
                //The fourth argument is true because the third argument is Lua source code rather than a filename.
                scriptBase.AddScript("testbed", "test", "return a + b", true,
                                    new KeyValuePair<string, Type>("a", typeof(int)),
                                    new KeyValuePair<string, Type>("b", typeof(int)));

                //Run the "test" script and fetch the int return value
                var result = scriptBase.RunScript<int>("testbed", "test", 10, 20);
                Console.WriteLine("Script test: 10 + 20 = " + result);

                //Reverse scenario: Lua calling C# code
                //Create a Func object out of a lambda, neat and clean.
                Func<double> GetPi = () =>
                {
                    return Math.PI;
                };

                //Add our C# function cSharpFunc to environment "testbed"; 
                //can now be used by any script in that environment
                scriptBase.AddFunction("testbed", "getPi", GetPi);

                //Add another script and call it
                scriptBase.AddScript("testbed", "funcTest", "return getPi()", true);
                var result2 = scriptBase.RunScript<double>("testbed", "funcTest");

                Console.WriteLine("Pi, according to Lua by way of C# = " + result2);

                //Add a script containing a very simple Lua function to be called from C#
                //The possibilities here are great: You could write a script 
                //with functions like "onUpdate()", "onDestroy()", "onCreate()", etc. (which
                //in turn call native C# functions), and call those in C#
                //at the appropriate time; letting you drive your game logic through Lua
                scriptBase.AddScript("testbed", "getFunc", "function luaSays(text) \n" +
                                                            //You get full access to the CLR through the clr namespace in Lua. So cool.
                                                            "    clr.System.Console.WriteLine('From Lua: ' .. text) \n" + // '..' is the Lua string concatenation operator; weird, I know!
                                                            "    return text \n" +
                                                            "end",
                                                            true);

                //Must run the script initially so Lua function winds up in the environment.
                //TODO: Make this step transparent (i.e. have the option for the system to run
                //      the script automatically upon adding to establish the Lua functions it
                //      contains; effectively a "compilation" step).
                scriptBase.RunScript("testbed", "getFunc");

                //The global space (environment) now contains the "luaSays(text)" Lua function,
                //so we can assign it to a C# function object and call it at will.
                var printline = scriptBase.GetFunction<string>("testbed", "luaSays");
                var result3 = printline("hey!");

                Console.WriteLine("From C#: " + result3.ToType(typeof(string)));

                //There're also helper methods to call Lua functions directly and extract the result
                //Generic syntax is <Return Type, Argument 1 Type, Argument 2 Type, etc.> (up to four arguments).
                string result4 = scriptBase.CallFunction<string, string>("testbed", "luaSays", "You know it!");
                Console.WriteLine("From C#: " + result4);

                //Setting globals can be useful for things like passing an ID through for the script to operate on,
                //or for providing global constants to the scripts.
                scriptBase.SetGlobal("testbed", "GLOBAL_ID_STRING", "1234567890ABCDEF");
                scriptBase.SetGlobal("testbed", "GOOBLE_GOBBLE", "one of us one of us");
                scriptBase.SetGlobal("testbed", "BOOBS", 80085);
                scriptBase.SetGlobal("testbed", "this", 0xFF903909);

                for (int i = 0; i < 10; i++)
                {
                    //Programatically set globals double0 through double9. This is a fairly useless example, but the
                    //possibilities are cool.        
                    scriptBase.SetGlobal("testbed", "double" + i, i * 2);
                }

                //Again, luaSays(text) is now in the global environment, so we can use it again here without redefining.
                //AddScript effectively functions like an "include" or "using" system. You could have a factory that
                //returns collections of scripts (building a module system and default set of libraries that call engine 
                //functions -- or simplified wrappers around engine functions -- in a contextually appropriate way would be a natural
                //extension of this).
                scriptBase.AddScript("testbed", "globals", "luaSays(GLOBAL_ID_STRING) \n luaSays(GOOBLE_GOBBLE) \n luaSays(tostring(BOOBS)) \n" +
                                                           "luaSays(tostring(double9)) \n luaSays(tostring(double5)) \n luaSays(tostring(this)) \n",
                                                           true);
                scriptBase.RunScript("testbed", "globals");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //*************************************************************************************
            //** Scripting Tests end here, set a breakpoint at next statement 
            //** to examine console output before game assets load
            //*************************************************************************************

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
