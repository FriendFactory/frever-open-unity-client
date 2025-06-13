using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.VideoRating
{
    internal sealed class VotingRewardInstaller: MonoInstaller
    {
        [SerializeField] private VideoRatingTierBadgeSettings[] _settings;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VideoRatingTierSettingsProvider>().AsSingle().WithArguments(_settings);
            Container.BindInterfacesAndSelfTo<VideoRatingStatusModel>().AsSingle();
        }
    }
}