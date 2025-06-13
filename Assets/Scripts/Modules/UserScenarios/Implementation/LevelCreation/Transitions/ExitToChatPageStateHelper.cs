using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class ExitToChatPageStateHelper
    {
        public ScenarioState GetDestinationForDirectMessageSharing(ChatInfo scenarioExecutionStartChat, MessagePublishInfo messagePublishInfo)
        {
            if (scenarioExecutionStartChat != null)
            {
                return scenarioExecutionStartChat.Type == ChatType.Crew? ScenarioState.CrewPageExit : ScenarioState.ChatPageExit;
            }

            if (GetSendDirectMessageDestinationsCount(messagePublishInfo) > 1) return ScenarioState.InboxPageExit;
            var shareDestinations = messagePublishInfo.ShareDestination;
            var hasUserSelectedCrewChat = !shareDestinations.Chats.IsNullOrEmpty() &&
                                          shareDestinations.Chats.Any(x => x.Type == ChatType.Crew);
            return hasUserSelectedCrewChat ? ScenarioState.CrewPageExit : ScenarioState.ChatPageExit;
        }

        public bool HasUserSelectedDirectMessageVideoSend(MessagePublishInfo messagePublishInfo)
        {
            return GetSendDirectMessageDestinationsCount(messagePublishInfo) > 0;
        }

        private int GetSendDirectMessageDestinationsCount(MessagePublishInfo messagePublishInfo)
        {
            if (messagePublishInfo == null) return 0;
            var shareDestinations = messagePublishInfo.ShareDestination;
            var output = 0;
            if (!shareDestinations.Chats.IsNullOrEmpty())
            {
                output += shareDestinations.Chats.Length;
            }

            if (!shareDestinations.Users.IsNullOrEmpty())
            {
                output += shareDestinations.Users.Length;
            }

            return output;
        }
    }
}