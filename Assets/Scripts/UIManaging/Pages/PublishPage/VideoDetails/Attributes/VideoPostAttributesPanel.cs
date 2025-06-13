using System.Collections.Generic;
using Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostAttributesPanel: BaseContextPanel<VideoPostAttributesModel>
    {
        [SerializeField] private List<BaseVideoPostAttribute> _videoAttributes;

        protected override void OnInitialized()
        {
            _videoAttributes.ForEach(attribute => {
                attribute.Initialize(ContextData);
            });
        }

        protected override void BeforeCleanUp()
        {
            _videoAttributes.ForEach(attribute => {
                attribute.CleanUp();
            });
        }
    }
}