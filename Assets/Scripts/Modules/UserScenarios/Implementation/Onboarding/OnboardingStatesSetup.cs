using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.Onboarding.States;
using Modules.UserScenarios.Implementation.Onboarding.Transitions;
using Zenject;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    [UsedImplicitly]
    internal sealed class OnboardingStatesSetup : StatesSetupBase, IResolvable
    {
        public OnboardingStatesSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            var birthdayState = ResolveState<OnboardingBirthdayState>();
            var signUp = ResolveState<OnboardingSignUpState>();
            var signUpExitState = ResolveState<SignUpExitState>();

            var signInOverlay = ResolveState<SignInOverlayState>();
            var signInFromSignUpOverlay = ResolveState<SignInFromSignUpOverlayState>();
            var signInVerificationCode = ResolveState<SignInVerificationCodeState>();
            var exitFromLoginOverlay = ResolveState<SignInExitFromOverlayState>();
            var exitToPostOnboardingPage = ResolveState<ExitToPostOnboardingPageState>();

            var raceSelection = ResolveState<OnboardingRaceSelectionState>();
            var combinedStyleSelection = ResolveState<CharacterStyleSelectionState>();
            var onboardingLoadingState = ResolveState<OnboardingLoadingState>();
            var characterEditor = ResolveState<OnboardingCharacterEditorState>();
            var selfieTaking = ResolveState<TakingSelfieState>();
            var avatarPreview = ResolveState<AvatarPreviewState>();

            signInOverlay.MoveBack = new EmptyTransition(exitFromLoginOverlay);
            signInOverlay.MoveNext = new EmptyTransition(signInVerificationCode);
            signInOverlay.MoveNextNoVerification = ResolveTransition<SignInCompletedTransition>();
            var startCharacterCreationTransition = new StartCharacterCreationTransition(ResolveService<IMetadataProvider>(), new ITransition[]
            {
                new EmptyTransition(raceSelection), new EmptyTransition(combinedStyleSelection)
            });
            signInOverlay.SignUp = new EmptyTransition(onboardingLoadingState);
            onboardingLoadingState.MoveNext = startCharacterCreationTransition;

            signInFromSignUpOverlay.MoveBack = new EmptyTransition(signUp);
            signInFromSignUpOverlay.MoveNext = new EmptyTransition(signInVerificationCode);
            signInFromSignUpOverlay.MoveNextNoVerification = ResolveTransition<SignInCompletedTransition>();

            signInVerificationCode.MoveBack = ResolveTransition<SignInMoveBackTransition>();
            signInVerificationCode.MoveNext = ResolveTransition<SignInCompletedTransition>();

            raceSelection.MoveNext = new EmptyTransition(combinedStyleSelection);
            
            combinedStyleSelection.MoveBack = new EmptyTransition(raceSelection);
            combinedStyleSelection.SelfieSelectedTransition = new EmptyTransition(selfieTaking);
            combinedStyleSelection.StyleSelectedTransition = new EmptyTransition(characterEditor);
            
            selfieTaking.MoveBack = new EmptyTransition(combinedStyleSelection);
            selfieTaking.OnSelfieCapturedTransition = new EmptyTransition(avatarPreview);

            avatarPreview.MoveBack = ResolveTransition<AvatarPreviewToSelfieTakingTransition>();
            avatarPreview.MoveNext = ResolveTransition<OnboardingAvatarPreviewToCharacterEditorTransition>();

            characterEditor.MoveBack = ResolveTransition<BackFromOnboardingCharacterEditorTransition>();
            characterEditor.MoveNext = new EmptyTransition(birthdayState);

            birthdayState.MoveNext = ResolveTransition<FinishOnboardingTransition>();
            birthdayState.MoveBack = ResolveTransition<OnboardingBackToCharacterEditorTransition>();
            
            signUp.MoveNextNoVerification = ResolveTransition<FinishOnboardingTransition>();
            signUp.MoveSignIn = new EmptyTransition(signInFromSignUpOverlay);
            signUp.MoveSignInNoVerification = ResolveTransition<SignInCompletedTransition>();
            
            return new IScenarioState[]
            {
                signInOverlay, exitFromLoginOverlay, signInVerificationCode,
                birthdayState, signUpExitState, combinedStyleSelection, 
                characterEditor, selfieTaking, avatarPreview, exitToPostOnboardingPage, signUp,
                signInFromSignUpOverlay, onboardingLoadingState, raceSelection
            };
        }
    }
}