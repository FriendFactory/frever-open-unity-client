using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Common.Publishers;
using JetBrains.Annotations;
using Modules.Amplitude;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using UIManaging.Pages.PublishPage;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class ExitScenarioFromPublish: ExitScenarioFromPublishBase
    {
        private readonly ExitToChatPageStateHelper _stateHelper;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private PublishContext PublishContext => Context.PublishContext;
        private MessagePublishInfo MessagePublishInfo => PublishContext.VideoPublishSettings.MessagePublishInfo;
        private PublishInfo PublishInfo => PublishContext.VideoPublishSettings.PublishInfo;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public ExitScenarioFromPublish(ILevelManager levelManager, PublishVideoHelper publishHelper,
            IPublishVideoPopupManager popupManager, AmplitudeManager amplitudeManager, IBridge bridge,
            ExitToChatPageStateHelper stateHelper) : base(levelManager, publishHelper, popupManager, amplitudeManager, bridge)
        {
            _stateHelper = stateHelper;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task<bool> SetNextState()
        {
            if (Context.OpenedFromChat != null || PublishContext.PublishingType == PublishingType.VideoMessage && _stateHelper.HasUserSelectedDirectMessageVideoSend(MessagePublishInfo))
            {
                DestinationState = _stateHelper.GetDestinationForDirectMessageSharing(Context.OpenedFromChat, MessagePublishInfo);
                return Task.FromResult(true);
            }
            
            if (PublishInfo.Access == VideoAccess.Private || PublishInfo.Access == VideoAccess.ForFriends || Context.LevelData.RemixedFromLevelId != null)
            {
                DestinationState = ScenarioState.ProfileExit;
            }
            else
            {
                DestinationState = ScenarioState.FeedExit;
            }

            return Task.FromResult(true);
        }
    }
}