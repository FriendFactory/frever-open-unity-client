using System;
using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class VideoAttributesPanel: BaseContextPanel<VideoAttributesModel>
    {
        [SerializeField] private List<VideoAttribute> _videoAttributes;

        [Inject] private PopupManager _popupManager;

        public event Action<VideoAttributeType> AttributeClicked;
        
        protected override void OnInitialized()
        {
            _videoAttributes.ForEach(attribute =>
            {
                attribute.Initialize(ContextData);
                
                attribute.Clicked += OnClicked;
            });

            var isVisible = _videoAttributes.Any(attribute => attribute.IsVisible);
            
            gameObject.SetActive(isVisible);
        }

        protected override void BeforeCleanUp()
        {
            _videoAttributes.ForEach(attribute =>
            {
                attribute.Clicked -= OnClicked;
                
                attribute.CleanUp();
            });
        }
        
        private void OnClicked(VideoAttributeType type)
        {
            AttributeClicked?.Invoke(type);
        }
    }
}