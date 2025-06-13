using System;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class ExitToCreationNewLevelScenarioState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.ExitToLevelCreation;
        public override ITransition[] Transitions => Array.Empty<ITransition>();
        public override bool IsExitState => true;

        public override void Run()
        {
            var args = new StartLevelCreationArgs
            {
                ChatInfo = Context.OpenedFromChat
            };
            var messagePublishInfo = Context.PublishContext.VideoPublishSettings.MessagePublishInfo;
            if (messagePublishInfo != null)
            {
                args.ShareDestination = messagePublishInfo.ShareDestination;
            }
            Context.ExecuteLevelCreationScenario?.Invoke(args);
        }
    }
}