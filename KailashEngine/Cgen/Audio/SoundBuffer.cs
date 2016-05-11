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

    /// <summary>
    /// Represent an OpenAL Sound Buffer.
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        /// <summary>
        /// Enumeration for <see cref="SoundBuffer"/> States.
        /// </summary>
        public enum SoundState
        {
            /// <summary>
            /// State that used to indicate an Invalid / Unused <see cref="SoundBuffer"/>.
            /// </summary>
            None = -1,

            /// <summary>
            /// Default state of <see cref="SoundBuffer"/> when its loaded, 
            /// can be manually set when sound is rewind.
            /// </summary>
            Initial = 0x1011,

            /// <summary>
            /// Paused state.
            /// </summary>
            Paused = 0x1013,

            /// <summary>
            /// Playing state.
            /// </summary>
            Playing = 0x1012,

            /// <summary>
            /// Stopped state.
            /// </summary>
            Stopped = 0x1014
        }

        /// <summary>
        /// Enumeration for <see cref="SoundBuffer"/> Format.
        /// </summary>
        public enum SoundFormat
        {
            /// <summary>
            /// Unidentified Format.
            /// </summary>
            Unknown = -1,

            /// <summary>
            /// Wave Format.
            /// </summary>
            Wav = 0,

            /// <summary>
            /// Vorbis Format.
            /// </summary>
            Vorbis = 1
        }

        /// <summary>
        /// Default the number of queue buffer in a <see cref="SoundBuffer"/>.
        /// </summary>
        public const int DEFAULT_BUFFER_COUNT = 3;

        private int _sourceId = -1; // Snatch available source from SoundSystem later
        private int _filterId; // If available, use filter
        private int[] _bufferIds; // Multiple buffer id for stream big sound file

        private float _lowPassHfGain;

        private Stream _stream = null;
        private bool _isUnknownFormat = false;
        private SoundFormat _format = SoundFormat.Unknown;
        private bool _isDeferred = true;

        internal bool IsPreparing { get; private set; }
        internal bool IsReady { get; private set; }
        
        /// <summary>
        /// Gets the value indicating whether the buffer is disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Gets the value indicating whether the <see cref="SoundBuffer"/> is valid or not.
        /// Note that if <see cref="Deferred"/> property return true, The Buffer should be played at least once before its indicated as a valid <see cref="SoundBuffer"/>,
        /// </summary>
        public bool IsValid { get { return AL.IsSource(_sourceId); } }

        /// <summary>
        /// Gets the Sound Stream Reader of the <see cref="SoundBuffer"/>.
        /// </summary>
        public ISoundStreamReader Reader { get; set; }

        /// <summary>
        /// Gets the Native Sound (Source) handle of the <see cref="SoundBuffer"/>.
        /// </summary>
        public int Source { get { return _sourceId; } protected set { _sourceId = value; } }

        /// <summary>
        /// Gets the Native Buffers handle of the <see cref="SoundBuffer"/>.
        /// </summary>
        public int[] Buffers { get { return _bufferIds; } protected set { _bufferIds = value; } }

        /// <summary>
        /// Gets the underlying stream of the <see cref="SoundBuffer"/>.
        /// </summary>
        public Stream Stream { get { return _stream; } internal set { _stream = value; } }

        /// <summary>
        /// Gets the provided buffer format.
        /// </summary>
        public SoundFormat Format
        {
            get
            {
                return _format;
            }
            private set
            {
                _format = value;
            }
        }

        /// <summary>
        /// Gets or Sets whether the Buffer should be deferred or not.
        /// When <see cref="Deferred"/> property return true, the Buffer will only load after sounds is being played.
        /// </summary>
        public bool Deferred
        {
            get { return _isDeferred; }
            set { _isDeferred = value; }
        }

        /// <summary>
        /// Gets or Sets floating-point of LowPassGainHF of the <see cref="SoundBuffer"/>.
        /// This feature only available if OpenAL Efx extension is supported.
        /// 
        /// Note that the property may return vary value from each different instance
        /// but assign this property with certain value may apply to all existing <see cref="SoundBuffer"/> in most cases.
        /// </summary>
        public float LowPassHFGain
        {
            get { return _lowPassHfGain; }
            set
            {
                bool isValidSource = AL.IsSource(Source);
                ALChecker.CheckError();

                if (!isValidSource)
                    return;
                
                if (SoundSystem.Efx.IsInitialized)
                {
                    SoundSystem.Efx.Filter(_filterId, EfxFilterf.LowpassGainHF, _lowPassHfGain = value);
                    SoundSystem.Efx.BindFilterToSource(_sourceId, _filterId);
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Volume of the <see cref="SoundBuffer"/>.
        /// The value range is between 0 (Mute) and 100 (Full)
        /// </summary>
        public float Volume
        {
            get
            {
                float vol = 0;
                ALChecker.Check(() => AL.GetSource(_sourceId, ALSourcef.Gain, out vol));

                return vol * 100f;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_sourceId, ALSourcef.Gain, value * 0.01f));
            }
        }

        /// <summary>
        /// Gets or Sets the Pitch of the <see cref="SoundBuffer"/>.
        /// </summary>
        public float Pitch
        {
            get
            {
                float pitch = 0;
                ALChecker.Check(() => AL.GetSource(_sourceId, ALSourcef.Pitch, out pitch));

                return pitch;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_sourceId, ALSourcef.Pitch, value));
            }
        }

        /// <summary>
        /// Gets or Sets the Attenuation factor of the <see cref="SoundBuffer"/>.
        /// </summary>
        public float Attenuation
        {
            get
            {
                float attenuation = 0;
                ALChecker.Check(() => AL.GetSource(_sourceId, ALSourcef.RolloffFactor, out attenuation));

                return attenuation;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_sourceId, ALSourcef.RolloffFactor, value));
            }
        }

        /// <summary>
        /// Gets or Sets a value whether the <see cref="SoundBuffer"/> is relative to the listener.
        /// </summary>
        public bool IsRelativeToListener
        {
            get
            {
                bool isRelative = false;
                ALChecker.Check(() => AL.GetSource(_sourceId, ALSourceb.SourceRelative, out isRelative));

                return isRelative;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_sourceId, ALSourceb.SourceRelative, value));
            }
        }

        /// <summary>
        /// Gets or Sets the Minimum distance, the closest distance that can be heard in maximum volume of the <see cref="SoundBuffer"/>.
        /// A value of 0, which mean inside the listener is not allowed. Default value is 1.
        /// </summary>
        public float MinimumDistance
        {
            get
            {
                float minDistance = 1f;
                ALChecker.Check(() => AL.GetSource(_sourceId, ALSourcef.ReferenceDistance, out minDistance));

                return minDistance;
            }
            set
            {
                ALChecker.Check(() => AL.Source(_sourceId, ALSourcef.ReferenceDistance, value));
            }
        }

        /// <summary>
        /// Gets or Sets the 3D Position of the <see cref="SoundBuffer"/>.
        /// <value>
        /// The value is should be an array of float that has length 3, first index is for X Component, 2nd for Y and 3rd for Z.
        /// (e.g: sound.Position3D = new float[3] { 1, 2, 3 }; // X = 1, Y = 2, Z = 3)
        /// </value>
        /// </summary>
        /// 
        public float[] Position3D
        {
            get
            {
                float[] position = new float[3];
                AL.GetSource(_sourceId, ALSource3f.Position, out position[0], out position[1], out position[2]);

                return position;
            }
            set
            {
                AL.Source(_sourceId, ALSource3f.Position, value[0], value[1], value[2]);
            }
        }

        /// <summary>
        /// Gets or Sets Pan value of <see cref="SoundBuffer"/>.
        /// This property is simplified <see cref="Position3D"/> property for 2D plane.
        /// 
        /// Note that when assign this property with a certain value, it will override <see cref="Position3D"/> value.
        /// </summary>
        public float Pan
        {
            get
            {
                float[] position = new float[3];
                AL.GetSource(_sourceId, ALSource3f.Position, out position[0], out position[1], out position[2]);

                return position[0];
            }
            set
            {
                AL.Source(_sourceId, ALSource3f.Position, value, 0, 1);
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether the sound should loop.
        /// </summary>
        public bool IsLooping { get; set; }

        /// <summary>
        /// Gets the state of the <see cref="SoundBuffer"/>.
        /// </summary>
        public SoundState State
        {
            get
            {
                bool isValidSource = AL.IsSource(Source);
                ALChecker.CheckError();

                if (isValidSource)
                {
                    SoundState state = (SoundState)AL.GetSourceState(Source);
                    ALChecker.CheckError();

                    return state;
                }
                return SoundState.None;
            }
        }

        /// <summary>
        /// Gets or Sets the stream position of <see cref="SoundBuffer"/>.
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                if (Reader == null)
                    return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(Reader.DecodeTime);
            }
            set
            {
                if (Reader == null)
                    return;
                Reader.DecodeTime = (int)value.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Gets the length of <see cref="SoundBuffer"/>.
        /// </summary>
        public TimeSpan Length
        {
            get
            {
                if (Reader == null)
                    return TimeSpan.Zero;
                return TimeSpan.FromMilliseconds(Reader.LengthTime);
            }
        }

        /// <summary>
        /// Construct a new <see cref="SoundBuffer"/>.
        /// </summary>
        /// <param name="filename">Path of the sound file.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        public SoundBuffer(string filename, int bufferSize = DEFAULT_BUFFER_COUNT)
            : this(File.OpenRead(filename), DEFAULT_BUFFER_COUNT)
        {
        }

        /// <summary>
        /// Construct a new <see cref="SoundBuffer"/>.
        /// </summary>
        /// <param name="buffer">An array of byte that contains sound data.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        public SoundBuffer(byte[] buffer, int bufferSize = DEFAULT_BUFFER_COUNT)
            : this (new MemoryStream(buffer, true), bufferSize)
        {
        }

        /// <summary>
        /// Construct a new <see cref="SoundBuffer"/>.
        /// </summary>
        /// <param name="stream">Stream that contains sound buffer data.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        public SoundBuffer(Stream stream, int bufferSize = DEFAULT_BUFFER_COUNT)
        {
            // Generate handles
            _bufferIds = AL.GenBuffers(bufferSize);
            _stream = stream;

            // Use XRAM Extensions if available
            if (SoundSystem.XRam.IsInitialized)
                SoundSystem.XRam.SetBufferMode(bufferSize, ref _bufferIds[0], XRamExtension.XRamStorage.Hardware);

            // Use EFX Extensions if available
            if (SoundSystem.Efx.IsInitialized)
            {
                _filterId = SoundSystem.Efx.GenFilter();
                SoundSystem.Efx.Filter(_filterId, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
                SoundSystem.Efx.Filter(_filterId, EfxFilterf.LowpassGain, 1);
                LowPassHFGain = 1;
            }

            // TODO: Do we really do it here?
            // Identify the buffer format if its not known yet
            byte[] headerBytes = new byte[4];

            // Reset position and read the header
            _stream.Position = 0;
            _stream.Read(headerBytes, 0, headerBytes.Length);

            // Fetch header string
            string header = Encoding.UTF8.GetString(headerBytes);

            // Go back to zero position
            _stream.Position = 0;

            if (header == "OggS")
                Format = SoundFormat.Vorbis;
            else if (header == "RIFF")
                Format = SoundFormat.Wav;
            else
            {
                _isUnknownFormat = true;
                Format = SoundFormat.Unknown;
            }

            // Use approriate Sound Stream Reader
            if (Format == SoundFormat.Vorbis)
                Reader = new VorbisStreamReader(this, false);
            else if (Format == SoundFormat.Wav)
                Reader = new WaveStreamReader(this, false);
        }

        /// <summary>
        /// Initialize the Native Handle, De-queue existing buffer and (re)Queue the buffer of the <see cref="SoundBuffer"/>.
        /// </summary>
        protected void Initialize()
        {
            // Check whether the buffer is already initializing state
            if (IsPreparing)
                return;

            // Check whether the current Source is valid
            bool isValidSource = AL.IsSource(Source);
            ALChecker.CheckError();
            if (!isValidSource || Source == -1)
                // Get unused Source from SoundSystem
                Source = SoundSystem.Instance.GetAvailableSource();

            // We don't have clue which buffer id that being attached to the source right now
            // So we don't attach (queue) the buffer to the source now
            // We will attach (queue) it when we are (really) going to queue the buffer (read: Play())
            
            // Initialize() call will dequeue current buffer and queue the buffer when its required

            // Check the sound state
            switch (State)
            {
                // Sound is already playing, no need preparation(?)
                case SoundState.Paused:
                case SoundState.Playing:
                    return;

                // The sounds is already stopped (which mean its been played once or more before)
                case SoundState.Stopped:
                    // Reset to 0 Position
                    Reader.DecodeTime = 0;
                    // Dequeue remaining buffer before we (re)queue the buffer
                    DequeueBuffer();
                    // We are not ready to start yet, we haven't queue the buffer
                    IsReady = false;

                    break;
            }

            // Check whether our buffer is ready
            if (!IsReady)
            {
                // If its not ready, lets queue them
                IsPreparing = true; // set the flag to prevent requeue during process
                QueueBuffer(precache: true);
                IsPreparing = false; // done
            }
        }

        /// <summary>
        /// Queue the Buffer Data.
        /// </summary>
        /// <param name="reader">Specify the <see cref="ISoundStreamReader"/>, be sure that the stream reader is able to handle the provided buffer.</param>
        /// <param name="precache">Specify whether the buffer should be pre-cached.</param>
        protected void QueueBuffer(ISoundStreamReader reader, bool precache = false)
        {
            // Reset position of the Stream
            Stream.Seek(0, SeekOrigin.Begin);

            // Use specified reader
            Reader = reader;

            if (precache)
            {
                // Fill first buffer synchronously
                Reader.BufferData(SoundSystem.Instance.BufferSize, Buffers[0]);

                // Here we attach the Source to the Buffer
                ALChecker.Check(() => AL.SourceQueueBuffer(Source, Buffers[0]));

                // Schedule the others buffer asynchronously as the game update
                if (Deferred)
                    SoundSystem.Instance.Add(this);
                else
                    SoundSystem.Instance.AddUndeferredSource(Source);
            }

            IsReady = true;
        }

        /// <summary>
        /// Queue the Buffer Data.
        /// </summary>
        /// <param name="precache">Specify whether the buffer should be pre-cached.</param>
        protected void QueueBuffer(bool precache = false)
        {
            // Reset position of the Stream
            Stream.Seek(0, SeekOrigin.Begin);

            // Use approriate Sound Stream Reader
            if (Reader == null)
            {
                if (Format == SoundFormat.Vorbis)
                    Reader = new VorbisStreamReader(this, false);
                else if (Format == SoundFormat.Wav)
                    Reader = new WaveStreamReader(this, false);
                else if (Format == SoundFormat.Unknown)
                    // You need to implement your own Sound Stream Reader
                    // Inherit ISoundStreamReader and pass it via QueueBuffer(ISoundStreamReader, bool)
                    // Or set the implementation under SoundBuffer.Reader property
                    throw new NotSupportedException("Unknown sound data buffer format.");
            }

            if (precache)
            {
                // Fill first buffer synchronously
                Reader.BufferData(SoundSystem.Instance.BufferSize, Buffers[0]);

                // Here we attach the Source to the Buffer
                ALChecker.Check(() => AL.SourceQueueBuffer(Source, Buffers[0]));

                // Schedule the others buffer asynchronously as the game update
                if (Deferred)
                    SoundSystem.Instance.Add(this);
                else
                    SoundSystem.Instance.AddUndeferredSource(Source);
            }

            IsReady = true;
        }

        /// <summary>
        /// De-queue the Buffer Data.
        /// </summary>
        protected void DequeueBuffer()
        {
            // Get how many buffers that already queued
            int queued = -1;
            ALChecker.Check(() => AL.GetSource(Source, ALGetSourcei.BuffersQueued, out queued));

            // Unqueue the buffer (if any)
            if (queued > 0)
            {
                try
                {
                    // Unqueue it
                    ALChecker.Check(() => AL.SourceUnqueueBuffers(Source, queued));
                }
                catch (InvalidOperationException)
                {
                    // This is a bug in the OpenAL implementation
                    // Salvage what we can
                    int processed = -1;
                    ALChecker.Check(() => AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out processed));
                    int[] salvaged = new int[processed];

                    // Try to unqueue again
                    if (processed > 0)
                        ALChecker.Check(() => AL.SourceUnqueueBuffers(_sourceId, processed, salvaged));

                    // Try to stopping the source (again?)
                    ALChecker.Check(() => AL.SourceStop(Source));

                    // Once again, dequeue it
                    DequeueBuffer();
                }
            }
        }

        internal virtual void OnFinish()
        {

        }

        /// <summary>
        /// Dispose <see cref="SoundBuffer"/> Handle.
        /// </summary>
        public void Dispose()
        {
            // Stop playing
            if (State == SoundState.Playing || State == SoundState.Paused)
                ALChecker.Check(() => AL.SourceStop(Source));

            // Remove from queue update
            SoundSystem.Instance.Remove(this);

            // Dequeue the queued buffers
            if (State != SoundState.Initial)
                DequeueBuffer();

            // Dispose the Sound Stream Reader
            if (Reader != null)
                Reader.Dispose();

            // It's safe to dispose the Source here (and it will be 'recycled' to later use)
            ALChecker.Check(() => AL.DeleteSource(Source));

            // Delete all buffer
            ALChecker.Check(() => AL.DeleteBuffers(Buffers));

            // Delete filter extension (if supported)
            if (SoundSystem.Efx.IsInitialized)
                SoundSystem.Efx.DeleteFilter(_filterId);

            IsDisposed = true;
        }
    }
}
