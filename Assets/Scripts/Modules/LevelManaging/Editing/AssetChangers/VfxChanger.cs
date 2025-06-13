using System;
using System.Linq;
using System.Threading;
using Bridge.Models;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetHelpers;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal sealed class VfxChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;
        private readonly IMetadataProvider _metadataProvider;
        private readonly VfxBinder _vfxBinder;

        private IVfxAsset _previousVfx;
        private Action<IAsset> _onCompleted;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public VfxChanger(IAssetManager assetManager, VfxBinder vfxBinder, IMetadataProvider metadataProvider)
        {
            _assetManager = assetManager;
            _vfxBinder = vfxBinder;
            _metadataProvider = metadataProvider;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run(VfxInfo target, CharacterSpawnPositionInfo spawnPosition, ICharacterAsset targetCharacter, Action<IAsset> onCompleted)
        {
            CancelAll();

            _onCompleted = onCompleted;
            var vfxAssets = _assetManager.GetActiveAssets<IVfxAsset>();
            _previousVfx = vfxAssets?.FirstOrDefault();
            var cancellationSource = new CancellationTokenSource();
            CancellationSources.Add(target.Id, cancellationSource);
            var args = new VfxLoadArgs() {CancellationToken = cancellationSource.Token};
            
            InvokeAssetStartedUpdating(DbModelType.Vfx, target.Id);
            _assetManager.Load(target, args, x=> OnVfxLoaded(x, spawnPosition, targetCharacter),x=>OnFail(x, target.Id, DbModelType.Vfx));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnVfxLoaded(IAsset asset, CharacterSpawnPositionInfo spawnPosition, ICharacterAsset character)
        {
            CancellationSources.Remove(asset.Id);
            var vfx = asset as IVfxAsset;
            vfx?.SetActive(true);
            
            var setLocation = _assetManager.GetActiveAssets<ISetLocationAsset>().FirstOrDefault();
            setLocation?.Attach(spawnPosition, asset as IVfxAsset);
            _vfxBinder.Setup(vfx, character);
            
            if (_previousVfx != null)
            {
                _previousVfx?.SetActive(false);
                _assetManager.Unload(_previousVfx);
            }

            _onCompleted?.Invoke(asset);
            _onCompleted = null;
            InvokeAssetUpdated(DbModelType.Vfx);
        }
    }
}