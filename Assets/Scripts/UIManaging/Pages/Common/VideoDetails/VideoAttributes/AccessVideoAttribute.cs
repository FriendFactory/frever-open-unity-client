using System;
using Bridge;
using Bridge.Models.VideoServer;
using Common.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class AccessVideoAttribute: VideoAttribute
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private VideoAccessLocalizationMap _videoAccessLocalizationMap;
        
        [Inject] private IBridge _bridge;

        protected override void OnBecomeVisible()
        {
            if (!_videoAccessLocalizationMap.TryGetValue(Video.Access, out var accessText)) return;

            _label.text = accessText;
        }

        protected override bool ShouldBeVisible() => _bridge.Profile.GroupId == ContextData.Video.GroupId;

        [Serializable]
        internal class VideoAccessLocalizationMap : SerializedDictionary<VideoAccess, LocalizedString>
        {
        }
    }
}