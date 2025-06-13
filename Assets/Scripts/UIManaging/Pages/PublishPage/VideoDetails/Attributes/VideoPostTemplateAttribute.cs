using Laphed.Rx;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostTemplateAttribute: VideoPostAttribute<string>
    {
        [SerializeField] private TMP_Text _templateName;
        
        protected override ReactiveProperty<string> Target => ContextData.TemplateName;
        
        protected override void OnTargetValueChanged()
        {
            IsVisible.Value = !string.IsNullOrEmpty(Target.Value);
        }

        protected override void OnBecomeVisible()
        {
            _templateName.text = Target.Value;
        }
    }
}