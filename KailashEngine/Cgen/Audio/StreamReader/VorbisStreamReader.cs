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
    using System.IO;

    using Cgen;
    using Cgen.Internal.OpenAL;

    using OpenTK;
    using OpenTK.Audio;
    using OpenTK.Audio.OpenAL;

    using NVorbis;

    internal class VorbisStreamReader : VorbisReader, ISoundStreamReader
    {
        private object _readMutex = new object();

        public SoundBuffer Sound
        {
            get;
            private set;
        }

        public int DecodeTime
        {
            get { return (int)DecodedTime.TotalMilliseconds; }
            set { DecodedTime = TimeSpan.FromMilliseconds(value); }
        }

        public int LengthTime
        {
            get { return (int)base.TotalTime.TotalMilliseconds; }
        }
        
        public VorbisStreamReader(SoundBuffer sound, bool closeStreamOnDispose = false)
            : base(sound.Stream, closeStreamOnDispose)
        {
            Sound = sound;
        }

        public bool BufferData(int bufferSize, int bufferId)
        {
            float[] readSampleBuffer = new float[SoundSystem.Instance.BufferSize];
            short[] castBuffer = new short[SoundSystem.Instance.BufferSize];

            int readSamples = ReadSamples(readSampleBuffer, 0, bufferSize);
            CastBuffer(readSampleBuffer, castBuffer, readSamples);

            ALChecker.Check(() => AL.BufferData(bufferId, Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, castBuffer,
                          readSamples * sizeof(short), SampleRate));

            return readSamples != bufferSize;
        }

        public static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var temp = (int)(32767f * inBuffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                outBuffer[i] = (short)temp;
            }
        }
    }
}
