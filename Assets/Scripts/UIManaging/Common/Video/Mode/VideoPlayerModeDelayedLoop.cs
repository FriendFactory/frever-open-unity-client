using System.Collections;
using Common;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoPlayerModeDelayedLoop : VideoPlayerPlaybackMode
    {
        private const float DELAY_SECONDS = 1f;
        private static readonly WaitForSeconds WAIT_FOR_DELAY = new WaitForSeconds(DELAY_SECONDS);

        private Coroutine _coroutine;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void StartPlayback()
        {
            StopPlayback();

            MediaPlayer.Loop = false;
            MediaPlayer.Events.AddListener(OnEventInvoked);

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

        private void OnEventInvoked(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
            {
                _coroutine = CoroutineSource.Instance.StartCoroutine(StartPlaybackWithDelay());
            }
        }

        private IEnumerator StartPlaybackWithDelay()
        {
            yield return WAIT_FOR_DELAY;

            if (MediaPlayer.gameObject.activeInHierarchy)
            {
                base.StartPlayback();
            }
        }
    }
}