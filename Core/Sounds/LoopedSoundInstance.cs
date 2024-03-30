using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;

namespace Luminance.Core.Sounds
{
    public class LoopedSoundInstance
    {
        protected readonly SoundStyle loopSoundStyle;

        /// <summary>
        ///     An automatic termination condition that governs whether the looping sound should terminate or not. Meant to serve as a fail-safe to ensure that looped sounds do not go on long after they should have stopped.
        ///     Useful for cases such as sounds that are attached to an entity but should go away.
        /// </summary>
        public Func<bool> AutomaticTerminationCondition
        {
            get;
            protected set;
        }

        /// <summary>
        ///     The sound slot that manages this looping sound.
        /// </summary>
        public SlotId LoopingSoundSlot
        {
            get;
            protected set;
        }

        /// <summary>
        ///     Whether the loop sound has been started yet or not.
        /// </summary>
        public bool HasLoopSoundBeenStarted
        {
            get;
            protected set;
        }

        /// <summary>
        ///     Whether this sound has been stopped or not.
        /// </summary>
        public bool HasBeenStopped
        {
            get;
            internal set;
        }

        /// <summary>
        ///     Whether the loop sound is being played.
        /// </summary>
        public bool LoopIsBeingPlayed => SoundEngine.TryGetActiveSound(LoopingSoundSlot, out _);

        // This constructor should not be used manually. Rather, sound instances should be created via the LoopedSoundManager's utilities, since that ensures that the sound is
        // properly logged by the manager.
        internal LoopedSoundInstance(SoundStyle loopingSound, Func<bool> automaticTerminationCondition)
        {
            loopSoundStyle = loopingSound;
            AutomaticTerminationCondition = automaticTerminationCondition;
            LoopingSoundSlot = SlotId.Invalid;
        }

        /// <summary>
        ///     Updates all active sounds.
        /// </summary>
        /// <param name="soundPosition">The moving source position of the sounds.</param>
        /// <param name="updateLoop">An optional update behavior that should be applied to the sounds.</param>
        protected virtual void UpdateSoundSlots(Vector2 soundPosition, Action<ActiveSound> updateLoop = null)
        {
            if (SoundEngine.TryGetActiveSound(LoopingSoundSlot, out ActiveSound s))
            {
                s.Position = soundPosition;
                updateLoop?.Invoke(s);
            }
            else if (!HasBeenStopped)
                HasLoopSoundBeenStarted = false;
        }

        /// <summary>
        ///     Starts all sounds.
        /// </summary>
        /// <param name="soundPosition">The source position of the sounds.</param>
        protected virtual void StartSounds(Vector2 soundPosition)
        {
            LoopingSoundSlot = SoundEngine.PlaySound(loopSoundStyle with
            {
                MaxInstances = 0,
                IsLooped = true
            }, soundPosition);
            HasLoopSoundBeenStarted = true;
        }

        /// <summary>
        ///     Handles stop behaviors for sounds, sans the state changes.
        /// </summary>
        protected virtual void StopSoundsInternal()
        {
            if (SoundEngine.TryGetActiveSound(LoopingSoundSlot, out ActiveSound s))
                s?.Stop();
        }

        /// <summary>
        ///     Performs all necessary update behaviors for sounds, evaluating whether they need to be started, updating sound positions in the world, and performing arbitrary update behaviors via <paramref name="soundUpdateStep"/>.
        /// </summary>
        /// <param name="soundPosition">The source position of the sounds.</param>
        /// <param name="soundUpdateStep">An optional update behavior that should be applied to the sounds.</param>
        public virtual void Update(Vector2 soundPosition, Action<ActiveSound> soundUpdateStep = null)
        {
            // Start the sound if it hasn't been activated yet.
            // If a starting sound should be used, play that first, and wait for it to end before playing the looping sound.
            if (!HasLoopSoundBeenStarted && !LoopIsBeingPlayed)
                StartSounds(soundPosition);

            // Keep the sound updated.
            UpdateSoundSlots(soundPosition, soundUpdateStep);
        }

        /// <summary>
        ///     Marks sounds as being eligible for restarting.
        /// </summary>
        public void Restart()
        {
            HasLoopSoundBeenStarted = false;
            HasBeenStopped = false;
        }

        /// <summary>
        ///     Stops all sounds.
        /// </summary>
        public void Stop()
        {
            // A sound cannot be stopped twice. If it was already stopped, do nothing.
            if (HasBeenStopped)
                return;

            StopSoundsInternal();

            // Mark this sound as having been stopped.
            HasBeenStopped = true;
        }
    }
}
