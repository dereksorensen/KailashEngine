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
    /// Represent a Sound Object.
    /// </summary>
    public class Sound : SoundBuffer
    {
        /// <summary>
        /// Represents the method that will handles the <see cref="Sound.SoundStarted"/>,
        /// <see cref="Sound.SoundPaused"/>, <see cref="Sound.SoundResumed"/>
        /// and <see cref="Sound.SoundStopped"/> events of a <see cref="Sound"/>.
        /// </summary>
        public delegate void SoundEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Invoked when the sound is start playing.
        /// </summary>
        public event SoundEventHandler SoundStarted;

        /// <summary>
        /// Invoked when the sound is paused.
        /// </summary>
        public event SoundEventHandler SoundPaused;

        /// <summary>
        /// Invoked when the sound is resumed.
        /// </summary>
        public event SoundEventHandler SoundResumed;

        /// <summary>
        /// Invoked when the sound is stop playing
        /// </summary>
        public event SoundEventHandler SoundStopped;

        /// <summary>
        /// Construct a new <see cref="Sound"/>.
        /// </summary>
        /// <param name="filename">Path of the sound file.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        /// <param name="deferred">Value indicating whether the <see cref="Sound"/> should use deferred buffering.</param>
        public Sound(string filename, bool deferred = true, int bufferSize = DEFAULT_BUFFER_COUNT)
            : this(File.OpenRead(filename), deferred, DEFAULT_BUFFER_COUNT)
        {
        }

        /// <summary>
        /// Construct a new <see cref="Sound"/>.
        /// </summary>
        /// <param name="buffer">An array of byte that contains sound data.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        /// <param name="deferred">Value indicating whether the <see cref="Sound"/> should use deferred buffering.</param>
        public Sound(byte[] buffer, bool deferred = true, int bufferSize = DEFAULT_BUFFER_COUNT)
            : this (new MemoryStream(buffer, true), deferred, bufferSize)
        {
        }

        /// <summary>
        /// Construct a new <see cref="Sound"/>.
        /// </summary>
        /// <param name="stream">Stream that contains sound buffer data.</param>
        /// <param name="bufferSize">The sound buffer size.</param>
        /// <param name="deferred">Value indicating whether the <see cref="Sound"/> should use deferred buffering.</param>
        public Sound(Stream stream, bool deferred = true, int bufferSize = DEFAULT_BUFFER_COUNT)
            : base(stream, bufferSize)
        {
            Deferred = deferred;

            if (!Deferred)
                Initialize();
        }

        /// <summary>
        /// Play the <see cref="Sound"/>.
        /// When the <see cref="Sound"/> is provided with valid buffer data, 
        /// <see cref="SoundBuffer.IsValid"/> property will return true.
        /// </summary>
        public void Play()
        {
            // Just resume if the sounds is paused
            if (State == SoundState.Paused)
            {
                Resume();
                return;
            }

            // Make sure the sound is well-queued
            if (Deferred || !IsReady)
                Initialize();
            else
                SoundSystem.Instance.Add(this);

            // Let's rock!
            ALChecker.Check(() => AL.SourcePlay(Source));

            // Triger the event
            if (SoundStarted != null)
                SoundStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Pause the <see cref="Sound"/>.
        /// </summary>
        public void Pause()
        {
            // This may cause OpenAL error if the Source is not valid.
            if (State != SoundState.Paused)
            {
                ALChecker.Check(() => AL.SourcePause(Source));

                if (SoundPaused != null)
                    SoundPaused(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Resume the <see cref="Sound"/>.
        /// </summary>
        public void Resume()
        {
            // This may cause OpenAL error if the Source is not valid.
            if (State == SoundState.Paused)
            {
                ALChecker.Check(() => AL.SourcePlay(Source));

                if (SoundResumed != null)
                    SoundResumed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Stop the <see cref="Sound"/>.
        /// </summary>
        public void Stop()
        {
            // This may cause OpenAL error if the Source is not valid.
            ALChecker.Check(() => AL.SourceStop(Source));
            Reader.DecodeTime = 0;

            if (!Deferred)
            {
                SoundSystem.Instance.RemoveUndeferredSource(Source);
                Deferred = true;
            }

            if (SoundStopped != null)
                SoundStopped(this, EventArgs.Empty);

            // In this case, we really need to check this first
            // Whether the Source is valid
            bool isValidSource = AL.IsSource(Source);
            ALChecker.CheckError();
            if (isValidSource)
            {
                // TODO: Should we really need to dispose the source here?
                ALChecker.Check(() => AL.DeleteSource(Source));
                Source = -1; // Set to invalid Source to make the next cycle source available
            }
        }

        internal override void OnFinish()
        {
            base.OnFinish();

            if (SoundStopped != null)
                SoundStopped(this, EventArgs.Empty);
        }
    }
}
