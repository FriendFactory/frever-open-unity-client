using Bridge;
using Bridge.Models.ClientServer.Chat;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    [UsedImplicitly]
    internal sealed class ChatPageExitState : ChatPageExitStateBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState Type => ScenarioState.ChatPageExit;
        
        protected override ChatInfo ChatRequestedToOpenOnFinish => Context.OpenedFromChat;
        protected override MessagePublishInfo MessagePublishInfo => Context.PublishContext.VideoPublishSettings.MessagePublishInfo;
        
        public ChatPageExitState(PageManager pageManager, IBridge bridge) : base(pageManager, bridge)
        {
        }
    }
}