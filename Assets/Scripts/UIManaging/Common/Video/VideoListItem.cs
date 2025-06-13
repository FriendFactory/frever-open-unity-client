using System;
using Abstract;
using Navigation.Args;
using UIManaging.Common.SelectableGrid;
using UnityEngine;

namespace UIManaging.Common
{
    public class VideoListItem : BaseContextDataButton<BaseLevelItemArgs>, ISelectableGridViewProvider
    {        
        public event Action OnIdChangedEvent;
        
        [SerializeField] protected VideoThumbnail _videoThumbnail;
        [SerializeField] private GameObject _videoIsNotAvailableOverlay;
        
        public long Id => ContextData.Level?.Id ?? 0;
        
        protected override void OnInitialized()
        {
            RefreshThumbnail();
            OnIdChangedEvent?.Invoke();
        }

        protected virtual void RefreshThumbnail()
        {
            var args = ContextData;
            var hasVideoModel = args.Video != null;

            _videoThumbnail.gameObject.SetActive(hasVideoModel);

            if (hasVideoModel)
            {
                SwitchContentIsNotAvailableOverlay(false);   
                _videoThumbnail.Initialize(ContextData);
            }
        }

        protected void SwitchContentIsNotAvailableOverlay(bool isOn)
        {
            if(_videoIsNotAvailableOverlay) _videoIsNotAvailableOverlay.gameObject.SetActive(isOn);
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            ContextData?.OnInteracted();
        }
    }
}