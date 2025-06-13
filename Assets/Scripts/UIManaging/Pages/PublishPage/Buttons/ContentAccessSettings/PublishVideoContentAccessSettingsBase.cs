using Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal interface IPublishVideoContentAccessSettings
    {
        bool AllowRemix { get; }
        bool AllowComment { get; }
    }

    internal abstract class PublishVideoContentAccessSettings<TModel> : BaseContextPanel<TModel>, IPublishVideoContentAccessSettings where TModel: ContentAccessSettingsModel
    {
        [SerializeField] private VideoPostSettingsToggle _allowRemixToggle;
        [SerializeField] private VideoPostSettingsToggle _allowCommentsToggle;
        [SerializeField] private GameObject _allowRemixSetting;
        protected override bool IsReinitializable => true;

        public bool AllowRemix
        {
            get => _allowRemixToggle.IsOn;
            protected set => _allowRemixToggle.IsOn = value;
        }

        public bool AllowComment
        {
            get => _allowCommentsToggle.IsOn;
            protected set => _allowCommentsToggle.IsOn = value;
        }

        protected override void OnInitialized()
        {
            _allowCommentsToggle.Initialize();
            _allowRemixToggle.Initialize();
            
            if (!ContextData.ForceRemix) return;
            
            _allowRemixToggle.SetWithoutNotify(AllowRemix);
            _allowRemixSetting.SetActive(false);
        }

        protected override void BeforeCleanUp()
        {
            _allowCommentsToggle.CleanUp();
            _allowRemixToggle.CleanUp();
        }
    }
}