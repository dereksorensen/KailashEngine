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

    using OpenTK;
    using OpenTK.Audio;
    using OpenTK.Audio.OpenAL;

    using Cgen;
    using Cgen.Internal.OpenAL;
    using Cgen.IO;
    using Cgen.Utilities;

    internal class WaveStreamReader : BufferStream, ISoundStreamReader
    {
        enum AudioFormat
        {
            PCM = 1,
            Float = 3
        }

        private object _readMutex = new object();
        private bool _closeStreamOnDispose;
        private ALFormat _alFormat;
        private AudioFormat _type;
        private bool _notSupported = false;

        public SoundBuffer Sound { get; private set; }

        public int DecodeTime
        {
            get { return TimeSpan.FromSeconds((double)Position / SampleRate).Milliseconds; }
            set { Position = (long)(TimeSpan.FromMilliseconds(value).TotalSeconds * SampleRate); }
        }

        public int LengthTime
        {
            get { return TimeSpan.FromSeconds((double)base.Length / SampleRate).Milliseconds; }
        }

        public int Bits { get; private set; }
        public int Channels { get; private set; }
        public int SampleRate { get; private set; }

        public WaveStreamReader(SoundBuffer sound, bool closeStreamOnDispose = false)
            : base()
        {
            Sound = sound;
            _closeStreamOnDispose = closeStreamOnDispose;

            LoadData();
        }

        public bool BufferData(int bufferSize, int bufferId)
        {
            byte[] readSampleBuffer = new byte[SoundSystem.Instance.BufferSize];
            int readSamples = Read(readSampleBuffer, 0, bufferSize);

            ALChecker.Check(() => AL.BufferData(bufferId, _alFormat, readSampleBuffer,
                                  readSamples, SampleRate));

            return readSamples != bufferSize;
        }

        private void LoadData()
        {
            using (BufferStream stream = new BufferStream())
            {
                CopySoundStream(stream);

                // RIFF header
                string signature = new string(stream.GetChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = stream.GetInt32();

                string format = new string(stream.GetChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(stream.GetChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = stream.GetInt32();
                int audio_format = stream.GetInt16();
                int num_channels = stream.GetInt16();
                int sample_rate = stream.GetInt32();
                int byte_rate = stream.GetInt32();
                int block_align = stream.GetInt16();
                int bits_per_sample = stream.GetInt16();
                
                string data_signature = new string(stream.GetChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = stream.GetInt32();

                Channels = num_channels;
                Bits = bits_per_sample;
                SampleRate = sample_rate;
                _type = (AudioFormat)audio_format;

                byte[] buffer = stream.GetRemaining();

                if (Channels == 1)
                {
                    if (Bits == 8)
                        _alFormat = ALFormat.Mono8;
                    else if (Bits == 16)
                        _alFormat = ALFormat.Mono16;
                    else
                    {
                        Logger.Warning(Bits.ToString() + "bit WAV detected, " +
                            "Samples will be converted to 16bit PCM.");
                        _alFormat = ALFormat.Mono16;
                        
                        DecodeTo16Bit(ref buffer);
                    }
                }
                else
                {
                    if (Bits == 8)
                        _alFormat = ALFormat.Stereo8;
                    else if (Bits == 16)
                        _alFormat = ALFormat.Stereo16;
                    else
                    {
                        Logger.Warning(Bits.ToString() + "bit WAV detected, " +
                            "Samples will be converted to 16bit PCM.");
                        _alFormat = ALFormat.Stereo16;

                        DecodeTo16Bit(ref buffer);
                    }
                }

                Write(buffer);
                Position = 0;
            }
        }

        private void DecodeTo16Bit(ref byte[] buffer)
        {
            using (BufferStream raw = new BufferStream(buffer))
            using (BufferStream decoded = new BufferStream())
            {
                // Convert 32bit WAV
                if (Bits == 32)
                {
                    if (_type == AudioFormat.PCM)
                    {
                        while (raw.HasRemaining)
                        {
                            int sample = raw.GetInt32();
                            short temp = (short)(sample >> 16);

                            decoded.Write(temp);
                        }
                    }
                    // Convert 32bit Float WAV
                    else if (_type == AudioFormat.Float)
                    {
                        while (raw.HasRemaining)
                        {
                            float sample = raw.GetFloat();
                            short temp = (short)(sample * 32767.0f);

                            decoded.Write(temp);
                        }
                    }
                }
                else if (Bits == 24)
                {
                    while(raw.HasRemaining)
                    {
                        // Skip the least significant byte
                        raw.Position += 1;

                        // Read the two significant bytes
                        decoded.Write(raw.GetBytes(2));
                    }
                }

                buffer = decoded.ToArray();
            }
        }

        private void DecodeTo16Bit(byte[] buffer, out short[] samples)
        {
            List<short> result = new List<short>();
            using (BufferStream raw = new BufferStream(buffer))
            using (BufferStream decoded = new BufferStream())
            {
                // Convert 32bit WAV
                if (Bits == 32)
                {
                    if (_type == AudioFormat.PCM)
                    {
                        while (raw.HasRemaining)
                        {
                            int sample = raw.GetInt32();
                            short temp = (short)(sample >> 16);

                            result.Add(temp);
                        }
                    }
                    // Convert 32bit Float WAV
                    else if (_type == AudioFormat.Float)
                    {
                        while (raw.HasRemaining)
                        {
                            float sample = raw.GetFloat();
                            short temp = (short)(sample * 32767.0f);

                            result.Add(temp);
                        }
                    }
                }

                samples = result.ToArray();
            }
        }

        private void CopySoundStream(BufferStream dest)
        {
            int size = (Sound.Stream.CanSeek) ? Math.Min((int)(Sound.Stream.Length - Sound.Stream.Position), 0x2000) : 0x2000;
            byte[] buffer = new byte[size];
            int n;
            do
            {
                n = Sound.Stream.Read(buffer, 0, buffer.Length);
                dest.Write(buffer, 0, n);
            } while (n != 0);

            dest.Position = 0;
            Sound.Stream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        public new void Dispose()
        {
            base.Dispose();

            if (_closeStreamOnDispose)
                Sound.Stream.Dispose();
        }

    }
}
