using Bridge.Models.Common;
using Common.Abstract;
using UnityEngine;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public abstract class SoundPreviewPanelBase : BaseContextPanel<IPlayableMusic>
    {
        [SerializeField] private SoundThumbnail _soundThumbnail;

        protected override void OnInitialized()
        {
            _soundThumbnail.Initialize(ContextData);
        }

        protected override void BeforeCleanUp()
        {
            _soundThumbnail.CleanUp();
        }
    }
}