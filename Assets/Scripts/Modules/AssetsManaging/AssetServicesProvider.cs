using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AssetBundleLoaders;
using Bridge.Models.Common;
using Bridge;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Loaders.DependencyLoading;
using Modules.AssetsManaging.LoadingProfiles;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.AssetsManaging.Unloaders;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraAnimations;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Recording.Core;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.MusicCacheManaging;

namespace Modules.AssetsManaging
{
    [UsedImplicitly]
    internal sealed class AssetServicesProvider
    {
        public readonly IReadOnlyCollection<DbModelType> SupportedTypes;

        private readonly IReadOnlyDictionary<DbModelType, IAssetLoadProfile> _assetServiceProviders;

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
        public AssetServicesProvider(IBridge bridge, CharacterViewContainer characterViewContainer,
                                     IAssetBundleLoader assetBundleLoader, AudioSourceManager audioSourceManager,
                                     UncompressedBundlesManager uncompressedBundlesManager,
                                     FaceAnimationConverter faceAnimationConverter, FaceBlendShapeMap faceBlendShapeMap,
                                     CameraAnimationConverter cameraAnimationConverter, ISceneObjectHelper sceneObjectHelper,
                                     CaptionLoadProfile captionLoadProfile, ILicensedMusicProvider licensedMusicProvider,
                                     ISetLocationBackgroundInMemoryCache setLocationBackgroundInMemoryCache)
        {
            _assetServiceProviders = new Dictionary<DbModelType, IAssetLoadProfile>
            {
                {DbModelType.BodyAnimation, new BodyAnimationLoadProfile(bridge, assetBundleLoader)},
                {DbModelType.Character, new CharacterLoadProfile(bridge, characterViewContainer, faceBlendShapeMap)},
                {DbModelType.Song, new SongLoadProfile(bridge, audioSourceManager)},
                {DbModelType.Vfx, new VfxLoadProfile(bridge, assetBundleLoader, sceneObjectHelper)},
                {DbModelType.CameraAnimation, new CameraAnimationLoadProfile(bridge, cameraAnimationConverter)},
                {DbModelType.FaceAnimation, new FaceAnimationLoadProfile(bridge, faceAnimationConverter)},
                {DbModelType.SetLocation, new SetLocationLoadProfile(bridge, assetBundleLoader, uncompressedBundlesManager, sceneObjectHelper)},
                {DbModelType.UserPhoto, new PhotoLoadProfile(bridge)},
                {DbModelType.SetLocationBackground, new SetLocationBackgroundLoadProfile(bridge, setLocationBackgroundInMemoryCache)},
                {DbModelType.VideoClip, new VideoClipLoadProfile(bridge)},
                {DbModelType.VoiceTrack, new VoiceTrackLoadProfile(bridge)},
                {DbModelType.CameraFilterVariant, new CameraFilterVariantLoadProfile(bridge, assetBundleLoader, sceneObjectHelper)},
                {DbModelType.UserSound, new UserSoundLoadProfile(bridge, audioSourceManager) },
                {DbModelType.ExternalTrack, new ExternalTrackLoadProfile(bridge, audioSourceManager, licensedMusicProvider) },
                {DbModelType.Caption, captionLoadProfile}
            };

            SupportedTypes = _assetServiceProviders.Keys.ToArray();
        }
        
        public AssetLoader<TEntity, TArgs> GetAssetLoader<TEntity, TArgs>()
            where TEntity: IEntity where TArgs: LoadAssetArgs<TEntity>
        {
            var servicesProvider = GetAssetServicesProvider<TEntity>();
            return (servicesProvider as ILoaderProvider<TEntity, TArgs>).GetAssetLoader();
        }

        public DependencyLoader<TEntity, TArgs> GetDependencyLoader<TEntity, TArgs>()
            where TEntity : IEntity where TArgs : LoadAssetArgs<TEntity>
        {
            var servicesProvider = GetAssetServicesProvider<TEntity>();
            return (servicesProvider as ILoaderProvider<TEntity, TArgs>).GetDependencyLoader();
        }

        public AssetUnloader GeAssetUnloader(DbModelType modelType)
        {
            var servicesProvider = GetAssetServicesProvider(modelType);
            return (servicesProvider as IUnloaderProvider).GetUnloader();
        }
        
        public LoadAssetArgs<TEntity> GetDefaultLoadArgs<TEntity>() where TEntity: IEntity
        {
            var servicesProvider = GetAssetServicesProvider<TEntity>();
            return (servicesProvider as IDefaultLoadArgsProvider<TEntity>).GetDefaultLoadArgs();
        }

        private IAssetLoadProfile GetAssetServicesProvider<TEntity>() where TEntity: IEntity
        {
            var dbModelType = DbModelExtensions.GetModelType<TEntity>();
            return GetAssetServicesProvider(dbModelType);
        }
        
        private IAssetLoadProfile GetAssetServicesProvider(DbModelType modelType)
        {
            return _assetServiceProviders[modelType];
        }
    }
}