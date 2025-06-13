using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Common.Publishers;
using JetBrains.Annotations;
using Models;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Common
{
    public interface IScenarioManager
    {
        event Action<ScenarioState> StateRun;
        
        void ExecuteNewLevelCreation(Universe universe, HashtagInfo hashtag = null, ChatInfo chatInfo = null);
        void ExecuteNewLevelCreation(Universe universe, IPlayableMusic music);
        void ExecuteNewLevelCreationBasedOnTemplate(CreateNewLevelBasedOnTemplateScenarioArgs args);
        void ExecuteNewLevelCreationBasedOnTemplateSocialAction(CreateNewLevelBasedOnTemplateScenarioArgs args);
        void ExecuteDraftEditing(Level level);
        void ExecuteTaskDraftEditing(Level level);
        void ExecuteLocalSavedLevelEditing(Level locallySaved, Level original);
        void ExecuteLevelRemixing(Universe universe, Level level, long videoId, Action onRemixCanceled,
            long? initialTemplateId);
        void ExecuteVideoMessageCreation(Universe universe, ChatInfo openChatOnComplete = null, PublishingType? publishingType = null);

        void ExecuteLevelRemixingSocialAction(Guid recommendationId, long actionId, Level level, long videId,
            CharacterFullInfo[] characters);
        void ExecuteTask(Universe universe, TaskFullInfo taskInfo);
        void ExecuteNewCharacterCreation(long raceId, Action onDisplayed = null);
        void ExecuteNewCharacterCreationOnBoarding(); // TODO: remove this scenario once new onboarding is done
        void ExecuteCharacterEditing(CharacterFullInfo characterFullInfo, long? categoryTypeId = null, long? themeId = null);
        void ExecuteOnboarding();
        void ExecuteSignIn();
        void ExecuteVotingFeed(Universe universe, TaskFullInfo taskInfo, Guid recommendationIdd = default, long? socialActionId = null);
        void ExecuteNonLevelVideoCreationScenario(NonLeveVideoData videoData, ChatInfo openChatOnComplete = null);
        void ExecuteNicknameEditingScenario();
    }
    
    [UsedImplicitly]
    internal sealed class ScenarioManager: IScenarioManager
    {
        public event Action<ScenarioState> StateRun;
        
        private readonly ICreateNewLevelScenario _createNewLevelScenario;
        private readonly ICreateNewLevelBasedOnTemplateScenario _newLevelBasedOnTemplateScenario;
        private readonly ICreateNewLevelBasedOnTemplateSocialActionScenario _newLevelBasedOnTemplateSocialActionScenario;
        private readonly IEditDraftScenario _editDraftScenario;
        private readonly IRemixLevelScenario _remixLevelScenario;
        private readonly IRemixLevelSocialActionScenario _remixLevelSocialActionScenario;
        private readonly IEditLocallySavedLevelScenario _editLocallySavedLevelScenario;
        private readonly ITaskScenario _taskScenario;
        private readonly IEditTaskDraftScenario _editTaskDraftScenario;
        private readonly ICreateNewCharacterScenario _createNewCharacterScenario;
        private readonly ICreateNewCharacterOnBoardingScenario _createNewCharacterOnBoardingScenario;
        private readonly IEditCharacterScenario _editCharacterScenario;
        private readonly IOnboardingScenario _onboardingScenario;
        private readonly IVotingFeedScenario _votingFeedScenario;
        private readonly IVideoMessageCreationScenario _videoMessageCreationScenario;
        private readonly IUploadGalleryVideoScenario _uploadGalleryVideoScenario;
        private readonly IEditNameScenario _editNameScenario;
        private readonly StateMachine _stateMachine;

        private readonly PageManager _pageManager;
        private PageId _lastScenarioExecutedFromPage;

        private PageId CurrentPage => _pageManager.CurrentPage.Id;
        
        public ScenarioManager(
            ICreateNewLevelScenario createNewLevelScenario, 
            IEditDraftScenario editDraftScenario,
            IRemixLevelScenario remixLevelScenario, 
            ICreateNewLevelBasedOnTemplateScenario newLevelBasedOnTemplateScenario, 
            IEditLocallySavedLevelScenario editLocallySavedLevelScenario,
            ITaskScenario taskScenario, StateMachine stateMachine, 
            IEditTaskDraftScenario editTaskDraftScenario, 
            ICreateNewCharacterScenario createNewCharacterScenario, 
            ICreateNewCharacterOnBoardingScenario createNewCharacterOnBoardingScenario,
            IEditCharacterScenario editCharacterScenario,
            IOnboardingScenario onboardingScenario,
            IVotingFeedScenario votingFeedScenario,
            IRemixLevelSocialActionScenario remixLevelSocialActionScenario,
            ICreateNewLevelBasedOnTemplateSocialActionScenario newLevelBasedOnTemplateSocialActionScenario,
            IVideoMessageCreationScenario videoMessageCreationScenario, PageManager pageManager,
            IUploadGalleryVideoScenario uploadGalleryVideoScenario, IEditNameScenario editNameScenario)
        {
            _createNewLevelScenario = createNewLevelScenario;
            _editDraftScenario = editDraftScenario;
            _remixLevelScenario = remixLevelScenario;
            _newLevelBasedOnTemplateScenario = newLevelBasedOnTemplateScenario;
            _newLevelBasedOnTemplateSocialActionScenario = newLevelBasedOnTemplateSocialActionScenario;
            _videoMessageCreationScenario = videoMessageCreationScenario;
            _pageManager = pageManager;
            _uploadGalleryVideoScenario = uploadGalleryVideoScenario;
            _editLocallySavedLevelScenario = editLocallySavedLevelScenario;
            _taskScenario = taskScenario;
            _stateMachine = stateMachine;
            _editTaskDraftScenario = editTaskDraftScenario;
            _createNewCharacterScenario = createNewCharacterScenario;
            _createNewCharacterOnBoardingScenario = createNewCharacterOnBoardingScenario;
            _editCharacterScenario = editCharacterScenario;
            _onboardingScenario = onboardingScenario;
            _votingFeedScenario = votingFeedScenario;
            _remixLevelSocialActionScenario = remixLevelSocialActionScenario;
            _editNameScenario = editNameScenario;
        }

        public void ExecuteNewLevelCreation(Universe universe, HashtagInfo hashtag, ChatInfo chatInfo)
        {
            var args = new CreateNewLevelScenarioArgs
            {
                Universe = universe,
                Hashtag = hashtag,
                OpenedFromChat = chatInfo,
                ShowTemplateCreationStep = true,
                ShowDressingStep = true,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother
            };
            Run(_createNewLevelScenario, args);
        }
        
        public void ExecuteNewLevelCreation(Universe universe, IPlayableMusic music)
        {
            var args = new CreateNewLevelScenarioArgs
            {
                Universe = universe,
                Music = music,
                ShowTemplateCreationStep = true,
                ShowDressingStep = true,
            };
            Run(_createNewLevelScenario, args);
        }

        public void ExecuteNewLevelCreationBasedOnTemplate(CreateNewLevelBasedOnTemplateScenarioArgs args)
        {
            args.StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother;
            Run(_newLevelBasedOnTemplateScenario, args);
        }
        
        public void ExecuteNewLevelCreationBasedOnTemplateSocialAction(CreateNewLevelBasedOnTemplateScenarioArgs args)
        {
            args.StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother;
            Run(_newLevelBasedOnTemplateSocialActionScenario, args);
        }

        public void ExecuteDraftEditing(Level level)
        {
            var args = new EditDraftScenarioArgs
            {
                Draft = level,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother
            };
            Run(_editDraftScenario, args);
        }

        public void ExecuteTaskDraftEditing(Level level)
        {
            var args = new EditTaskDraftScenarioArgs
            {
                Draft = level,
                TaskId = level.SchoolTaskId.Value,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother
            };
            Run(_editTaskDraftScenario, args);
        }

        public void ExecuteLocalSavedLevelEditing(Level locallySaved, Level original)
        {
            var args = new EditLocalSavedLevelScenarioArgs
            {
                LocallySavedLevel = locallySaved,
                OriginalFromServer = original,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother
            };
            Run(_editLocallySavedLevelScenario, args);
        }

        public void ExecuteLevelRemixing(Universe universe, Level level, long videoId, Action onRemixCanceled,
            long? initialTemplateId)
        {
            var args = new RemixLevelScenarioArgs
            {
                Universe = universe,
                Level = level,
                VideoId = videoId,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother,
                OnRemixCanceled = onRemixCanceled,
                InitialTemplateId = initialTemplateId,
            };
            Run(_remixLevelScenario, args);
        }

        public void ExecuteVideoMessageCreation(Universe universe, ChatInfo openChatOnComplete = null, PublishingType? publishingType = null)
        {
            Run(_videoMessageCreationScenario, new VideoMessageCreationScenarioArgs
            {
                Universe = universe,
                PublishingType = publishingType,
                OpenedFromChat = openChatOnComplete,
                StartLevelCreationAction = ExecuteNewLevelCreationScenarioFromAnother
            });
        }

        public void ExecuteLevelRemixingSocialAction(Guid recommendationId, long actionId, Level level, long videId, CharacterFullInfo[] characters)
        {
            var args = new RemixLevelSocialActionScenarioArgs
            {
                RecommendationId = recommendationId,
                ActionId = actionId,
                Level = level,
                VideoId = videId,
                Characters = characters,
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother
            };
            
            Run(_remixLevelSocialActionScenario, args);
        }

        public void ExecuteTask(Universe universe, TaskFullInfo taskInfo)
        {
            var args = new TaskScenarioArgs { TaskInfo = taskInfo, Universe = universe};
            Run(_taskScenario, args);
        }

        public void ExecuteNewCharacterCreation(long raceId, Action onDisplayed)
        {
            var args = new CreateNewCharacterArgs
            {
                RaceId = raceId,
                OnDisplayed = onDisplayed
            };
            Run(_createNewCharacterScenario, args);
        }

        public void ExecuteNewCharacterCreationOnBoarding()
        {
            var args = new CreateNewCharacterArgs();
            Run(_createNewCharacterOnBoardingScenario, args);
        }

        public void ExecuteCharacterEditing(CharacterFullInfo characterFullInfo, long? categoryTypeId = null, long? themeId = null)
        {
            var args = new EditCharacterArgs
            {
                Character = characterFullInfo,
                CategoryTypeId = categoryTypeId,
                ThemeId = themeId,
            };
            
            Run(_editCharacterScenario, args);
        }

        public void ExecuteOnboarding()
        {
            var args = new OnboardingArgs();
            Run(_onboardingScenario, args);
        }
        
        public void ExecuteSignIn()
        {
            var args = new OnboardingArgs
            {
                StartFromSignIn = true,
            };
            Run(_onboardingScenario, args);
        }

        public void ExecuteVotingFeed(Universe universe, TaskFullInfo taskInfo, Guid recommendationId = default, long? actionId = null)
        {
            var args = new VotingFeedArgs
            {
                Universe = universe,
                RecommendationId = recommendationId,
                ActionId = actionId,
                TaskInfo = taskInfo
            };
            Run(_votingFeedScenario, args);
        }

        public void ExecuteNonLevelVideoCreationScenario(NonLeveVideoData videoData, ChatInfo openChatOnComplete = null)
        {
            var args = new NonLevelVideoUploadArgs
            {
                NonLeveVideoData = videoData,
                OpenChatOnComplete = openChatOnComplete
            };
            Run(_uploadGalleryVideoScenario, args);
        }

        public void ExecuteNicknameEditingScenario()
        {
            var args = new EditNameArgs();
            Run(_editNameScenario, args);
        }

        private void Run<TArgs>(IScenario<TArgs> scenario, TArgs args) where TArgs: ScenarioArgsBase
        {
            Run(scenario, args, CurrentPage);
        }
        
        private void Run<TArgs>(IScenario<TArgs> scenario, TArgs args, PageId executedFromPageId) where TArgs: ScenarioArgsBase
        {
            args.ExecutedFrom = executedFromPageId;
            _lastScenarioExecutedFromPage = executedFromPageId;
            scenario.SetArgs(args);
            _stateMachine.StateRun += OnStateRun;
            _stateMachine.Run(scenario);
        }

        private void OnStateRun(IScenarioState state)
        {
            if (state.IsExitState)
            {
                _stateMachine.StateRun -= OnStateRun;
            }
            
            StateRun?.Invoke(state.Type);
        }

        private void ExecuteVideoMessageCreationScenarioFromAnother(VideoMessageOpenArgs args)
        {
            Run(_videoMessageCreationScenario, new VideoMessageCreationScenarioArgs
            {
                StartLevelCreationAction = ExecuteNewLevelCreationScenarioFromAnother,
                ShareDestination = args.ShareDestination,
                OpenedFromChat = args.ChatInfo
            }, _lastScenarioExecutedFromPage);
        }
        
        private void ExecuteNewLevelCreationScenarioFromAnother(StartLevelCreationArgs passedArgs)
        {
            var args = new CreateNewLevelScenarioArgs
            {
                StartVideoMessageEditorAction = ExecuteVideoMessageCreationScenarioFromAnother,
                OpenedFromChat = passedArgs.ChatInfo,
                ShareDestination = passedArgs.ShareDestination
            };
            Run(_createNewLevelScenario, args, _lastScenarioExecutedFromPage);
        }
    }
}