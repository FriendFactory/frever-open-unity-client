using UIManaging.Pages.Common.VideoRating;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal sealed class VideoRatingInstaller: MonoInstaller
    {
        [SerializeField] private VideoRatingTierBadgeSettings[] _settings;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VideoRatingTierSettingsProvider>().AsSingle().WithArguments(_settings);
        }
    }
}