using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Development;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.CameraManaging.CameraSettingsManaging;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class SetLocationChangingAlgorithm : BaseChanger
    {
        private readonly IAssetManager _assetManager;
        private readonly ICameraSystem _cameraSystem;
        private readonly CameraSettingProvider _cameraSettingProvider;
        private readonly BodyAnimationForSpawnPositionLoader _bodyAnimationForSpawnPositionLoader;
        private readonly CharacterSpawnPointChangingAlgorithm _characterSpawnPointChangingAlgorithm;
        private readonly BodyAnimationModelsLoader _bodyAnimationModelsLoader;
        private readonly ILayerManager _layerManager;
        
        private SetLocationChanged _onCompleted;
        private ISetLocationAsset _previousSetLocation;
        private SetLocationFullInfo _targetSetLocation;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private IBodyAnimationSelector BodyAnimationSelector { get; set; }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SetLocationChangingAlgorithm(IAssetManager assetManager, ICameraSystem cameraSystem, 
            CameraSettingProvider cameraSettingProvider, BodyAnimationForSpawnPositionLoader bodyAnimationForSpawnPositionLoader, 
            CharacterSpawnPointChangingAlgorithm characterSpawnPointChangingAlgorithm, BodyAnimationModelsLoader bodyAnimationModelsLoader, ILayerManager layerManager)
        {
            _assetManager = assetManager;
            _cameraSystem = cameraSystem;
            _cameraSettingProvider = cameraSettingProvider;
            _bodyAnimationForSpawnPositionLoader = bodyAnimationForSpawnPositionLoader;
            _characterSpawnPointChangingAlgorithm = characterSpawnPointChangingAlgorithm;
            _bodyAnimationModelsLoader = bodyAnimationModelsLoader;
            _layerManager = layerManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetBodyAnimationSelector(IBodyAnimationSelector bodyAnimationSelector)
        {
            BodyAnimationSelector = bodyAnimationSelector;
        }
        
        public async void Run(Event currentEvent, SetLocationFullInfo nextSetLocationModel, long targetSpawnPositionId, bool applyRecommendedAnimations, bool keepFormation, SetLocationChanged onCompleted)
        {
            if (_previousSetLocation == null)
            {
                _previousSetLocation = _assetManager.GetActiveAssets<ISetLocationAsset>().First();
            }

            var alreadyLoaded = _previousSetLocation.RepresentedModel.Id == nextSetLocationModel.Id;
            if (alreadyLoaded)
            {
                onCompleted?.Invoke(_previousSetLocation);
                return;
            }
            
            CancelAll();
            _bodyAnimationForSpawnPositionLoader.Cancel();
            _bodyAnimationForSpawnPositionLoader.Reset();
            _targetSetLocation = nextSetLocationModel;
            _onCompleted = onCompleted;

            InvokeAssetStartedUpdating(DbModelType.SetLocation, nextSetLocationModel.Id);
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Token.Register(() =>
            {
                _bodyAnimationForSpawnPositionLoader.Cancel();
                OnFail("Operation canceled", nextSetLocationModel.Id, DbModelType.SetLocation);
            });
            CancellationSources.Add(nextSetLocationModel.Id, cancellationSource);
            LoadSetLocationAsset(nextSetLocationModel, asset => OnSetLocationLoaded(currentEvent, nextSetLocationModel, asset, targetSpawnPositionId, applyRecommendedAnimations, keepFormation), cancellationSource);

            if (!applyRecommendedAnimations) return;
            
            var applyData = BodyAnimationSelector.Select();
            if (applyData.CharacterToBodyAnimation.Any())
            {
                var animationIds = applyData.CharacterToBodyAnimation.Values.ToArray();
                var animations = await _bodyAnimationModelsLoader.Load(animationIds, cancellationSource.Token);
                _bodyAnimationForSpawnPositionLoader.LoadAnimations(animations, cancellationSource.Token);
            }
        }

        public void Reset()
        {
            _previousSetLocation = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void LoadSetLocationAsset(SetLocationFullInfo targetModel, Action<ISetLocationAsset> onLoaded ,CancellationTokenSource cancellationSource)
        {
            var args = new SetLocationLoadArgs
            {
                DeactivateOnLoad = true,
                CancellationToken = cancellationSource.Token,
                PictureInPictureLayerMask = _layerManager.GetCharacterLayers()
            };

            _assetManager.Load(
                targetModel, args,
                asset => onLoaded(asset as ISetLocationAsset),
                message => OnFail(message, targetModel.Id, DbModelType.SetLocation));
        }

        private async void OnSetLocationLoaded(Event targetEvent, SetLocationFullInfo nextLocation, ISetLocationAsset setLocationAsset, long nextSpawnPointId, bool applyAnimations, bool keepFormation)
        {
            CancellationSources.Remove(setLocationAsset.Id);
            
            if (nextLocation.Id != _targetSetLocation.Id)
            {
                setLocationAsset.SetActive(false);
                _assetManager.Unload(setLocationAsset);
                OnCompleted(setLocationAsset);
                return;
            }

            if (applyAnimations)
            {
                while (_bodyAnimationForSpawnPositionLoader.IsLoading)
                {
                    await Task.Delay(100);
                }
            }

            targetEvent.SetSetLocation(nextLocation);
            targetEvent.ResetSetLocationCue();
            if (!nextLocation.AllowPhoto)
            {
                targetEvent.SetPhoto(null);
            }
            
            if (!nextLocation.AllowVideo)
            {
                targetEvent.SetVideo(null);
            }
            
            SetupSetLocationDayTime(targetEvent, setLocationAsset);
            
            DevUtils.ApplyShadersWorkaroundForWinEditor(setLocationAsset.SceneName);

            var spawnPosModel = nextLocation.GetSpawnPosition(nextSpawnPointId);
            _characterSpawnPointChangingAlgorithm.SetBodyAnimationSelector(BodyAnimationSelector);
            await _characterSpawnPointChangingAlgorithm.RunAsync(spawnPosModel, targetEvent, applyAnimations, keepFormation);
            
            setLocationAsset.SetActive(true);
            
            if (_previousSetLocation != null)
            {
                _previousSetLocation.SetActive(false);
                _assetManager.Unload(_previousSetLocation);
            }

            _previousSetLocation = setLocationAsset;
            UpdateCameraSettings(setLocationAsset, nextLocation.GetSpawnPosition(nextSpawnPointId));
            InvokeAssetUpdated(DbModelType.SetLocation);
            OnCompleted(setLocationAsset);
        }

        private void UpdateCameraSettings(ISetLocationAsset setLocation, CharacterSpawnPositionInfo characterSpawnPosition)
        {
            var spaceSizeId = characterSpawnPosition.SpawnPositionSpaceSizeId;
            var cameraSetting = _cameraSettingProvider.GetSettingWithSpawnPositionId(spaceSizeId);
            _cameraSystem.SetCameraComponents(setLocation.Camera, setLocation.CinemachineBrain);
            _cameraSystem.ChangeCameraSetting(cameraSetting);
            _cameraSystem.RecenterFocus();
        }
        
        private void SetupSetLocationDayTime(Event targetEvent ,ISetLocationAsset setLocationAsset)
        {
            if (!setLocationAsset.IsDayTimeControlSupported) return;
            targetEvent.GetSetLocationController().TimeOfDay = setLocationAsset.DayNightController.StartTime.ToMilli();
        }

        private void OnCompleted(ISetLocationAsset setLocationAsset)
        {
            if (_bodyAnimationForSpawnPositionLoader.HasLoadedAnimations)
            {
                _onCompleted?.Invoke(setLocationAsset, DbModelType.BodyAnimation);
            }
            else
            {
                _onCompleted?.Invoke(setLocationAsset);
            }

            _onCompleted = null;
        }
    }
}