using System;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.UserNameEditing.States
{
    [UsedImplicitly]
    internal sealed class ExitEditUserNameState: StateBase<EditNameContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.ExitEditingUserName;
        public override ITransition[] Transitions { get; } = Array.Empty<ITransition>();

        private readonly PageManager _pageManager;

        public ExitEditUserNameState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            _pageManager.MoveBackTo(Context.OpenedFromPage);
        }
    }
}