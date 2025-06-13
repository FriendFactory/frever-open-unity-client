using UIManaging.Common.Rewards;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.SeasonPage;
using Zenject;

namespace Installers
{
    internal static class SeasonServicesBinder 
    {
        public static void BindSeasonRewards(this DiContainer container)
        {
            container.Bind<RewardEventModel>().AsSingle();
            container.BindInterfacesAndSelfTo<SeasonLikesNotificationHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<SeasonRewardsHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<FeedPopupHelper>().AsSingle();
            container.BindFactory<SeasonRewardPreviewModel, SeasonRewardPreviewModel.Factory>().AsSingle();
        }
    }
}