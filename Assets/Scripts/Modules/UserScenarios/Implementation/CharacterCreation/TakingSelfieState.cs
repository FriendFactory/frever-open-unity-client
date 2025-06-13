using Bridge.Services.SelfieAvatar;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class TakingSelfieState : StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition MoveBack;
        public ITransition OnSelfieCapturedTransition;

        public TakingSelfieState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override ScenarioState Type => ScenarioState.TakingSelfie;
        public override ITransition[] Transitions => new[] { MoveBack, OnSelfieCapturedTransition }.RemoveNulls();
      
        public override void Run()
        {
            var pageArgs = new AvatarSelfieArgs
            {
                Gender = Context.Gender,
                BackButtonClicked = OnBackButtonClicked,
                OnSelfieTaken = OnSelfieCaptured
            };
            _pageManager.MoveNext( pageArgs);
        }

        private async void OnBackButtonClicked()
        {
            await MoveBack.Run();
        }

        private async void OnSelfieCaptured(JSONSelfie jsonSelfie)
        {
            Context.JsonSelfie = jsonSelfie;
            await OnSelfieCapturedTransition.Run();
        }
    }
}