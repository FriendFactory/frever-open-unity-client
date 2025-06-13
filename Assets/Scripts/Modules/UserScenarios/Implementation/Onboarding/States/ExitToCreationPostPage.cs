using Bridge;
using Common;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class ExitToCreationPostPage: ExitStateBase<OnboardingContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        private readonly IBridge _bridge;
        public override ScenarioState Type => ScenarioState.OnboardingExitToCreationPost;
        
        public ExitToCreationPostPage(PageManager pageManager, IBridge bridge)
        {
            _pageManager = pageManager;
        }
        
        public override async void Run()
        {
            var result = await _bridge.CompleteOnboarding();

            if (result.IsError)
            {
                Debug.LogError($"Failed to mark onboarding as completed, reason: {result.ErrorMessage}");
            }
            
            var args = new CreatePostPageArgs();
            _pageManager.MoveNext(args);
        }
    }
}