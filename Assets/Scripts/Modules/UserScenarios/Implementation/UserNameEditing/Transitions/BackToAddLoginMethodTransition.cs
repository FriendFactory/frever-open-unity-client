using System.Threading.Tasks;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.UserCredentialsChanging;
using Navigation.Core;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.UserNameEditing.Transitions
{
    internal sealed class BackToAddLoginMethodTransition : TransitionBase<EditNameContext>, IResolvable
    {
        public override ScenarioState To => ScenarioState.AddLoginMethod;
        
        private readonly PageManager _pageManager;
        
        public BackToAddLoginMethodTransition(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            // The state is popup, so it creates cycling transitions on back and
            // we have to switch the page manually here
            _pageManager.MoveBack();
            return base.OnRunning();
        }
    }
}
