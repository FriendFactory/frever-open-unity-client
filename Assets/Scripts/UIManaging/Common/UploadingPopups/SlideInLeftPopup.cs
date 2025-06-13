using System.Collections;
using BrunoMikoski.AnimationSequencer;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;

namespace UIManaging.Common.UploadingPopups
{
    internal abstract class SlideInLeftPopup<T> : BasePopup<T> where T : PopupConfiguration
    {
        [SerializeField] private GameObject _doneImage;
        [Header("Animations")] 
        [SerializeField] private float _hideDelay = 0.5f;
        [SerializeField] private AnimationSequencerController _animationSequencer;

        public override void Show()
        {
            base.Show();
            SetDoneImageActiveState(false);
            
            _animationSequencer.Rewind();
            _animationSequencer.PlayForward();
        }

        public override void Hide()
        {
            StartCoroutine(DelayedHideRoutine());
            SetDoneImageActiveState(true);
        }

        protected IEnumerator DelayedHideRoutine()
        {
            yield return new WaitForSeconds(_hideDelay);
            _animationSequencer.PlayBackwards(onCompleteCallback: () => base.Hide(null));
        }
        
        protected void SetDoneImageActiveState(bool isOn)
        {
            _doneImage.SetActive(isOn);
        }
    }
}