using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Modules.UserScenarios.Implementation.VotingFeed.States;
using Modules.UserScenarios.Implementation.VotingFeed.Transitions;
using Zenject;
using CharacterEditorState = Modules.UserScenarios.Implementation.LevelCreation.States.CharacterEditorState;

namespace Modules.UserScenarios.Implementation.VotingFeed
{
    [UsedImplicitly]
    internal sealed class VotingFeedStatesSetup : StatesSetupBase, IResolvable
    {
        public VotingFeedStatesSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var styleBattleStart = ResolveState<StyleBattleStartState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var votingFeedCharacterEditor = ResolveState<VotingFeedCharacterEditorState>();
            var pip = ResolveState<VotingFeedPostRecordEditorState>();
            var submitAndVote = ResolveState<SubmitAndVoteState>();
            var votingFeed = ResolveState<VotingFeedState>();
            var votingDone = ResolveState<VotingDoneState>();
            var votingFeedExit = ResolveState<VotingFeedExitState>();
            var tasksExit = ResolveState<TasksExitState>();

            styleBattleStart.MoveBack = new EmptyTransition(votingFeedExit);
            styleBattleStart.MoveNext = ResolveTransition<VotingFeedTaskTransition>();
            
            characterEditor.MoveBack = ResolveTransition<MoveBackFromCharacterEditorTransition>();
            characterEditor.MoveNext = ResolveTransition<MoveNextFromCharacterEditorTransition>();
            
            votingFeedCharacterEditor.MoveBack = new EmptyTransition(votingFeedExit);
            votingFeedCharacterEditor.MoveNext = ResolveTransition<MoveNextFromCharacterEditorTransition>();
            
            pip.MoveNext = ResolveTransition<PostRecordEditorToSubmitAndVoteTransition>();
            pip.OnPreviewFinished = ResolveTransition<BackToPublishAfterPreview>();
            pip.MoveBack = ResolveTransition<MoveBackToVotingFeedCharacterEditorTransition>();
            pip.ExitScenario = new EmptyTransition(votingFeedExit);
            pip.OutfitCreationTransition = ResolveTransition<EnterCharacterEditorTransition>();

            submitAndVote.MoveBack = new EmptyTransition(pip);
            submitAndVote.MoveNext = new EmptyTransition(votingFeed);

            votingFeed.MoveBack = new EmptyTransition(votingFeedExit);
            votingFeed.MoveNext = ResolveTransition<VotingFeedToVotingDoneTransition>();

            votingDone.MoveBack = new EmptyTransition(votingFeedExit);
            votingDone.MoveNextChat = ResolveTransition<ExitTaskScenarioFromPublish>();
            votingDone.MoveNextPublic = ResolveTransition<ExitTaskScenarioFromPublish>();
            votingDone.MoveNextLimitedAccess = ResolveTransition<ExitTaskScenarioFromPublish>();

            return new IScenarioState[] { styleBattleStart, characterEditor, votingFeedCharacterEditor, pip, submitAndVote, votingFeed, votingDone, 
                votingFeedExit, tasksExit };
        }
    }
}