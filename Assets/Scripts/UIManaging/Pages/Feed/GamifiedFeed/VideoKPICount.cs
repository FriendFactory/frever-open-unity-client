using Abstract;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Feed.GamifiedFeed
{
    public class VideoKPICount: BaseContextDataView<IVideoKPICount>
    {
        [SerializeField] private TMP_Text _count;
        
        protected virtual bool IsVisible => ContextData.Count > 0;
        protected virtual bool IsOwner => ContextData.IsOwner;
        
        protected override void OnInitialized()
        {
            if (!IsOwner)
            {
                this.SetActive(false);
                return;
            }
            
            ContextData.Changed += OnCountChanged;
            
            OnCountChanged(ContextData.Count);
        }

        protected override void BeforeCleanup()
        {
            if (ContextData == null) return;
            
            ContextData.Changed -= OnCountChanged;
        }

        private void OnCountChanged(long value)
        {
            if (!IsOwner) return;
            
            _count.text = ContextData.Count.ToString();
            
            this.SetActive(IsVisible);
        }
    }
}