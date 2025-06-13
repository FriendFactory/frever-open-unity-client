using System;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class NonLevelVideoPostSettingsPanel : PostVideoSettingsPanelBase
    {
        [SerializeField] private PublishVideoContentAccessSettings _contentAccessSettings;

        private Action _previewClicked;

        public override IPublishVideoContentAccessSettings ContentAccessSettings => _contentAccessSettings;

        public new void Init()
        {
            _contentAccessSettings.Initialize(new ContentAccessSettingsModel(true, false));
            
            base.Init();
        }

        protected override long[] GetTaggedMemberGroupIds()
        {
            return Array.Empty<long>();
        }
    }
}