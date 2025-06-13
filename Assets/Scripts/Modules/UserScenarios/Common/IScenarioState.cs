using System;

namespace Modules.UserScenarios.Common
{
    internal interface IScenarioState
    {
        ScenarioState Type { get; }
        bool IsExitState { get; }
        ITransition[] Transitions { get; }
        void Run();
    }

    internal interface IContextDependant<in TContext>
    {
        void SetContext(TContext context);
    }
    
    internal interface IScenarioState<in TContext>: IScenarioState, IContextDependant<TContext>
    {
    }

    internal abstract class StateBase<TContext> : IScenarioState<TContext>
    {
        protected TContext Context;
        
        public abstract ScenarioState Type { get; }
        public abstract ITransition[] Transitions { get; }
        public abstract void Run();

        public virtual bool IsExitState => false;
        
        public void SetContext(TContext context)
        {
            Context = context;
        }
    }
    
    internal abstract class ExitStateBase : IScenarioState
    {
        public abstract ScenarioState Type { get; }
        public ITransition[] Transitions => Array.Empty<ITransition>();
        public abstract void Run();
        
        public bool IsExitState => true;
    }
    
    internal abstract class ExitStateBase<TContext> : StateBase<TContext>
    {
        public override ITransition[] Transitions => Array.Empty<ITransition>();
        
        public override bool IsExitState => true;
    }

    public enum ScenarioState
    {
        Entry,
        LevelEditor,
        PostRecordEditor,
        CharacterEditor,
        Publish,
        PublishFromGallery,
        TasksExit,
        ProfileExit,
        PreviousPageExit,
        FeedExit,
        CombinedStyleSelection,
        TakingSelfie,
        AvatarPreview,
        CharacterSelection,
        StarCreatorFollow,
        
        StyleBattleStart,
        VotingFeedCharacterEditor,
        SubmitAndVote,
        VotingFeed,
        VotingDone,
        
        SignInOverlay,
        SignInFromSignUpOverlay,
        SignInSuccess,
        SignUpBirthday,
        SignUpUserName,
        SignUpGeneral,
        SignInExitFromOverlay,
        SignInExitToLandingPage,
        SignInVerificationCode,
        SignUpExitToLandingPage,
        SignUpPassword,
        OnboardingVerificationCode,
        OnboardingCharacterEditor,
        OnboardingAnyOtherWayExit,
        OnboardingExitToCreationPost,
        OnboardingExitToForMeFeed,
        OnboardingContactsFollow,
        PostOnboardingExit,
        
        TemplateGrid,
        HomePageExit,
        VideoMessageEditor,
        InboxPageExit,
        ChatPageExit,
        ExitToVideoMessageCreation,
        ExitToLevelCreation,
        CrewPageExit,
        RaceSelection,
        OnboardingLoadingState,

        RatingFeed,
        EditUserName,
        ExitEditingUserName,
        SetupAuthentication,
        VerificationCode,
        AddLoginMethod,
    }
}