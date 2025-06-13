using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(menuName = "L10N/FeedLocalization", fileName = "FeedLocalization")]
    public class FeedLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _feedTabForMe;
        [SerializeField] private LocalizedString _feedTabFeatured;
        [SerializeField] private LocalizedString _feedTabNew;
        [SerializeField] private LocalizedString _feedTabFriends;
        [SerializeField] private LocalizedString _feedTabFollowing;
        
        [SerializeField] private LocalizedString _remixUsedCounterFormat;
        [SerializeField] private LocalizedString _videoViewsCounterFormat;
        
        [SerializeField] private LocalizedString _remixVideoHeader;
        [SerializeField] private LocalizedString _remixStepTwoHeader;
        [SerializeField] private LocalizedString _remixDescription;
        [SerializeField] private LocalizedString _remixSelectCharactersReasonFormat;
        
        [SerializeField] private LocalizedString _templateSelectCharactersTitle;
        [SerializeField] private LocalizedString _templateSelectCharactersReasonFormat;
        
        [SerializeField] private LocalizedString _templateChangePrivacyPopupTitle;
        [SerializeField] private LocalizedString _templateChangePrivacyPopupDescription;
        [SerializeField] private LocalizedString _templateChangePrivacyPopupConfirmButton;
        [SerializeField] private LocalizedString _templateChangePrivacyPopupCancelButton;
        
        [SerializeField] private LocalizedString _templateNotFoundSnackbarMessage;
        [SerializeField] private LocalizedString _templateNotAvailableSnackbarMessage;
        
        [SerializeField] private LocalizedString _templatePrivacyUpdateSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _templatePrivacyUpdateFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _templateGenerationSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _templateGenerationFailedSnackbarMessage;
        [SerializeField] private LocalizedString _templateEditOption;
        [SerializeField] private LocalizedString _templateRenameCompletedSnackbarMessage;
        
        [SerializeField] private LocalizedString _characterSelectorCategoryMyFrevers;
        [SerializeField] private LocalizedString _characterSelectorCategoryFriends;
        [SerializeField] private LocalizedString _characterSelectorCategoryStarCreator;

        [SerializeField] private LocalizedString _deleteVideoPopupTitle;
        [SerializeField] private LocalizedString _deleteVideoPopupDescription;
        [SerializeField] private LocalizedString _deleteVideoPopupUploadedOrMessageDescription;
        [SerializeField] private LocalizedString _deleteVideoPopupTemplateDescription;
        [SerializeField] private LocalizedString _deleteVideoPopupConfirmButton;
        [SerializeField] private LocalizedString _deleteVideoPopupCancelButton;

        [SerializeField] private LocalizedString _videoPrivacyPopupTitle;
        [SerializeField] private LocalizedString _videoPrivacyPopupDescription;
        [SerializeField] private LocalizedString _videoPrivacyPopupConfirmButton;
        [SerializeField] private LocalizedString _videoPrivacyPopupCancelButton;

        [SerializeField] private LocalizedString _videoPrivacyUpdateSuccessSnackbarMessageFormat;
        [SerializeField] private LocalizedString _videoPrivacyUpdateFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _reportVideoFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _reportVideoSuccessPopupTitle;
        [SerializeField] private LocalizedString _reportVideoSuccessPopupDescription;
        [SerializeField] private LocalizedString _reportVideoSuccessPopupConfrimButton;
        
        [SerializeField] private LocalizedString _followingUserSnackbarMessageFormat;
        [SerializeField] private LocalizedString _friendsWithUserSnackbarMessageFormat;
        
        [SerializeField] private LocalizedString _videoSaveSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _videoSaveFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _videoDeletedSnackbarMessage;
        [SerializeField] private LocalizedString _videoDeleteLoadingTitle;
        
        [SerializeField] private LocalizedString _videoShareSuccessSnackbarMessage;
        [SerializeField] private LocalizedString _videoShareFailedSnackbarMessage;
        
        [SerializeField] private LocalizedString _useTemplateButton;
        [SerializeField] private LocalizedString _joinChallengeButton;
        [SerializeField] private LocalizedString _joinStyleChallengeButton;
        [SerializeField] private LocalizedString _challengeResultsButton;
        [SerializeField] private LocalizedString _exploreChallengesButton;
        [SerializeField] private LocalizedString _joinHashtagButton;
        
        [SerializeField] private LocalizedString _challengePlace1;
        [SerializeField] private LocalizedString _challengePlace2;
        [SerializeField] private LocalizedString _challengePlace3;
        [SerializeField] private LocalizedString _challengePlace4;
        [SerializeField] private LocalizedString _challengePlace5;
        [SerializeField] private LocalizedString _challengePlace6;
        [SerializeField] private LocalizedString _challengePlace7;
        [SerializeField] private LocalizedString _challengePlace8;
        [SerializeField] private LocalizedString _challengePlace9;
        [SerializeField] private LocalizedString _challengePlace10;
        [SerializeField] private LocalizedString _remixFailedInaccessibleAssetsError;

        public string FeedTabForMe => _feedTabForMe;
        public string FeedTabFeatured => _feedTabFeatured;
        public string FeedTabNew => _feedTabNew;
        public string FeedTabFriends => _feedTabFriends;
        public string FeedTabFollowing => _feedTabFollowing;
        
        public string VideoViewsCounterFormat => _videoViewsCounterFormat;
        public string RemixUsedCounterFormat => _remixUsedCounterFormat;
        
        public string RemixVideoHeader => _remixVideoHeader;
        public string RemixStepTwoHeader => _remixStepTwoHeader;
        public string RemixDescription => _remixDescription;
        public string RemixSelectCharactersReasonFormat => _remixSelectCharactersReasonFormat;
        public string RemixFailedInaccessibleAssetsAssetsError => _remixFailedInaccessibleAssetsError;
        
        public string TemplateSelectCharactersTitle => _templateSelectCharactersTitle;
        public string TemplateSelectCharactersReasonFormat => _templateSelectCharactersReasonFormat;
        
        public string TemplateChangePrivacyPopupTitle => _templateChangePrivacyPopupTitle;
        public string TemplateChangePrivacyPopupDescription => _templateChangePrivacyPopupDescription;
        public string TemplateChangePrivacyPopupConfirmButton => _templateChangePrivacyPopupConfirmButton;
        public string TemplateChangePrivacyPopupCancelButton => _templateChangePrivacyPopupCancelButton;
        
        public string TemplateNotFoundSnackbarMessage => _templateNotFoundSnackbarMessage;
        public string TemplateNotAvailableSnackbarMessage => _templateNotAvailableSnackbarMessage;
       
        public string TemplatePrivacyUpdateSuccessSnackbarMessage => _templatePrivacyUpdateSuccessSnackbarMessage;
        public string TemplatePrivacyUpdateFailedSnackbarMessage => _templatePrivacyUpdateFailedSnackbarMessage;
        
        public string TemplateGenerationSuccessSnackbarMessage => _templateGenerationSuccessSnackbarMessage;
        public string TemplateGenerationFailedSnackbarMessage => _templateGenerationFailedSnackbarMessage;
        
        public string TemplateEditOption => _templateEditOption;
        public string TemplateRenameCompletedSnackbarMessage => _templateRenameCompletedSnackbarMessage;
        
        public string CharacterSelectorCategoryMyFrevers => _characterSelectorCategoryMyFrevers;
        public string CharacterSelectorCategoryFriends => _characterSelectorCategoryFriends;
        public string CharacterSelectorCategoryStarCreator => _characterSelectorCategoryStarCreator;

        public string DeleteVideoPopupTitle => _deleteVideoPopupTitle;
        public string DeleteVideoPopupDescription => _deleteVideoPopupDescription;
        public string DeleteVideoPopupUploadedOrMessageDescription => _deleteVideoPopupUploadedOrMessageDescription;
        public string DeleteVideoPopupTemplateDescription => _deleteVideoPopupTemplateDescription;
        public string DeleteVideoPopupConfirmButton => _deleteVideoPopupConfirmButton;
        public string DeleteVideoPopupCancelButton => _deleteVideoPopupCancelButton;
       
        public string VideoPrivacyPopupTitle => _videoPrivacyPopupTitle;
        public string VideoPrivacyPopupDescription => _videoPrivacyPopupDescription;
        public string VideoPrivacyPopupConfirmButton => _videoPrivacyPopupConfirmButton;
        public string VideoPrivacyPopupCancelButton => _videoPrivacyPopupCancelButton;
        
        public string VideoPrivacyUpdateSuccessSnackbarMessageFormat => _videoPrivacyUpdateSuccessSnackbarMessageFormat;
        public string VideoPrivacyUpdateFailedSnackbarMessage => _videoPrivacyUpdateFailedSnackbarMessage;
        
        public string ReportVideoFailedSnackbarMessage => _reportVideoFailedSnackbarMessage;
        
        public string ReportVideoSuccessPopupTitle => _reportVideoSuccessPopupTitle;
        public string ReportVideoSuccessPopupDescription => _reportVideoSuccessPopupDescription;
        public string ReportVideoSuccessPopupConfrimButton => _reportVideoSuccessPopupConfrimButton;
        
        public string FollowingUserSnackbarMessageFormat => _followingUserSnackbarMessageFormat;
        public string FriendsWithUserSnackbarMessageFormat => _friendsWithUserSnackbarMessageFormat;
        
        public string VideoSaveSuccessSnackbarMessage => _videoSaveSuccessSnackbarMessage;
        public string VideoSaveFailedSnackbarMessage => _videoSaveFailedSnackbarMessage;
        
        public string VideoDeletedSnackbarMessage => _videoDeletedSnackbarMessage;
        public string VideoDeleteLoadingTitle => _videoDeleteLoadingTitle;
        
        public string VideoShareSuccessSnackbarMessage => _videoShareSuccessSnackbarMessage;
        public string VideoShareFailedSnackbarMessage => _videoShareFailedSnackbarMessage;
        
        public string UseTemplateButton => _useTemplateButton;
        public string JoinChallengeButton => _joinChallengeButton;
        public string JoinStyleChallengeButton => _joinStyleChallengeButton;
        public string ChallengeResultsButton => _challengeResultsButton;
        public string ExploreChallengesButton => _exploreChallengesButton;
        public string JoinHashtagButton => _joinHashtagButton;
        
        public string ChallengePlace1 => _challengePlace1;
        public string ChallengePlace2 => _challengePlace2;
        public string ChallengePlace3 => _challengePlace3;
        public string ChallengePlace4 => _challengePlace4;
        public string ChallengePlace5 => _challengePlace5;
        public string ChallengePlace6 => _challengePlace6;
        public string ChallengePlace7 => _challengePlace7;
        public string ChallengePlace8 => _challengePlace8;
        public string ChallengePlace9 => _challengePlace9;
        public string ChallengePlace10 => _challengePlace10;
    }
}