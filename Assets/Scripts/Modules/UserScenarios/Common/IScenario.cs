using System.Threading.Tasks;

namespace Modules.UserScenarios.Common
{
    /// <summary>
    /// Setup of use case(create new level, update draft, new task etc)
    /// </summary>
    internal interface IScenario
    {
        ITransition Entry { get; }
        IScenarioState[] States { get; }
        ITransition[] Transitions { get; }
        Task Setup();
        void OnExit();
    }
    
    internal interface IScenario<in T>: IScenario where T: IScenarioArgs
    {
        void SetArgs(T args);
    }
    
    internal interface IRemixLevelScenario: IScenario<RemixLevelScenarioArgs>
    {
    }

    internal interface IVideoMessageCreationScenario : IScenario<VideoMessageCreationScenarioArgs>
    {
    }

    internal interface IRemixLevelSocialActionScenario: IScenario<RemixLevelSocialActionScenarioArgs>
    {
    }
    
    internal interface IEditLocallySavedLevelScenario: IScenario<EditLocalSavedLevelScenarioArgs>
    {
    }
    
    internal interface IEditDraftScenario: IScenario<EditDraftScenarioArgs>
    {
    }
   
    internal interface IEditTaskDraftScenario: IScenario<EditTaskDraftScenarioArgs>
    {
    }
    
    internal interface ICreateNewLevelScenario: IScenario<CreateNewLevelScenarioArgs>
    {
    }

    internal interface ICreateNewLevelBasedOnTemplateScenario: IScenario<CreateNewLevelBasedOnTemplateScenarioArgs>
    {
    }

    internal interface ICreateNewLevelBasedOnTemplateSocialActionScenario: IScenario<CreateNewLevelBasedOnTemplateScenarioArgs>
    {
    }

    internal interface ITaskScenario: IScenario<TaskScenarioArgs>
    {
    }

    internal interface ICreateNewCharacterScenario : IScenario<CreateNewCharacterArgs>
    {
    }
    
    internal interface ICreateNewCharacterOnBoardingScenario : IScenario<CreateNewCharacterArgs>
    {
    }

    internal interface IEditCharacterScenario : IScenario<EditCharacterArgs>
    {
    }

    internal interface IOnboardingScenario : IScenario<OnboardingArgs>
    {
    }

    internal interface IVotingFeedScenario : IScenario<VotingFeedArgs>
    {
    }

    internal interface IUploadGalleryVideoScenario : IScenario<NonLevelVideoUploadArgs>
    {
    }
    
    internal interface IEditNameScenario : IScenario<EditNameArgs>
    {
    }
}