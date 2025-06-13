using System;
using System.Collections;
using Common;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets.AssetDependencies;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Common
{
    [UsedImplicitly]
    public class SongPlayer
    {
        private readonly AudioSource _audioSource;
        
        private Coroutine _playingRoutine;

        public SongPlayer(AudioSourceManager audioSourceManager)
        {
            _audioSource = audioSourceManager.SongAudioSource;
        }
        
        public void PlayForSeconds(AudioClip audioClip, float activationCue, Action onStop, float endCue = 15f, bool loop = false)
        {
            var startCue = Mathf.Clamp(activationCue, 0, audioClip.length);
            _audioSource.clip = audioClip;
            _audioSource.loop = loop;
            _playingRoutine = CoroutineSource.Instance.StartCoroutine(PlayForTime(startCue, endCue, onStop, loop));
        }

        public void Stop()
        {
            if (!_audioSource) return;
            
            _audioSource.Stop();
            
            if(_playingRoutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_playingRoutine);
            }
        }
        
        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public float GetCurrentTime()
        {
            return _audioSource.time;
        }

        private IEnumerator PlayForTime(float startCue, float endCue, Action onStop, bool loop = false)
        {
            do
            {
                _audioSource.time = startCue;
                _audioSource.Play();
                yield return new WaitForSecondsRealtime(endCue);
                _audioSource.Stop();
            } while (loop);

            onStop?.Invoke();
        }
    }
}
