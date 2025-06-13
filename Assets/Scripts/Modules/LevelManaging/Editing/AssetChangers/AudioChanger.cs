using System;
using Bridge.Models.Common;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal sealed class AudioChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;
        private Action<IAsset> _onCompleted;
        private IAudioAsset _previousAudio;
        private DbModelType _modelType;
        private float _audioVolume;
        
        public AudioChanger(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }
        
        public void Run(IPlayableMusic target, IAudioAsset previousAudio, float audioVolume, Action<IAsset> onCompleted)
        {
            _onCompleted = onCompleted;
            _previousAudio = previousAudio;
            _audioVolume = audioVolume;
            _modelType= DbModelExtensions.GetModelType(target.GetType());
            InvokeAssetStartedUpdating(_modelType, target.Id);
            _assetManager.Load(target, OnAudioLoaded,x=>OnFail(x, target.Id, _modelType));
        }

        private void OnAudioLoaded(IAsset asset)
        {
            if (_previousAudio != null && _previousAudio.Id != asset.Id)
            {
                _assetManager.Unload(_previousAudio);
            }

            var audioAsset = asset as IAudioAsset;
            audioAsset?.SetVolume(_audioVolume);

            InvokeAssetUpdated(_modelType);
            _onCompleted?.Invoke(asset);
            _onCompleted = null;
        }
    }
}
