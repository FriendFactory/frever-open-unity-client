namespace Navigation.Core
{
    public enum PageId
    {
        // Ordinals here have a step in one hundred intentionally. Enums are serialized in Unity by their ordinal
        // and not the name. In this way, if you need to add some value in between, you don't need to change ordinals
        // for existing ones and it won't break serialization in this case.
        
        None = -1, // For disabling old tips without removing it
        
        StartupLoadingPage = 110,
        UpdateAppPage = 150,
        NoConnectionPage = 175,
        UserProfile = 200,
        LevelEditor = 300,
        PostRecordEditor = 350,
        PublishPage = 400,
        UserSelectionPage = 410,
        UmaEditor = 500,
        UmaEditorNew = 510,
        AvatarPage = 600,
        DraftsPage = 700,
        AvatarCreation = 800,
        AvatarSelfie = 900,
        AvatarPreview = 1000,
        Feed = 1100,
        GamifiedFeed = 1101,

        EditProfile = 1200,
        EditUsername = 1210,
        EditBio = 1220,
        EditBioLinks = 1230,

        AppSettings = 1300,
        RaceSelection = 1350,
        GenderSelection = 1400,
        StyleSelection = 1500,
        CharacterStyleSelection = 1510,
        GeneralDataProtection = 1600,
        FollowersPage = 1700,
        DiscoveryPage = 1800,
        OnBoardingPage = 1900,
        OnBoardingScrollSelectorPage = 2000,
        OnBoardingTextInputPage = 2100,
        OnBoardingToggleSelectorPage = 2200,
        PreRemixPage = 2300,
        ReportPage = 2400,
        NotificationPage = 2500,
        ContactsPage = 2700,
        DiscoverPeoplePage = 2800,
        StarCreatorDiscoverPeople = 2801,
        StarCreatorAcceptedInvitations = 2802,
        ManageAccountPage = 3300,
        PublishGalleryVideoPage = 3400,
        OnBoardingUsernamePage = 3500,
        OnBoardingTemplateSelection = 3600,
        OnBoardingPhoneNumberPage = 3700,
        VideosBasedOnTemplatePage = 3800,
        BlockedAccountsPage = 3900,
        ProfilePhotoPreview = 4000,
        PrivacySettings = 4010,
        ProfilePhotoEditor = 4100,
        ProfilePhotoPostEditor = 4200,
        TasksPage = 4300,
        TaskDetails = 4400,
        TaskVideoGrid = 4410,
        SeasonInfo = 4500,
        HomePage = 4600,
        HomePageSimple = 4650,
        CreateFrever = 4700,
        AdvancedSettings = 4800,
        CreatePost = 4900,

        OnboardingPublishPage = 5001,
        OnboardingCharacterResult = 5012,
        OnboardingTasksPage = 5020,
        OnboardingStarCreatorsPage = 5030,
        OnboardingContactsPage = 5040,
        SignUp = 5050,
        VerificationPage = 5060,

        StyleBattleStart = 5100,
        SubmitAndVote = 5110,
        VotingDone = 5120,
        VotingResult = 5130,
        VotingFeed = 5140,
        
        CreatorScore = 5200,
        EditTemplate = 5300,
        EditTemplateFeed = 5400,
        ExternalLinks = 5500,
        MusicSelection = 5600,
        Inbox = 5700,
        
        VideoMessage = 5800,
        
        AboutCrew = 6100,
        CrewInfo = 6200,
        CrewSearch = 6300,
        ChatPage = 6600,
        CrewPage = 6000,
        CrewCreate = 7000,
        
        SavedSounds = 7100,
        VideosBasedOnSound = 7110,
        
        VerificationMethodUpdate = 8000,

        RatingFeed = 9000,
    }
}