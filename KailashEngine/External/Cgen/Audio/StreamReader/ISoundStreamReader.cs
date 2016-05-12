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

namespace Cgen.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represent a Stream Reader to decode the Audio.
    /// </summary>
    public interface ISoundStreamReader : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="SoundBuffer"/> handle of the <see cref="ISoundStreamReader"/>.
        /// </summary>
        SoundBuffer Sound { get; }

        /// <summary>
        /// Gets or Sets the current position of Decoding, in milisecond.
        /// </summary>
        int DecodeTime { get; set; }

        /// <summary>
        /// Gets the length of Reader in millisecond.
        /// </summary>
        int LengthTime { get; }

        /// <summary>
        /// Upload the buffer data samples.
        /// </summary>
        /// <param name="bufferSize">The size of buffer.</param>
        /// <param name="bufferId">Buffer handle.</param>
        /// <returns></returns>
        bool BufferData(int bufferSize, int bufferId);
    }
}
