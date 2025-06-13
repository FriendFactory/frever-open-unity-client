using System;
using System.Linq;
using Bridge.Models.ClientServer.Tasks;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation
{
    internal interface ILevelCreationStatesSetupProvider
    {
        LevelCreationSetupBase GetStatesSetupForTask(TaskFullInfo task);
        LevelCreationSetupBase GetStatesSetup(LevelCreationSetup type);
    }

    [UsedImplicitly]
    internal sealed class LevelCreationStatesSetupProvider : ILevelCreationStatesSetupProvider
    {
        private readonly DiContainer _diContainer;
        private readonly TaskPagesAndStatesSetup[] _taskAndStatesSetups;

        public LevelCreationStatesSetupProvider(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _taskAndStatesSetups = new[]
            {
                new TaskPagesAndStatesSetup
                {
                    Pages = new[] { Page.PostRecordEditor },
                    LevelCreationSetup = LevelCreationSetup.PostRecordOnly
                },
                new TaskPagesAndStatesSetup
                {
                    Pages = new[] { Page.PostRecordEditor, Page.CharacterEditor },
                    LevelCreationSetup = LevelCreationSetup.PostRecordWithOutfitCreation
                },
                new TaskPagesAndStatesSetup
                {
                    Pages = new[] { Page.LevelEditor, Page.PostRecordEditor },
                    LevelCreationSetup = LevelCreationSetup.LevelEditorToPip
                },
                new TaskPagesAndStatesSetup
                {
                    Pages = new[] { Page.PostRecordEditor, Page.LevelEditor },
                    LevelCreationSetup = LevelCreationSetup.PostRecordWithNewEventCreation
                },
                new TaskPagesAndStatesSetup
                {
                    Pages = new [] { Page.PostRecordEditor, Page.CharacterEditor, Page.LevelEditor},
                    LevelCreationSetup = LevelCreationSetup.PostRecordWithOutfitAndNewEventCreation
                },
                new TaskPagesAndStatesSetup
                {
                    Pages = new [] { Page.CharacterEditor, Page.PostRecordEditor},
                    LevelCreationSetup = LevelCreationSetup.CharacterDressingToPostRecord
                }
            };
        }

        public LevelCreationSetupBase GetStatesSetupForTask(TaskFullInfo task)
        {
            var setupType = _taskAndStatesSetups.First(x => task.Pages.SequenceEqual(x.Pages)).LevelCreationSetup;
            var statesSetup = GetStatesSetup(setupType);
            if (statesSetup.States.FirstOrDefault(x => x.Type == ScenarioState.Publish) is PublishStateBase publishState)
            {
                var exitTask = _diContainer.Resolve<ExitTaskScenarioFromPublish>();
                publishState.MoveNextChat = exitTask;
                publishState.MoveNextPublic = exitTask;
                publishState.MoveNextLimitedAccess = exitTask;
            }
            return statesSetup;
        }

        public LevelCreationSetupBase GetStatesSetup(LevelCreationSetup type)
        {
            switch (type)
            {
                case LevelCreationSetup.DefaultLevelCreation:
                    return _diContainer.Resolve<DefaultLevelCreationSetup>();
                case LevelCreationSetup.LevelEditorToPip:
                    return _diContainer.Resolve<LevelEditorAndPip>();
                case LevelCreationSetup.PostRecordOnly:
                    return _diContainer.Resolve<PiPSetup>();
                case LevelCreationSetup.PostRecordWithOutfitCreation:
                    return _diContainer.Resolve<PiPAndCharacterEditorSetup>();
                case LevelCreationSetup.PostRecordWithOutfitAndNewEventCreation:
                    return _diContainer.Resolve<PiPAndCharacterAndLevelEditorSetup>();
                case LevelCreationSetup.PostRecordWithNewEventCreation:
                    return _diContainer.Resolve<PiPWithCreationNewEventOnlySetup>();
                case LevelCreationSetup.RemixSetup:
                    return _diContainer.Resolve<RemixStatesSetup>();
                case LevelCreationSetup.CharacterDressingToPostRecord:
                    return _diContainer.Resolve<CharacterEditorAndPipSetup>();
                case LevelCreationSetup.EditLocallySavedLevel:
                    return _diContainer.Resolve<EditLocallySavedLevelSetup>();
                case LevelCreationSetup.RemixSocialAction:
                    return _diContainer.Resolve<RemixSocialActionStatesSetup>();
                case LevelCreationSetup.TemplateActionSetup:
                    return _diContainer.Resolve<TemplateSocialActionLevelCreationSetup>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    internal sealed class TaskPagesAndStatesSetup
    {
        public Page[] Pages;
        public LevelCreationSetup LevelCreationSetup;
    }
}