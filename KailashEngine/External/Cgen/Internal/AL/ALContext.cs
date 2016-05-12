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

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Platform;

namespace Cgen.Internal.OpenAL
{
    /// <summary>
    /// Defines common OpenAL Context behavior and operations.
    /// </summary>
    public sealed class ALContext : IDisposable
    {
        private AudioContext _context;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ALContext"/> is currently processing audio events.
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                return _context.IsProcessing;
            }
        }

        /// <summary>
        /// Gets a value whether the <see cref="ALContext"/> is synchronized.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return _context.IsSynchronized;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ALContext"/> instance.
        /// </summary>
        public ALContext()
        {
            try
            {
                if (_context == null)
                    _context = new AudioContext();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to create ALContext: " + e.Message);
            }
        }

        internal ALContext(AudioContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Force the <see cref="ALContext"/> to process queued audio events.
        /// </summary>
        public void Process()
        {
            _context.Process();
        }

        /// <summary>
        /// Suspend the <see cref="ALContext"/> from processing audio events.
        /// </summary>
        public void Suspend()
        {
            _context.Suspend();
        }

        /// <summary>
        /// Activate the <see cref="ALContext"/> in the calling thread.
        /// </summary>
        public void Activate()
        {
            _context.MakeCurrent();
        }

        /// <summary>
        /// Dispose <see cref="ALContext"/> Resource.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
