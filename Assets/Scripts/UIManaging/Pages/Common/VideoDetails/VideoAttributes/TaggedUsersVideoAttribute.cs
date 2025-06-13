using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class TaggedUsersVideoAttribute: VideoAttribute
    {
        [SerializeField] private TextMeshProUGUI _taggedUsersCounter;
        [SerializeField] private Button _taggedUsersButton;
        [Header("L10N")]
        [SerializeField] private LocalizedString _taggedUsersButtonText;

        protected override void OnBecomeVisible()
        {
            _taggedUsersCounter.text = $"{Video.TaggedGroups.Length} {_taggedUsersButtonText}";
            
            _taggedUsersButton.onClick.AddListener(OnClicked);
        }

        protected override void BeforeCleanUp()
        {
            _taggedUsersButton.onClick.RemoveListener(OnClicked);
        }

        protected override bool ShouldBeVisible() => Video.TaggedGroups?.Length > 0;
    }
}