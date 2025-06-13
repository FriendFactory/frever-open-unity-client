using Modules.FeaturesOpening;
using Modules.QuestManaging;
using Modules.QuestManaging.Redirections;
using Modules.SignUp;
using TipsManagment;
using UIManaging.Pages.OnBoardingPage;
using UIManaging.Pages.OnBoardingPage.UI;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class OnBoardingServicesBinder
    {
        public static void BindOnBoardingServices(this DiContainer container, TutorialConfig tutorialConfig)
        {
            container.BindInterfacesAndSelfTo<SignInService>().AsSingle();
            container.BindInterfacesAndSelfTo<SignUpService>().AsSingle();
            container.Bind<TutorialConfig>().FromInstance(tutorialConfig).AsSingle();
            container.BindInterfacesAndSelfTo<TipManager>().AsSingle();
            container.Bind<BirthDayPicker>().AsSingle();
            container.BindInterfacesTo<AppFeaturesManager>().AsSingle();
            container.BindInterfacesAndSelfTo<UnlockedLevelCreationFeatureHandler>().AsSingle();
            container.BindInterfacesAndSelfTo<LockedFeaturesFilesLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<QuestManager>().AsSingle();
            container.BindInterfacesAndSelfTo<QuestRedirectionDictionary>().AsSingle();
            container.Bind<DiContainer>().FromInstance(container).AsSingle()
                     .WhenInjectedInto<QuestRedirectionDictionary>();
        }
    }
}