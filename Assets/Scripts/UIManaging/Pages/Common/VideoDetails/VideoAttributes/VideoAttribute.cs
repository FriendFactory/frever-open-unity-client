using System;
using Bridge.Models.VideoServer;
using Common.Abstract;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    public abstract class VideoAttribute: BaseContextPanel<VideoAttributesModel>
    {
        [SerializeField] private VideoAttributeType _type;
        
        public bool IsVisible { get; private set; }
        protected Video Video => ContextData.Video;

        public event Action<VideoAttributeType> Clicked;
        
        protected override void OnInitialized()
        {
            IsVisible = ShouldBeVisible();
            
            this.SetActive(IsVisible);

            if (IsVisible)
            {
                OnBecomeVisible();
            }
        }
        
        protected virtual void OnBecomeVisible() {}

        protected virtual void OnClicked() => Clicked?.Invoke(_type);

        protected abstract bool ShouldBeVisible();
    }
}