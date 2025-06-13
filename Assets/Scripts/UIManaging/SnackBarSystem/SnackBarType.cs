namespace UIManaging.SnackBarSystem
{
    public enum SnackBarType
    {
        
        Undefined = 0,
        Information = 1,
        InformationDark = 2,
        AssetLoading = 3,
        VideoPublished = 4,
        SeasonLikes = 5,
        Success = 6,
        PurchaseSuccess = 7,
        PurchaseFailed = 8,
        AssetClaimed = 9,
        // do not know the reason why we keep dark and the old one version of snackbar after design update
        SuccessDark = 10,
        InviterFollowed = 11,
        StyleBattleResultCompleted = 12,
        Fail = 13,
        InformationShort = 14,
        VideoSharedToChat = 15,
        OnboardingSeasonLikes = 16,
        MessagesLocked = 17,
        TransferCrewOwnership = 100,
    }
}