using Bridge;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class TemplateGridState : StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PopupManager _popupManager;
        private readonly PageManager _pageManager;
        private readonly IBridge _bridge;
        
        public override ScenarioState Type => ScenarioState.TemplateGrid;
        public override ITransition[] Transitions => new []{MoveNext, MoveBack};

        public ITransition MoveBack;
        public ITransition MoveNext;

        public TemplateGridState(PageManager pageManager, PopupManager popupManager,IBridge bridge)
        {
            _pageManager = pageManager;
            _popupManager = popupManager;
            _bridge = bridge;
        }
        public override async void Run()
        {
            var response = await _bridge.GetEventTemplate(Context.InitialTemplateId.Value);
            if (response.IsError)
            {
                Debug.LogError(response.ErrorMessage);
                return;
            }

            var pageArgs = new VideosBasedOnTemplatePageArgs
            {
                TemplateInfo = response.Model,
                TemplateName =  response.Model.Title,
                UsageCount =  response.Model.UsageCount,
                OnBackButtonRequested =  OnMoveBackRequested,
                OnJoinTemplateRequested = OnJoinTemplateRequested,
            };
            _pageManager.MoveNext(pageArgs);
        }

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
        }

        private async void OnJoinTemplateRequested()
        {
            await MoveNext.Run();
        }
    }
}