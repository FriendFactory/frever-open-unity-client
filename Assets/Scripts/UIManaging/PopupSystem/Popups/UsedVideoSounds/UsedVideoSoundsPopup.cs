using Bridge.Models.Common;
using Navigation.Core;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.FavoriteSounds;
using UIManaging.Pages.FavoriteSounds;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.UsedVideoSounds
{
    public sealed class UsedVideoSoundsPopup: BasePopup<UsedVideoSoundsPopupConfiguration>
    {
        [SerializeField] private Button _outsideButton;
        [SerializeField] private SlideInOutBehaviour _slideInOut;
        [SerializeField] private UsedVideoSoundsList _soundsList;
        
        [Inject] private PageManager _pageManager;

        private void OnEnable()
        {
            _outsideButton.onClick.AddListener(Hide);
        }
        
        private void OnDisable()
        {
            _outsideButton.onClick.RemoveListener(Hide);
        }

        protected override void OnConfigure(UsedVideoSoundsPopupConfiguration configuration)
        {
            _soundsList.Initialize(configuration.SoundsListModel);

            _soundsList.OnCellViewInstantiatedEvent += OnItemInstantiated;
            
            _slideInOut.Show();
        }

        protected override void OnHidden()
        {
            _soundsList.CleanUp();
            
            _soundsList.OnCellViewInstantiatedEvent -= OnItemInstantiated;
        }

        private void OnItemInstantiated(UsedSoundItem item)
        {
            item.OpenSoundPageRequested += OnOpenSoundPageRequested;
        }

        private void OnOpenSoundPageRequested(UsedSoundItemModel itemModel)
        {
            // no need to play slide out animation
            base.Hide();
            
            _pageManager.MoveNext(new VideosBasedOnSoundPageArgs(itemModel));
        }

        public override void Hide()
        {
            _slideInOut.SlideOut(() => base.Hide(null));
        }
    }

}