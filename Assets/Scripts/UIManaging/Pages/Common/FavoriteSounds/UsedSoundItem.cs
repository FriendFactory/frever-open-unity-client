using System;
using Bridge.Models.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public abstract class FeedSoundItemBase<TModel>: SoundItemBase<TModel> where TModel : UsedSoundItemModel 
    {
        [SerializeField] private Button _openSoundPageButton;

        public event Action<TModel> OpenSoundPageRequested;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _openSoundPageButton.onClick.AddListener(OnOpenSoundPageButtonClicked);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            _openSoundPageButton.onClick.RemoveListener(OnOpenSoundPageButtonClicked);
        }

        private void OnOpenSoundPageButtonClicked()
        {
            OpenSoundPageRequested?.Invoke(ContextData);
        }
    }
}