#region License
// ================================================================================
// Cgen.Audio - Audio Submodules of CygnusJam Game Engine
// Copyright (C) 2015 Alghi Fariansyah (com@cxo2.me)
// 
// This software is provided 'as-is', without any express or implied
// warranty.In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software.If you use this software
//    in a product, an acknowledgement in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
// ================================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using OpenTK;
using OpenTK.Audio.OpenAL;

using Cgen;
using Cgen.Utilities;

namespace Cgen.Internal.OpenAL
{
    /// <summary>
    /// Defines OpenAL error checking functions.
    /// </summary>
    public static class ALChecker
    {
        private static bool _isManuallyChecked = true;

        /// <summary>
        /// Defines Verbose level of <see cref="ALChecker"/>.
        /// </summary>
        public enum VerboseFlags
        {
            /// <summary>
            /// Display all messages.
            /// </summary>
#if !DEBUG
            [Obsolete("Changing VerboseLevel to All will lead performance issue under Release Build Configurations.")]
#endif
            All = 0,

            /// <summary>
            /// Display Warning and Error messages only.
            /// </summary>
            Error = 1
        }

        /// <summary>
        /// Gets the last OpenAL Error that occurred.
        /// Any OpenAL call should use <see cref="ALChecker.Check(Action)"/> to make this property working properly.
        /// </summary>
        public static ALError LastError
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or Sets whether the Error Checking should be performed and printed under <see cref="Trace"/> / <see cref="Debug"/> Listeners
        /// Regardless to Build Configurations.
        /// </summary>
        public static bool Verbose
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or Sets Verbose Level of if <see cref="Verbose"/> property return true.
        /// </summary>
        public static VerboseFlags VerboseLevel
        {
            get;
            set;
        }

        static ALChecker()
        {
            Verbose = false;
            VerboseLevel = VerboseFlags.Error;
        }

        /// <summary>
        /// Call specific function and check for OpenAL Error.
        /// </summary>
        /// <param name="func">Function to call.</param>
        public static void Check(Action func)
        {
            func();
            _isManuallyChecked = false;

            if (Verbose)
            {
                CheckError();
                return;
            }
#if DEBUG
            CheckError();
#endif
        }

        /// <summary>
        /// Check for the OpenAL Error.
        /// </summary>
        public static void CheckError()
        {
            ALError errorCode = AL.GetError();
            
            if (errorCode == ALError.NoError)
            {
                if (VerboseLevel == VerboseFlags.All)
                {
                    if (_isManuallyChecked)
                        Logger.StackFrame = 2;
                    else
                        Logger.StackFrame = 3;

                    Logger.Log("AL Operation Success (NoError)", Logger.Level.Information);
                    Logger.StackFrame = 1;
                }

                _isManuallyChecked = true;
                return;
            }

            LastError = errorCode;
            string error = "Unknown Error.";
            string description = "No Description available.";

            // Decode the error code
            switch (errorCode)
            {
                case ALError.InvalidEnum:
                    {
                        error = "AL_INVALID_ENUM";
                        description = "An unacceptable value has been specified for an enumerated argument.";
                        break;
                    }
                case ALError.InvalidValue:
                    {
                        error = "AL_INVALID_VALUE";
                        description = "A numeric argument is out of range.";
                        break;
                    }
                case ALError.InvalidOperation:
                    {
                        error = "AL_INVALID_OPERATION";
                        description = "The specified operation is not allowed in the current state.";
                        break;
                    }
                case ALError.InvalidName:
                    {
                        error = "AL_INVALID_NAME";
                        description = "The specified name is not available.";
                        break;
                    }
                case ALError.OutOfMemory:
                    {
                        error = "AL_OUT_OF_MEMORY";
                        description = "there is not enough memory left to execute the command.";
                        break;
                    }
                default:
                    {
                        error = errorCode.ToString();
                        break;
                    }
            }

            if (_isManuallyChecked)
                Logger.StackFrame = 1;
            else
                Logger.StackFrame = 2;

            Logger.Log(error + ": " + description, Logger.Level.Error);
            Logger.StackFrame = 1;
            _isManuallyChecked = true;
        }
    }
}
