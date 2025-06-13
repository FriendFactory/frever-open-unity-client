using System;
using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Modules.Sound
{
    internal sealed class SoundController 
    {
        private AudioSource _source;

        private Coroutine _waitRoutine;
        public event Action<SoundController> Finished;
        public event Action<SoundController> Used;

        public SoundController(AudioSource source)
        {
            _source = source;
        }
        
        public void Play(AudioClip clip, AudioMixerGroup mixerGroup)
        {
            if (clip == null) throw new ArgumentException("Provided clip is null");

            _source.outputAudioMixerGroup = mixerGroup;
            _source.clip = clip;
            _source.Play();
            
            NotifyAboutPlaybackFinished();
        }

        public void MarkAsUsed()
        {
            _source.Stop();
            Used?.Invoke(this);
        }

        public void Destroy()
        {
            if (_waitRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(NotifyAboutPlaybackFinishedRoutine());
            }
            
            Finished = null;
            Object.Destroy(_source);
        }
        
        private void NotifyAboutPlaybackFinished()
        {
            if (_waitRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(NotifyAboutPlaybackFinishedRoutine());
            }
            
            _waitRoutine = CoroutineSource.Instance.StartCoroutine(NotifyAboutPlaybackFinishedRoutine());
        }

        private IEnumerator NotifyAboutPlaybackFinishedRoutine()
        {
            yield return new WaitWhile(() => _source.isPlaying); 
            Finished?.Invoke(this);
            _waitRoutine = null;
        }
    }
}