using System;
using System.Collections;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.AvatarPreview.Ui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage
{
    internal sealed class OnboardingCharacterResultPage : GenericPage<OnboardingCharacterResultArgs>
    {
        private const float DELAY_TIME = 3f;
        
        [SerializeField] private AvatarDisplayView _avatarDisplayView;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private ParticleSystem _confettiVfx;
        [SerializeField] private Sprite _disabledConfirmSprite;
        [SerializeField] private Sprite _enabledConfirmSprite;

        [Inject] private AvatarDisplayCharacterModel.Factory _avatarDisplayModelFactory;
        
        private AvatarDisplayCharacterModel _avatarDisplayModel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.OnboardingCharacterResult;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {    
            _confirmButton.onClick.AddListener(Confirm);
        }

        protected override void OnDisplayStart(OnboardingCharacterResultArgs args)
        {
            _confirmButton.interactable = false;
            _confirmButton.image.sprite = _disabledConfirmSprite;
            
            base.OnDisplayStart(args);
            
            _avatarDisplayModel = _avatarDisplayModelFactory.Create(new AvatarDisplayCharacterModel.Args
            {
                Character = OpenPageArgs.Character,
                Outfit = OpenPageArgs.Outfit
            });
            _avatarDisplayModel.AvatarReady += OnAvatarReady;
            
            _avatarDisplayView.Initialize(_avatarDisplayModel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _avatarDisplayModel.AvatarReady -= OnAvatarReady;
            _avatarDisplayModel.CloseRoom();
            _avatarDisplayView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnAvatarReady()
        {
            _confettiVfx.Play();

            StartCoroutine(DelayedConfirmEnableCoroutine(DELAY_TIME));
        }

        private IEnumerator DelayedConfirmEnableCoroutine(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            
            _confirmButton.interactable = true;
            _confirmButton.image.sprite = _enabledConfirmSprite;
            
            OpenPageArgs.OnDelayComplete?.Invoke();
        }

        private void Confirm()
        {
            OpenPageArgs.OnConfirm?.Invoke();
        }
    }
}
