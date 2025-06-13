using System;
using UI.UIAnimators;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public sealed class RemixOptionsPopupConfiguration: PopupConfiguration
    {
        public Action OnSimpleRemixChosen;
        public Action OnShuffledRemixChosen;
        public Action OnCancelled;

        public RemixOptionsPopupConfiguration(): base(PopupType.RemixOptions, null)
        {
        }

        public RemixOptionsPopupConfiguration(Action onSimpleRemixChosen, Action onShuffledRemixChosen, Action onCancelled): base(PopupType.RemixOptions, null)
        {
            OnSimpleRemixChosen = onSimpleRemixChosen;
            OnShuffledRemixChosen = onShuffledRemixChosen;
            OnCancelled = onCancelled;
        }
    }
    
    internal sealed class RemixOptionsPopup: BasePopup<RemixOptionsPopupConfiguration>
    {
        [SerializeField] private Button _simpleRemixButton;
        [SerializeField] private Button _shuffledRemixButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private BaseUiAnimationPlayer _animationPlayer;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _simpleRemixButton.onClick.AddListener(()=>
            {
                Hide();
                Configs.OnSimpleRemixChosen?.Invoke();
            });
            _shuffledRemixButton.onClick.AddListener(()=>
            {
                Hide();
                Configs.OnShuffledRemixChosen?.Invoke();
            });
            
            _cancelButton.onClick.AddListener(() =>
            {
                Hide();
                Configs.OnCancelled?.Invoke();
            });
        }
        
        private void OnEnable()
        {
            _animationPlayer.PlayHideAnimationInstant();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Show()
        {
            base.Show();
            _animationPlayer.PlayShowAnimation();
        }

        public override void Hide()
        {
            _animationPlayer.PlayHideAnimation(base.Hide);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnConfigure(RemixOptionsPopupConfiguration configuration)
        {
        }
    }
}