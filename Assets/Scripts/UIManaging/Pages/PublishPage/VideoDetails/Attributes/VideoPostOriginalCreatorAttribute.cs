using I2.Loc;
using Laphed.Rx;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostOriginalCreatorAttribute: VideoPostAttribute<string>
    {
        [SerializeField] private TMP_Text _originalCreatorName;
        [Header("L10N")]
        [SerializeField] private LocalizedString _prefix;
        
        protected override ReactiveProperty<string> Target => ContextData.OriginalCreator;
        
        protected override void OnTargetValueChanged()
        {
            IsVisible.Value = !string.IsNullOrEmpty(Target.Value);
        }
        
        protected override void OnBecomeVisible()
        {
            _originalCreatorName.text = $"{_prefix} <b>@{Target.Value}</b>";
        }
    }
}