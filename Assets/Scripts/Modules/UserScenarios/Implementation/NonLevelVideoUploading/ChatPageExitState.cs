using Bridge;
using Bridge.Models.ClientServer.Chat;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.NonLevelVideoUploading
{
    [UsedImplicitly]
    internal sealed class ChatPageExitState: ChatPageExitStateBase<NonLevelVideoPublishContext>, IResolvable
    {
        public ChatPageExitState(PageManager pageManager, IBridge bridge) : base(pageManager, bridge)
        {
        }

        public override ScenarioState Type => ScenarioState.ChatPageExit;
        protected override ChatInfo ChatRequestedToOpenOnFinish => Context.OpenChatOnComplete;
        protected override MessagePublishInfo MessagePublishInfo => Context.MessagePublishInfo;
    }
}