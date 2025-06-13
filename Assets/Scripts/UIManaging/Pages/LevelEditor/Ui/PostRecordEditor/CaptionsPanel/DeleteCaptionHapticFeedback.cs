using System.Collections;
using UIManaging.Common.Buttons;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class DeleteCaptionHapticFeedback: EventBasedHapticFeedback<DeleteCaptionArea>
    {
        [SerializeField] private float _period = 0.67f;
        
        private Coroutine _continuousFeedbackRoutine;
        private WaitForSeconds _waitPeriod;

        protected override void Awake()
        {
            base.Awake();

            _waitPeriod = new WaitForSeconds(_period);
        }

        protected override void Subscribe()
        {
            Source.CaptionEntered += PlayContinuous;
            Source.CaptionExited += Stop;
        }

        protected override void Unsubscribe()
        {
            Source.CaptionEntered -= PlayContinuous;
            Source.CaptionExited -= Stop;
        }

        private void PlayContinuous()
        {
            Stop();
            
            _continuousFeedbackRoutine = StartCoroutine(PlayContinuousHapticFeedback());
        }
        
        private void Stop()
        {
            if (_continuousFeedbackRoutine == null) return;
            
            StopCoroutine(_continuousFeedbackRoutine);
            _continuousFeedbackRoutine = null;
        }

        private IEnumerator PlayContinuousHapticFeedback()
        {
            while (true)
            {
                PlayFeedback();

                yield return _waitPeriod;
            }
        }
    }
}