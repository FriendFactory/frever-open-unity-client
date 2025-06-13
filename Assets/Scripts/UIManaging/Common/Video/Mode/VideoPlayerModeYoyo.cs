using System.Collections;
using Common;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoPlayerModeYoyo : VideoPlayerPlaybackMode
    {
        private static readonly WaitForSeconds WAIT_FOR_ONE_SECOND = new WaitForSeconds(1);

        private Coroutine _coroutine;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void StartPlayback()
        {
            StopPlayback();
            _coroutine = CoroutineSource.Instance.StartCoroutine(BouncePlayback());
            base.StartPlayback();
        }
        
        public override void StopPlayback()
        {
            if (_coroutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            base.StopPlayback();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private IEnumerator BouncePlayback()
        {
            while (true)
            {
                yield return WAIT_FOR_ONE_SECOND;
                
                if (MediaPlayer.gameObject.activeInHierarchy)
                {
                    MediaPlayer.PlaybackRate *= -1;
                }
            }
        }
    }
}