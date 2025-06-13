using Common;
using Common.ModelsMapping;
using Common.TimeManaging;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using Modules.LevelManaging.Editing.CameraManaging;
using Modules.LevelManaging.Editing.CameraManaging.CameraSettingsManaging;
using Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation;
using Modules.LevelManaging.Editing.EventRecording;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.LevelPreview;
using Modules.LevelManaging.Editing.LevelSaving;
using Modules.LevelManaging.Editing.Players;
using Modules.LevelManaging.Editing.Players.AssetPlayerProfiles;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking;
using Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms;
using Modules.LevelManaging.Editing.Players.PreviewSplitting;
using Modules.LevelManaging.Editing.Templates;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.Pages.PublishPage;
using UnityEngine;
using Zenject;
using Context = Modules.LevelManaging.Editing.LevelManagement.Context;

namespace Installers
{
    internal static class LevelManagerServicesBinder
    {
        private const string SPAWN_FORMATION_CAMERA_SETUP_PATH = "ScriptableObjects/Spawn Formation Angles Setup";

        private static int AverageSetLocationRamUsage
        {
            get
            {
                #if UNITY_IOS
                return Constants.Memory.RESERVED_MEMORY_PER_SETLOCATION_MB_IOS;
                #elif UNITY_ANDROID
                return Constants.Memory.RESERVED_MEMORY_PER_SETLOCATION_MB_ANDROID;
                #endif
            }
        }

        public static void BindLevelManager(this DiContainer container, ICameraFocusAnimationCurveProvider cameraFocusAnimationCurveProvider)
        {
            container.BindInterfacesTo<LevelManager>().AsSingle();
            container.BindInternalServices();
            container.BindLevelPreviewAssetLoaders();
            container.BindInterfacesTo<LevelHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelAssetsUnCompressingService>().AsSingle();
            container.Bind<ICameraFocusAnimationCurveProvider>().FromInstance(cameraFocusAnimationCurveProvider).AsSingle();
        }

        private static void BindInternalServices(this DiContainer container)
        {
            container.BindAssetPlayerRelatedServices();
            container.BindAssetChangers();
            container.BindFormationTypes();
            container.BindTemplatesServices();
            container.BindAssetCuesManagers();
            container.BindFormationCameraControls();

            container.BindInterfacesAndSelfTo<EventAssetsProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<NotUsedAssetsUnloader>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraFocusManager>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnchorManager>().AsSingle();
            container.BindInterfacesAndSelfTo<EventSetupAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<AssetPlayStateOnEventSwitchingManager>().AsSingle();
            container.BindInterfacesAndSelfTo<ReusedAssetsAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<EventRecorder>().AsSingle();
            container.BindInterfacesAndSelfTo<EventTimeSourceControl>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationPlayer>().AsTransient();
            container.BindInterfacesAndSelfTo<EventEditor>().AsSingle();
            container.BindInterfacesAndSelfTo<LayerManager>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraSettingProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationGenerator>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationSimulator>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationFrameProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<PreviousEventLastCameraFrameProvider>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelSaver>().AsSingle();
            container.BindInterfacesAndSelfTo<DefaultEvenThumbnailService>().AsSingle();
            container.BindInterfacesAndSelfTo<MusicApplier>().AsSingle();
            container.BindInterfacesAndSelfTo<Context>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationGroupProvider>().AsSingle();
            container.BindInterfacesTo<AudioRecordingStateHolder>().AsSingle();
            container.BindInterfacesAndSelfTo<WatermarkControl>().AsSingle();

            container.BindTimeServices();
            container.BindModelsMapping();
        }

        private static void BindAssetPlayerRelatedServices(this DiContainer container)
        {
            container.BindAssetPlayerProfiles();
            
            container.Bind<IPreviewManager>().To<PreviewManager>().AsSingle();
            container.Bind<IEventPlayControl>().To<EventPlayControl>().AsSingle();
            container.Bind<ILevelPlayControl>().To<LevelPlayControl>().AsSingle();
            
            container.BindInterfacesAndSelfTo<PreRecordingPlayingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<RecordingPlayingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<PreviewPlayingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<LoopEventPreviewPlayingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<OverrideCameraAnimationByTemplateEventPreview>().AsSingle();
            container.BindInterfacesAndSelfTo<StayOnFirstFramePlayingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelPreviewAssetsLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelEditorEventAssetsLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelPreviewProgressCounter>().AsSingle();
            container.BindInterfacesAndSelfTo<PublishVideoPopupManager>().AsSingle();

            container.Bind<PreviewSplitter>().AsSingle()
                            .WithArguments(
                                AverageSetLocationRamUsage,
                                Constants.Memory.RESERVED_MEMORY_PER_CHARACTER_MB,
                                Constants.Memory.RESERVED_MEMORY_BY_UNITY_MB,
                                Constants.Memory.RESERVED_MEMORY_FOR_UMA_BUILD_PROCESS_MB);
        }

        private static void BindAssetPlayerProfiles(this DiContainer container)
        {
            container.BindInterfacesTo<PlayersManager>().AsSingle();

            container.BindInterfacesTo<FaceAnimationPlayerProfile>().AsSingle();
            container.BindInterfacesTo<BodyAnimationPlayerProfile>().AsSingle();
            container.BindInterfacesTo<SetLocationPlayerProfile>().AsSingle();
            container.BindInterfacesTo<VfxPlayerProfile>().AsSingle();
            container.BindInterfacesTo<CameraAnimationPlayerProfile>().AsSingle();
            container.BindInterfacesTo<UserSoundPlayerProfile>().AsSingle();
            container.BindInterfacesTo<VoiceTrackPlayerProfile>().AsSingle();
            container.BindInterfacesTo<SongPlayerProfile>().AsSingle();
            container.BindInterfacesTo<CharacterPlayerProfile>().AsSingle();
            container.BindInterfacesTo<CameraFilterVariantPlayerProfile>().AsSingle();
            container.BindInterfacesTo<VideoPlayerProfile>().AsSingle();
            container.BindInterfacesTo<PhotoPlayerProfile>().AsSingle();
            container.BindInterfacesTo<SetLocationBackgroundPlayerProfile>().AsSingle();
            container.BindInterfacesTo<CaptionPlayerProfile>().AsSingle();
            container.BindInterfacesTo<ExternalTrackPlayerProfile>().AsSingle();
            
            container.BindInterfacesAndSelfTo<AnimatorMonitorProvider>().AsSingle();
        }

        private static void BindTemplatesServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<EventTemplateManager>().AsSingle();

            container.BindInterfacesAndSelfTo<EventDataTemplateBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationControllerBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<MusicControllerBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<VfxControllerBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraFilterControllerBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<CharacterControllerBuildStep>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraControllerBuildStep>().AsSingle();
            
            container.BindInterfacesAndSelfTo<TemplatesContainer>().AsSingle();
        }

        private static void BindAssetCuesManagers(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ActivationCueManager>().AsSingle();
            
            container.BindInterfacesAndSelfTo<SetLocationCueManager>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationCueProvider>().AsSingle();
            
            container.BindInterfacesAndSelfTo<VideoCueManager>().AsSingle();
            container.BindInterfacesAndSelfTo<VideoCueProvider>().AsSingle();
            
            container.BindInterfacesAndSelfTo<VfxCueManager>().AsSingle();
            container.BindInterfacesAndSelfTo<VfxCueProvider>().AsSingle();
            
            container.BindInterfacesAndSelfTo<AudioCueManager>().AsSingle();
            container.BindInterfacesAndSelfTo<MusicCueProvider>().AsSingle();
            
            container.BindInterfacesAndSelfTo<BodyAnimationCueManager>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationCueProvider>().AsSingle();
        }

        private static void BindAssetChangers(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CharacterSpawnFormationChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<SetLocationChangingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationForSpawnPositionLoader>().AsTransient();
            container.BindInterfacesAndSelfTo<BodyAnimationForNewCharacterLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<VfxChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<AudioChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<DefaultBodyAnimationForSpawnPositionLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<CharacterRemovingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraFilterVariantChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<CharactersChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<CharacterSpawnPointChangingAlgorithm>().AsSingle();
            container.BindInterfacesAndSelfTo<FaceAnimationChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<VoiceTrackChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<OutfitChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<CameraAnimationTemplateChanger>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationForChangingSpawnPositionSelector>().AsSingle();
            container.BindInterfacesAndSelfTo<ShuffleAnimationsSelector>().AsSingle();
            container.BindInterfacesAndSelfTo<BodyAnimationModelsLoader>().AsSingle();
        }

        private static void BindFormationTypes(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<SpawnFormationProvider>().AsSingle();
            
            container.BindInterfacesAndSelfTo<SingleFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<DuoForwardFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<DuoStoryFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<DuoDuelFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<TrioLineFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<TrioStoryFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<TrioDanceFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<TrioDuelFormation>().AsSingle();
            container.BindInterfacesAndSelfTo<TrioQueueFormation>().AsSingle();
        }

        private static void BindFormationCameraControls(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<CameraSpawnFormationControl>().AsSingle();

            var spawnFormationSetup = LoadSpawnFormationCameraSetup();
            container.BindInterfacesAndSelfTo<SpawnFormationCameraAngleProvider>().AsSingle().WithArguments(spawnFormationSetup.FormationSetups, spawnFormationSetup.DefaultAngle);
        }

        private static SpawnFormationCameraSetup LoadSpawnFormationCameraSetup()
        {
            return Resources.Load<SpawnFormationCameraSetup>(SPAWN_FORMATION_CAMERA_SETUP_PATH);
        }
        
        private static void BindLevelPreviewAssetLoaders(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<EventSetLocationLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventCharacterLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventFaceAnimationLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventVoiceTrackLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventBodyAnimationLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventVfxLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventSongLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventUserSoundLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventExternalTrackLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventCameraAnimationLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventCameraFilterVariantLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventUserPhotoLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventVideoClipLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventCaptionLoader>().AsSingle();
            container.BindInterfacesAndSelfTo<EventSetLocationBackgroundLoader>().AsSingle();
        }

        private static void BindTimeServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<StopWatch>().AsSingle();
            container.BindInterfacesAndSelfTo<UnityTimeBasedTimeSource>().AsSingle();
            container.BindInterfacesAndSelfTo<AudioTimeSource>().AsSingle();
        }

        private static void BindModelsMapping(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<Mapper>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelToLevelFullInfoMapper>().AsSingle();
            container.BindInterfacesAndSelfTo<LevelFullDataToLevelMapper>().AsSingle();
        }
    }
}