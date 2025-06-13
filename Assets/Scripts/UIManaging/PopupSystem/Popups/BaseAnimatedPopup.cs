using System;
using System.Threading.Tasks;
using BrunoMikoski.AnimationSequencer;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class BaseAnimatedPopup<TConfiguration>: BasePopup<TConfiguration> where TConfiguration : PopupConfiguration
    {
        [Header("Animation")]
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private float _fadeInDelay;
        [SerializeField] private float _fadeOutDelay;

        protected virtual void OnDestroy()
        {
            _animationSequencer.ResetToInitialState();
            _animationSequencer.Kill();
        }

        public override async void Show()
        {
            await Task.Delay(TimeSpan.FromSeconds(_fadeInDelay));
            
            base.Show();
            
            _animationSequencer.PlayForward(true, OnShown);
        }

        public override async void Hide(object result)
        {
            await Task.Delay(TimeSpan.FromSeconds(_fadeOutDelay));
            
            _animationSequencer.PlayBackwards(true, () => base.Hide(result));
        }

        protected abstract override void OnConfigure(TConfiguration configuration);

        protected virtual void OnShown() { }
    }
}