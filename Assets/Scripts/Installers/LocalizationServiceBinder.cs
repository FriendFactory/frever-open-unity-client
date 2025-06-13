using System.IO;
using AppStart;
using Modules.AssetsStoraging.Core;
using UIManaging.Localization;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class LocalizationServiceBinder
    {
        private const string RESOURCES_PATH = "L10N/ScriptableObjects/";
        
        public static void BindLocalizationServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<LocalizationSetup>().FromInstance(AppEntryContext.LocalizationSetup);
            //todo: use RESOURCES_PATH for all cases 
            container.BindInstance(Resources.Load<LoadingOverlayLocalization>("L10N/ScriptableObjects/LoadingOverlayLocalization")).AsSingle();
            container.BindInstance(Resources.Load<LevelEditorAssetSelectorLocalization>("L10N/ScriptableObjects/LevelEditorAssetSelectorLocalization")).AsSingle();
            container.BindInstance(Resources.Load<LevelEditorPopupLocalization>("L10N/ScriptableObjects/LevelEditorPopupLocalization")).AsSingle();
            container.BindInstance(Resources.Load<LevelEditorCameraSettingsLocalization>("L10N/ScriptableObjects/LevelEditorCameraSettingsLocalization")).AsSingle();
            container.BindInstance(Resources.Load<CreatorScorePageLocalization>("L10N/ScriptableObjects/CreatorScorePageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<OnBoardingLocalization>("L10N/ScriptableObjects/OnBoardingLocalization")).AsSingle();
            container.BindInstance(Resources.Load<PostRecordEditorLocalization>("L10N/ScriptableObjects/PostRecordEditorLocalization")).AsSingle();
            container.BindInstance(Resources.Load<CharacterEditorLocalization>("L10N/ScriptableObjects/CharacterEditorLocalization")).AsSingle();
            container.BindInstance(Resources.Load<ShoppingCartLocalization>("L10N/ScriptableObjects/ShoppingCartLocalization")).AsSingle();
            container.BindInstance(Resources.Load<UmaEditorCategoriesLocalization>("L10N/ScriptableObjects/UmaEditorCategoriesLocalization")).AsSingle();
            container.BindInstance(Resources.Load<UmaEditorCharacterParametersLocalization>("L10N/ScriptableObjects/UmaEditorCharacterParametersLocalization")).AsSingle();
            container.BindInstance(Resources.Load<AvatarPageLocalization>("L10N/ScriptableObjects/AvatarPageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<OnboardingServerErrorLocalization>("L10N/ScriptableObjects/OnboardingServerErrorLocalization")).AsSingle();
            container.BindInstance(Resources.Load<NotificationsLocalization>("L10N/ScriptableObjects/NotificationsLocalization")).AsSingle();
            container.BindInstance(Resources.Load<DiscoveryPageLocalization>("L10N/ScriptableObjects/DiscoveryPageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<UserListItemLocalization>("L10N/ScriptableObjects/UserListItemLocalization")).AsSingle();
            container.BindInstance(Resources.Load<HashtagItemViewLocalization>("L10N/ScriptableObjects/HashtagItemViewLocalization")).AsSingle();
            container.BindInstance(Resources.Load<NativeGalleryPermissionPopupLocalization>("L10N/ScriptableObjects/NativeGalleryPermissionPopupLocalization")).AsSingle();
            container.BindInstance(Resources.Load<FeedLocalization>("L10N/ScriptableObjects/FeedLocalization")).AsSingle();
            container.BindInstance(Resources.Load<CommentsLocalization>("L10N/ScriptableObjects/CommentsLocalization")).AsSingle();
            container.BindInstance(Resources.Load<ProfileLocalization>("L10N/ScriptableObjects/ProfileLocalization")).AsSingle();
            container.BindInstance(Resources.Load<DraftGridLocalization>("L10N/ScriptableObjects/DraftGridLocalization")).AsSingle();
            container.BindInstance(Resources.Load<PublishPageLocalization>("L10N/ScriptableObjects/PublishPageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<ShareToPopupLocalization>("L10N/ScriptableObjects/ShareToPopupLocalization")).AsSingle();
            container.BindInstance(Resources.Load<MentionsPanelLocalization>("L10N/ScriptableObjects/MentionsPanelLocalization")).AsSingle();
            container.BindInstance(Resources.Load<CrewPageLocalization>("L10N/ScriptableObjects/CrewPageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<LootboxPopupLocalization>("L10N/ScriptableObjects/LootboxPopupLocalization")).AsSingle();
            container.BindInstance(Resources.Load<ErrorMessageLocalization>("L10N/ScriptableObjects/ErrorMessageLocalization")).AsSingle();
            container.BindInstance(Resources.Load<InviteRewardPopupLocalization>("L10N/ScriptableObjects/InviteRewardPopupLocalization")).AsSingle();
            container.BindInstance(Resources.Load<StorePageLocalization>(Path.Combine(RESOURCES_PATH, nameof(StorePageLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<PurchasesLocalization>(Path.Combine(RESOURCES_PATH, nameof(PurchasesLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<DateTimeLocalization>(Path.Combine(RESOURCES_PATH, nameof(DateTimeLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<SeasonPageLocalization>(Path.Combine(RESOURCES_PATH, nameof(SeasonPageLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<VideoReportReasonMapping>(Path.Combine(RESOURCES_PATH, nameof(VideoReportReasonMapping)))).AsSingle();
            container.BindInstance(Resources.Load<ChatLocalization>(Path.Combine(RESOURCES_PATH, nameof(ChatLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<RatingFeedPageLocalization>(Path.Combine(RESOURCES_PATH, nameof(RatingFeedPageLocalization)))).AsSingle();
            container.BindInstance(Resources.Load<TemplateCharacterSelectionLocalization>(Path.Combine(RESOURCES_PATH, nameof(TemplateCharacterSelectionLocalization)))).AsSingle();
        }
    }
}