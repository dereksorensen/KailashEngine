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

using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Cgen.Internal.OpenAL
{
    /// <summary>
    /// Defines a list of OpenAL Extensions that could processed by the hardware.
    /// </summary>
    public sealed class ALExtensions
    {
        /// <summary>
        /// Check availability of the OpenAL extension in current hardware.
        /// </summary>
        /// <param name="ext">Extension name to check.</param>
        /// <returns>Return true if available, otherwise false.</returns>
        internal static bool IsAvailable(string ext)
        {
            return AudioContext.CurrentContext.SupportsExtension(ext);
        }
    }
}
