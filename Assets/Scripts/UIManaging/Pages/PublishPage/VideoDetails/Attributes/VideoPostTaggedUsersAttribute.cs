using I2.Loc;
using Laphed.Rx;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostTaggedUsersAttribute: VideoPostAttribute<int>
    {
        [SerializeField] private TextMeshProUGUI _taggedUsersCounter;
        [Header("L10N")]
        [SerializeField] private LocalizedString _taggedUsersButtonText;

        protected override ReactiveProperty<int> Target => ContextData.TaggedUsersCount;
        
        protected override void OnTargetValueChanged()
        {
            var isVisible = Target.Value > 0;
            IsVisible.Value = isVisible;
            
            if (!isVisible) return;
            
            _taggedUsersCounter.text = $"{Target.Value} {_taggedUsersButtonText}";
        }
    }
}