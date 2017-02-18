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
    using System.Threading;
    using System.IO;

    using Cgen;
    using Cgen.Entities;
    using Cgen.Utilities;
    using Cgen.Internal.OpenAL;

    using OpenTK;
    using OpenTK.Audio;
    using OpenTK.Audio.OpenAL;

    /// <summary>
    /// Provides common Sound operations.
    /// </summary>
    public class SoundSystem : IUpdatable
    {
        /// <summary>
        /// Default Buffer Size per <see cref="SoundBuffer"/>.
        /// </summary>
        internal const int DEFAULT_BUFFER_SIZE = 44100;

        /// <summary>
        /// Default Update Rate.
        /// </summary>
        internal const float DEFAULT_UPDATE_RATE = 10;

        /// <summary>
        /// Maximum Number of Source per Context.
        /// </summary>
        internal const int MAXIMUM_NUMBER_OF_SOURCES = 64;

        private static SoundSystem _instance;
        private static object _singletonMutex = new object();
        private static ALContext _context;
        private static List<int> _unDeferredSources = new List<int>();
        private volatile bool _isStarted = false;
        private System.Threading.Thread _automatedUpdateThread;

        /// <summary>
        /// Gets the XRAM Extension.
        /// </summary>
        internal static XRamExtension XRam { get; private set; }

        /// <summary>
        /// Gets the EFX Extension.
        /// </summary>
        internal static EffectsExtension Efx { get; private set; }

        /// <summary>
        /// Gets the Singleton Instance of the <see cref="SoundSystem"/>.
        /// </summary>
        public static SoundSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_singletonMutex)
                    {
                        if (_instance == null)
                        {
                            _instance = new SoundSystem();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets the allowed amount of Buffer Size.
        /// </summary>
        public int BufferSize { get; set; }

        internal bool IsUndeferredAvailable
        {
            get { return _unDeferredSources.Count < MAXIMUM_NUMBER_OF_SOURCES; }
        } 

        private object _iterationMutex = new object();
        private List<SoundBuffer> _sounds = new List<SoundBuffer>();
        private int[] _sources = new int[64];

        /// <summary>
        /// Construct a new <see cref="SoundSystem"/>.
        /// </summary>
        public SoundSystem()
        {
        }

        /// <summary>
        /// Initialize the <see cref="SoundSystem"/>.
        /// </summary>
        /// <param name="bufferSize"></param>
        public void Initialize(int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            if (_context == null)
                _context = new ALContext();

            BufferSize = bufferSize;

            XRam = new XRamExtension();
            Efx = new EffectsExtension();

            // Init empty sources
            _sources = AL.GenSources(MAXIMUM_NUMBER_OF_SOURCES);
            ALChecker.CheckError();
        }

        /// <summary>
        /// Start <see cref="SoundSystem"/> engine.
        /// Automatically update the engine under separate <see cref="Thread"/>.
        /// <para>After calling <see cref="SoundSystem.Start"/>, you don't have to call <see cref="SoundSystem.Update(double)"/> manually.</para>
        /// </summary>
        public void Start()
        {
            _automatedUpdateThread =
                new Thread(() => AutomatedUpdate());
            _isStarted = true;
            _automatedUpdateThread.Start();
        }

        /// <summary>
        /// Stop <see cref="SoundSystem"/> engine.
        /// Stop automatically update the engine and terminate the automated update <see cref="Thread"/>.
        /// 
        /// <para>If there are the sounds still playing, you need to update the engine manually by calling <see cref="SoundSystem.Update(double)"/>.</para>
        /// </summary>
        public void Stop()
        {
            _isStarted = false;
        }

        private void AutomatedUpdate()
        {
            while(_isStarted)
            {
                Update(0);
                Thread.Sleep((int)DEFAULT_UPDATE_RATE);
            }
        }

        internal void Add(SoundBuffer stream)
        {
            _sounds.Add(stream);
        }

        internal void AddUndeferredSource(int source)
        {
            if (_unDeferredSources.Count >= MAXIMUM_NUMBER_OF_SOURCES)
            {
                throw new OutOfMemoryException("The number of un-deferred sounds is exceeded. " +
                    "Maximum number of sources per instance: " + MAXIMUM_NUMBER_OF_SOURCES.ToString());
            }
            _unDeferredSources.Add(source);
        }

        internal void Remove(SoundBuffer stream)
        {
            _sounds.Remove(stream);
        }

        internal void RemoveUndeferredSource(int source)
        {
            _unDeferredSources.Remove(source);
        }

        internal int GetAvailableSource()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                bool isValidSource = AL.IsSource(_sources[i]);
                ALChecker.CheckError();

                if (isValidSource)
                {
                    // Some sources are probably used by another sounds
                    // Return the one that not in use

                    // Identify source state
                    ALSourceState state = AL.GetSourceState(_sources[i]);
                    ALChecker.CheckError();

                    // Do not use un-deferred source
                    if (_unDeferredSources.Contains(_sources[i]) && state == ALSourceState.Initial)
                        continue;
                    else if (_unDeferredSources.Contains(_sources[i]) && state == ALSourceState.Stopped)
                        _unDeferredSources.Remove(_sources[i]);

                    // No sounds using it or no longer in use, use it
                    if (state == ALSourceState.Initial || (state == ALSourceState.Stopped))
                        return _sources[i];
                    // Source is in use by a sound, find another one
                    else if (state == ALSourceState.Paused || state == ALSourceState.Playing)
                        continue;
                }
                else
                {
                    // Not a source (anymore..?)
                    // Generate and use it
                    _sources[i] = AL.GenSource();

                    // Since it's newly generated, it must be unused source
                    return _sources[i];
                }
            }

            // All sources are used at the moment...
            // Return invalid source
            return -1;
        }

        /// <summary>
        /// Update the <see cref="SoundSystem"/>.
        /// </summary>
        /// <param name="delta"></param>
        public void Update(double delta)
        {
            // Copy to a new list
            List<SoundBuffer> soundStreams = new List<SoundBuffer>(_sounds);

            // Loop it
            foreach (var sound in soundStreams)
            {
                // The sounds is no longer exist, skip it
                if (!_sounds.Contains(sound))
                    continue;

                bool finished = false;

                // Get how many queued buffers
                int queued = -1;
                ALChecker.Check(() => AL.GetSource(sound.Source, ALGetSourcei.BuffersQueued, out queued));

                // Get how many processed buffers
                int processed = -1;
                ALChecker.Check(() => AL.GetSource(sound.Source, ALGetSourcei.BuffersProcessed, out processed));

                // Every queued buffers are processed, skip it
                if (processed == 0 && queued == sound.Buffers.Length)
                    continue;

                // Unqueue the processed buffers
                int[] tempBuffers;
                if (processed > 0)
                {
                    tempBuffers = AL.SourceUnqueueBuffers(sound.Source, processed);
                    ALChecker.CheckError();
                }
                else
                {
                    // Get remaining queued buffers
                    List<int> buffers = new List<int>();
                    for (int i = queued; i < sound.Buffers.Length; i++)
                        buffers.Add(sound.Buffers[i]);

                    //tempBuffers = sound.alBufferIds.Skip(queued).ToArray();
                    tempBuffers = buffers.ToArray();
                }

                // Loop for each queued buffers
                for (int i = 0; i < tempBuffers.Length; i++)
                {
                    // Upload buffer data of samples and check whether its already processed
                    finished |= sound.Reader.BufferData(BufferSize, tempBuffers[i]);

                    // Buffer data has been processed
                    if (finished)
                    {
                        // Check whether the sounds is looping
                        // Reset position of DecodeTime to zero
                        // Since Looping via AL command is not supported on stream sounds.
                        if (sound.IsLooping)
                            sound.Reader.DecodeTime = 0;
                        else
                        {
                            // No repeat, then safely remove the sounds from the queue list
                            // Triger the event first before remove
                            sound.OnFinish();

                            _sounds.Remove(sound);
                            i = tempBuffers.Length;
                        }
                    }
                }

                ALChecker.Check(() => AL.SourceQueueBuffers(sound.Source, tempBuffers.Length, tempBuffers));

                if (finished && !sound.IsLooping)
                    continue;

                if (sound.IsPreparing)
                    continue;

                if (!_sounds.Contains(sound))
                    continue;

                var state = AL.GetSourceState(sound.Source);
                ALChecker.CheckError();
                if (state == ALSourceState.Stopped)
                {
                    ALChecker.Check(() => AL.SourcePlay(sound.Source));
                }

            }
        }

        /// <summary>
        /// Update the <see cref="SoundSystem"/>.
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="position"></param>
        /// <param name="look"></param>
        /// <param name="up"></param>
        public void Update(double delta, Vector3 position, Vector3 look, Vector3 up)
        {
            ALChecker.Check(() => AL.Listener(ALListener3f.Position, ref position));
            ALChecker.Check(() => AL.Listener(ALListenerfv.Orientation, ref look, ref up));
            Update(delta);
        }

    }
}
