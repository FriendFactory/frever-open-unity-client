using System;
using Bridge.Models.VideoServer;
using Common.Collections;
using I2.Loc;
using Laphed.Rx;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostAccessAttribute: VideoPostAttribute<VideoAccess>
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private VideoAccessLocalizationMap _videoAccessLocalizationMap;

        protected override ReactiveProperty<VideoAccess> Target => ContextData.VideoAccess;

        protected override void OnInitialized()
        {
            IsVisible.Value = true;
            
            base.OnInitialized();
        }

        protected override void OnTargetValueChanged() => UpdateLabel();
        protected override void OnBecomeVisible() => UpdateLabel();

        private void UpdateLabel()
        {
            if (!_videoAccessLocalizationMap.TryGetValue(Target.Value, out var accessText)) return;

            _label.text = accessText;
        }
        
        [Serializable]
        internal class VideoAccessLocalizationMap : SerializedDictionary<VideoAccess, LocalizedString>
        {
        }
    }
}