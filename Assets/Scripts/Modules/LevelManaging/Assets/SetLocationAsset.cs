using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BoatAttack;
using Bridge.Models.ClientServer.Assets;
using Cinemachine;
using Common;
using Extensions;
using Modules.LevelManaging.Assets.AssetDependencies.AssetSceneMovers;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Modules.LevelManaging.Assets
{
    public interface ISetLocationAsset: IAsset<SetLocationFullInfo>
    {
        string SceneName { get; }
        Scene Scene { get; }
        bool IsDayTimeControlSupported { get; }
        CharacterSpawnPositionMonoBehaviour[] CharacterSpawnPositions { get; }
        UserPhotoVideoPlayer MediaPlayer { get; }
        DayNightController2 DayNightController { get; }
        PictureInPictureController PictureInPictureController { get; }
        Camera Camera { get; }
        int PlaybackTimeMs { get; }
        float VideoPlaybackTime { get; }
        Stopwatch StopWatch { get; }
        CinemachineBrain CinemachineBrain { get; }
        
        void Attach(long spawnPositionId, params IAttachableAsset[] assets);
        void Attach(CharacterSpawnPositionInfo spawnPosition, params IAttachableAsset[] assets);
        void ResetPosition(IAttachableAsset[] attachedAsset);
        void ResetPosition(IAttachableAsset attachedAsset);
        void Detach(IAttachableAsset assets);
        Transform GetCharacterSpawnPositionTransform(Guid guid);
        bool IsAttached(IAttachableAsset asset);
        void SetPictureInPictureRenderScale(float renderScale);
    }

    internal sealed class SetLocationAsset : RepresentationAsset<SetLocationFullInfo>, ISetLocationAsset
    {
        private const float LIGHT_INTENSITY_CONVERSION_FACTOR = 2f;

        private SetLocationRoot _setLocationRoot;
        private readonly Dictionary<IAttachableAsset, Guid> _attachedAssetsData = new();
        private AssetAttachingControlProvider _assetAttachingControlProvider;
        private Scene _scene;
        private LayerMask _pictureInPictureLayer;
        private float _pictureInPictureRenderScale;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override DbModelType AssetType => DbModelType.SetLocation;
        public string SceneName => _scene.name;
        public Scene Scene => _scene;
        public bool IsDayTimeControlSupported { get; private set; }
        public CharacterSpawnPositionMonoBehaviour[] CharacterSpawnPositions { get; private set; }
        public UserPhotoVideoPlayer MediaPlayer => _setLocationRoot.PhotoVideoPlayer;
        public DayNightController2 DayNightController { get; private set; }
        public PictureInPictureController PictureInPictureController => _setLocationRoot.PictureInPictureController;
        public Camera Camera { get; private set; }
        public int PlaybackTimeMs => (int) StopWatch.ElapsedMilliseconds;
        public float VideoPlaybackTime => (float)MediaPlayer.VideoPlayer.clockTime; 
        public Stopwatch StopWatch { get; private set; }
        public CinemachineBrain CinemachineBrain { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(SetLocationFullInfo represent, Scene scene, AssetAttachingControlProvider assetAttachingControlProvider, LayerMask pictureInPictureLayerMask, float pictureInPictureRenderScale)
        {
            StopWatch = new Stopwatch();
            BasicInit(represent);
            _scene = scene;
            FindSetLocationRoot();
            SetupCamera();
            DisableAudioListener();
            
            StopWatch.Start();
            SetupCharacterSpawnPositions();
            SetupDayNightController();
            _assetAttachingControlProvider = assetAttachingControlProvider;
            InitMediaPlayer();
            _pictureInPictureLayer = pictureInPictureLayerMask;
            InitPictureInPictureController(pictureInPictureLayerMask, pictureInPictureRenderScale);
        }

        public void Attach(long spawnPositionId, params IAttachableAsset[] assets)
        {
            var spawnPositionModel = RepresentedModel.GetSpawnPosition(spawnPositionId);
            if (spawnPositionModel == null)
            {
                throw new InvalidOperationException(
                    $"Spawn point {spawnPositionId} does not exist on set location: {RepresentedModel.Name}");
            }
            Attach(spawnPositionModel, assets);
        }
        
        public void Attach(CharacterSpawnPositionInfo spawnPosition, params IAttachableAsset[] assets)
        {
            if (CharacterSpawnPositions.All(x => x.Guid != spawnPosition.UnityGuid))
            {
                throw new InvalidOperationException("You are trying to attach objects to spawn position which does not exist on this setlocation.");
            }
            if (assets.Length == 0)
            {
                Debug.LogWarning("You are trying to attach 0 objects on spawn position");
                return;
            }
            
            foreach (var asset in assets)
            {
                ValidateAttachingObject(asset);
                
                if (IsAttachedToSpawnPosition(asset, spawnPosition.UnityGuid)) continue;

                if (IsAttached(asset))
                {
                    Detach(asset);
                }

                Attach(asset, spawnPosition);
            }
        }

        public void ResetPosition(IAttachableAsset[] attachedAssets)
        {
            foreach (var attachedAsset in attachedAssets)
            {
                ResetPosition(attachedAsset);
            }
        }

        public void ResetPosition(IAttachableAsset attachedAsset)
        {
            if (!IsAttached(attachedAsset)) return;
            var spawnPositionGuid = _attachedAssetsData[attachedAsset];
            var spawnPosition = RepresentedModel.GetSpawnPosition(spawnPositionGuid);
            var control = GetAssetAttachingControlForAsset(attachedAsset.AssetType);
            control.ResetPosition(attachedAsset, spawnPosition);
        }

        public void Detach(IAttachableAsset asset)
        {
            ValidateAttachingObject(asset);
            
            if (!IsAttached(asset))
            {
                Debug.LogWarning($"You are trying to detach {asset.GetType().Name} which is not attached to SetLocation");
                return;
            }

            asset.Destroyed -= OnAttachedObjectDestroyed;
            asset.MovedToScene -= OnAssetMovedToScene;

            var attachingControl = GetAssetAttachingControlForAsset(asset.AssetType);
            attachingControl.Detach(asset);
            _attachedAssetsData.Remove(asset);
        }

        public override void PrepareForUnloading()
        {
            DetachAllAssets();
            Camera.targetTexture = null;
            base.PrepareForUnloading();
        }

        public Transform GetCharacterSpawnPositionTransform(Guid guid)
        {
            return CharacterSpawnPositions.First(x => x.Guid == guid).transform;
        }
        
        public bool IsAttached(IAttachableAsset asset)
        {
            return _attachedAssetsData.ContainsKey(asset);
        }

        public void SetPictureInPictureRenderScale(float renderScale)
        {
            if (PictureInPictureController == null || Math.Abs(renderScale - _pictureInPictureRenderScale) < 0.001f) return;
            InitPictureInPictureController(_pictureInPictureLayer, renderScale);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
            var rootObjects = _scene.GetRootGameObjects();

            foreach (var obj in rootObjects)
            {
                if (obj != null && !obj.name.Contains("Inactive"))
                {
                    obj.SetActive(value);
                }
            }

            if (value)
            {
                SceneManager.SetActiveScene(_scene);
                DayNightController?.Run();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Attach(IAttachableAsset asset, CharacterSpawnPositionInfo spawnPositionModel)
        {
            _attachedAssetsData[asset] = spawnPositionModel.UnityGuid;
            asset.Destroyed += OnAttachedObjectDestroyed;
                
            var attachingControl = GetAssetAttachingControlForAsset(asset.AssetType);
            var spawnPosition = CharacterSpawnPositions.First(x => x.Guid == spawnPositionModel.UnityGuid);
            attachingControl.Attach(asset, spawnPositionModel, spawnPosition.transform);
            
            switch (asset)
            {
                case ICharacterAsset characterAsset:
                    UpdateSpawnPositionLights(spawnPositionModel, characterAsset);
                    break;
                case ICaptionAsset captionAsset:
                    captionAsset.SetCamera(Camera);
                    break;
            }

            asset.MovedToScene += OnAssetMovedToScene;
        }
        
        private void DetachAllAssets()
        {
            var assetsToDetach = new List<IAttachableAsset>(_attachedAssetsData.Keys);
            foreach (var asset in assetsToDetach)
            {
                Detach(asset);
            }
        }

        private void UpdateSpawnPositionLights(CharacterSpawnPositionInfo spawnPosition, params ICharacterAsset[] characters)
        {
            var characterSpawnPositionMono = CharacterSpawnPositions.First(x => x.Guid == spawnPosition.UnityGuid);
            if (characterSpawnPositionMono.LightGuidOwnersParent == null)
            {
                return;
            }

            var lightParent = characterSpawnPositionMono.LightGuidOwnersParent;
            var lightSettings = spawnPosition.LightSettings.ToArray();

            lightParent.gameObject.SetActive(false);

            foreach (var character in characters)
            {
                character.ChangeLightSource(lightParent, lightSettings);

                if (DayNightController == null) continue;
                DayNightController.TimeUpdated -= character.UpdateSpawnPositionLightsIntensity;
                DayNightController.TimeUpdated += character.UpdateSpawnPositionLightsIntensity;
            }

            SetupSpawnPositionLightSettings(lightParent.GetGuidOwners(), lightSettings);
        }
        
        private void SetupSpawnPositionLightSettings(LightMonoBehaviour[] lights, LightSettingsInfo[] lightSettings)
        {
            for (var i = 0; i < lights.Length; i++)
            {
                if (ColorUtility.TryParseHtmlString("#" + lightSettings[i].Color, out var color))
                {
                    lights[i].LightComponent.color = color;
                    lights[i].LightComponent.intensity = lightSettings[i].Intensity / 100f;
                }
                else
                {
                    Debug.LogError("Hexadecimal string for light settings color is incorrect.");
                }
            }
        }
        
        private void OnTimeOfDayChanged(float newTime)
        {
            var lightIntensityMultiplier = GetLightIntensityMultiplier(newTime);
            SetSpawnPositionLightsIntensity(lightIntensityMultiplier);
        }

        private float GetLightIntensityMultiplier(float timeOfDay)
        {
            var timeFromMidDay = Mathf.Abs(DayNightController.GetMidDayTime() - timeOfDay);
            var newLightIntensityMultiplier = DayNightController.GetMidDayTime() + timeFromMidDay;
            return newLightIntensityMultiplier * LIGHT_INTENSITY_CONVERSION_FACTOR; 
        }

        private void SetSpawnPositionLightsIntensity(float intensityMultiplier)
        {
            foreach (var spawnPosition in RepresentedModel.CharacterSpawnPosition)
            {
                var spawnPositionComponent = CharacterSpawnPositions.First(x => x.Guid == spawnPosition.UnityGuid);
                var lights = GetLights(spawnPositionComponent);
                if (lights == null) continue;
                
                var lightSetting = GetLightSettings(spawnPositionComponent);
                
                for (var i = 0; i < lights.Length; i++)
                {
                    var lightSettingIntensity = lightSetting[i].Intensity / 100f;
                    lights[i].LightComponent.intensity = lightSettingIntensity * intensityMultiplier;
                }
            } 
        }

        private LightSettingsInfo[] GetLightSettings(CharacterSpawnPositionMonoBehaviour spawnPositionMono)
        {
            var spawnPositions = RepresentedModel.CharacterSpawnPosition;
            var spawnPositionModel = spawnPositions.First(x => x.UnityGuid.Equals(spawnPositionMono.Guid));
            return spawnPositionModel.LightSettings.ToArray();
        }

        private LightMonoBehaviour[] GetLights(CharacterSpawnPositionMonoBehaviour spawnPositionMono)
        {
            var lightOwner = spawnPositionMono.LightGuidOwnersParent;
            return lightOwner?.GetGuidOwners();
        }

        private void FindSetLocationRoot()
        {
            var setLocationRoot = Object.FindObjectsOfType<SetLocationRoot>();
            _setLocationRoot = setLocationRoot.First(x => x.gameObject.scene == _scene);
        }

        private void SetupCamera()
        {
            var rootObjects = _scene.GetRootGameObjects();

            foreach (var rootObject in rootObjects)
            {
                Camera = rootObject.GetComponent<Camera>();
                if (Camera != null) break;
            }

            // We had to disable occlusion culling due to wrong occlusion culling behavior with multiple scene
            // loading in additive mode occlusion data applies from first additive scene to second and leads
            // to wrong rendering
            Camera.useOcclusionCulling = false;
            
            CinemachineBrain = Camera.GetComponent<CinemachineBrain>();
        }

        private void SetupDayNightController()
        {
            //todo: get day night controller in more efficient way [FREV-6698]
            var dayNightControllers = Object.FindObjectsOfType<DayNightController2>();
            var dayNightController = dayNightControllers.FirstOrDefault(x => x.gameObject.scene == _scene);

            IsDayTimeControlSupported = dayNightController != null;
            if (!IsDayTimeControlSupported) return;

            DayNightController = dayNightController;
            DayNightController.TimeUpdated += OnTimeOfDayChanged;
            DayNightController.Speed = 0;
        }

        private void OnAttachedObjectDestroyed(long assetId)
        {
            var asset = _attachedAssetsData.FirstOrDefault(c => c.Key.Id == assetId).Key;

            if (asset == null) return;
            
            asset.Destroyed -= OnAttachedObjectDestroyed;
            _attachedAssetsData.Remove(asset);
        }

        private AssetAttachingControl GetAssetAttachingControlForAsset(DbModelType assetType)
        {
            var attachControl = _assetAttachingControlProvider.GetControl(assetType);
            return attachControl;
        }

        private bool IsAttachedToSpawnPosition(IAttachableAsset asset, Guid spawnPositionGuid)
        {
            return IsAttached(asset) && _attachedAssetsData[asset] == spawnPositionGuid;
        }

        private void SetupCharacterSpawnPositions()
        {
            CharacterSpawnPositions = _setLocationRoot.CharacterGuidOwnersParent.GetGuidOwners();
            foreach (var spawnPosition in CharacterSpawnPositions)
            {
                spawnPosition.LightGuidOwnersParent.SetActive(false);
            }
        }

        private void ValidateAttachingObject(IAttachableAsset asset)
        {
            if (asset is null)
            {
                throw new ArgumentNullException(nameof(asset),
                                             "Asset attaching/detaching from/to is being failed. Asset must not be null");}

            if (!(asset is ISceneObject))
            {
                throw new InvalidOperationException(
                    $"To attach/detach asset from scene it must inherit {nameof(ISceneObject)} interface");
            }
        }

        private void OnAssetMovedToScene(ISceneObject sceneObject, Scene scene)
        {
            var attachableAsset = sceneObject as IAttachableAsset;
            if (!IsAttached(attachableAsset))
            {
                sceneObject.MovedToScene -= OnAssetMovedToScene;
            }
            if (scene == _scene) return;

            _attachedAssetsData.Remove(attachableAsset);
        }
        
        private void InitMediaPlayer()
        {
            if (MediaPlayer == null) return;
            MediaPlayer.Init(Camera);
        }
        
        private void InitPictureInPictureController(LayerMask pictureInPictureLayerMask, float renderScale)
        {
            if (PictureInPictureController == null) return;
            _pictureInPictureRenderScale = renderScale;

            var videoResolution = Constants.VideoRenderingResolution.PORTRAIT_1080;
            var pictureInPictureResolution = new Vector2Int((int)(videoResolution.x * renderScale), (int)(videoResolution.y * renderScale));//increase for looking good on scaling
            PictureInPictureController.Init(pictureInPictureResolution, pictureInPictureLayerMask);
            var urpCameraData = PictureInPictureController.PictureInPictureCamera.GetComponent<UniversalAdditionalCameraData>();
            urpCameraData.SetRenderer(1); //fix the issue with rendering transparency. It's not possible with the default renderer because of the horizontal water plane
        }

        private void DisableAudioListener()
        {
            var audioListener = Camera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
    }
}
