using Common.Abstract;
using TLab.UI.SDF;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoRating
{
    public sealed class VideoRatingTierBadge: BaseContextPanel<VideoRatingTierModel> 
    {
        [SerializeField] private SDFCircle _background;
        [SerializeField] private SDFCircle _foreground;

        [Inject] private VideoRatingTierSettingsProvider _settingsProvider;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            SetTierSettings();
        }

        private void SetTierSettings()
        {
            var settings = _settingsProvider.GetSettings(ContextData.Tier);
            
            _background.fillColor = settings.SecondaryColor;
            _foreground.fillColor = settings.PrimaryColor;
        }
    }
}