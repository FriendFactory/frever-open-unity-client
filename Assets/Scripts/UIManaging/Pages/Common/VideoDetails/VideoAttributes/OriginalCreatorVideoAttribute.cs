using I2.Loc;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.PreRemixPage.Ui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class OriginalCreatorVideoAttribute: VideoAttribute
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [Header("L10N")]
        [SerializeField] private LocalizedString _prefix;

        [Inject] private PageManager _pageManager;

        protected override void OnBecomeVisible()
        {
            _button.onClick.AddListener(OnClicked);
            
            _label.text = $"{_prefix} <b>@{Video.OriginalCreator.Nickname}</b>";
        }

        protected override void BeforeCleanUp()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        protected override bool ShouldBeVisible() => Video?.RemixedFromLevelId.HasValue ?? default;

        protected override void OnClicked()
        {
            _pageManager.MoveNext(new PreRemixPageArgs(ContextData.Video));
            
            base.OnClicked();
        }
    }
}