using Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Common.TimeManaging
{
    /// <summary>
    ///     Provides audio time with shifted value for face animation synchronisation with
    ///     new audio activation cue. In another words, it adapts audio time curve to face animation time curve 
    /// </summary>
    public interface IAudioBasedTimeSource: ITimeSource
    {
        void SetupAudioSource(AudioSource audioSource);
        void SetAudioTimeLineShift(float seconds);
    }
    
    [UsedImplicitly]
    internal sealed class AudioTimeSource : IAudioBasedTimeSource
    {
        private AudioSource _audioSource;
        private float _audioTimeLineShift;

        private AudioClip Clip => _audioSource.clip;
        public long ElapsedMs => ElapsedSeconds.ToMilliseconds();

        public float ElapsedSeconds => _audioSource.timeSamples / (Clip != null ? Clip.frequency : 44000f) -
                                       _audioTimeLineShift;

        public void SetupAudioSource(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }

        public void SetAudioTimeLineShift(float seconds)
        {
            _audioTimeLineShift = seconds;
        }
    }
}