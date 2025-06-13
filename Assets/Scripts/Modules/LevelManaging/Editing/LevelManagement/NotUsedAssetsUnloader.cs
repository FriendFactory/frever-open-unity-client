using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    [UsedImplicitly]
    internal sealed class NotUsedAssetsUnloader
    {
        private readonly EventAssetsProvider _eventAssetsProvider;
        private readonly IAssetManager _assetManager;
        private readonly CharacterViewContainer _characterViewContainer;
        private readonly DbModelType[] _allAssetTypes;
        private readonly Dictionary<DbModelType, List<long>> _lockedFromUnloading = new();

        public NotUsedAssetsUnloader(EventAssetsProvider eventAssetsProvider, IPlayableTypesProvider playableTypesProvider, 
                                     IAssetManager assetManager, CharacterViewContainer characterViewContainer)
        {
            _eventAssetsProvider = eventAssetsProvider;
            _assetManager = assetManager;
            _characterViewContainer = characterViewContainer;
            _allAssetTypes = playableTypesProvider.PlayableTypes.ToArray();
        }

        public void LockFromUnloading(DbModelType modelType, params long[] ids)
        {
            if (!_lockedFromUnloading.ContainsKey(modelType))
            {
                _lockedFromUnloading.Add(modelType, new List<long>());
            }
            _lockedFromUnloading[modelType].AddRange(ids);
        }

        public void UnlockForUnloading(DbModelType modelType, params long[] ids)
        {
            if (!_lockedFromUnloading.ContainsKey(modelType)) return;
            
            var locked = _lockedFromUnloading[modelType];
            foreach (var id in ids)
            {
                if (!ids.Contains(id)) continue;
                locked.Remove(id);
            }
        }
        
        public void UnloadNotUsedAssets(params Event[] keepEvents)
        {
            var eventAssets = new List<IAsset>();
            
            foreach (var ev in keepEvents)
            {
                var assets = _eventAssetsProvider.GetLoadedAssets(ev, _allAssetTypes);
                eventAssets.AddRange(assets);
            }
            var distinctAssets = eventAssets.Distinct().ToArray();
            var lockedFromUnloading = GetAssetsThatShouldBeKept();
            _assetManager.UnloadAllExceptFor(distinctAssets.Concat(lockedFromUnloading).ToArray());

            UnloadNotUsedOutfits(keepEvents);
        }

        private IEnumerable<IAsset> GetAssetsThatShouldBeKept()
        {
            return _assetManager.GetAllLoadedAssets().Where(x => _lockedFromUnloading.ContainsKey(x.AssetType) &&
                                                                 _lockedFromUnloading[x.AssetType].Contains(x.Id));
        }

        public void UnloadNotUsedAssets(DbModelType targetType, params Event[] keepEvents)
        {
            var eventAssets = new List<IAsset>();
            
            foreach (var ev in keepEvents)
            {
                var assets = _eventAssetsProvider.GetLoadedAssets(ev, targetType);
                eventAssets.AddRange(assets);
            }
            
            var distinctAssets = eventAssets.Distinct().ToArray();
            var lockedFromUnloading = GetAssetsThatShouldBeKept();
            _assetManager.UnloadAll(targetType, distinctAssets.Concat(lockedFromUnloading).ToArray());

            UnloadNotUsedOutfits(keepEvents);
        }
        
        public void UnloadNotUsedOutfits(params Event[] keepEvents)
        {
            var allCharacterControllers = keepEvents.SelectMany(x => x.CharacterController).ToArray();
            var loadedViews = _characterViewContainer.GetLoadedViewsList();
            var viewsToUnload = loadedViews.Where(view => !allCharacterControllers.Any(cc =>
                                                      cc.CharacterId == view.CharacterId && cc.OutfitId.Compare(view.OutfitId))).ToArray();

            _characterViewContainer.Unload(viewsToUnload);
            var characters = _assetManager.GetAllLoadedAssets(DbModelType.Character).Cast<ICharacterAsset>();
            foreach (var characterAsset in characters)
            {
                if (characterAsset.HasView) continue;
                var existedView = _characterViewContainer.GetLoadedViewsList().FirstOrDefault(x => x.CharacterId == characterAsset.Id);
                if (existedView == null)
                {
                    _assetManager.Unload(characterAsset);
                }
                else
                {
                    characterAsset.ChangeView(existedView);
                }
            }
        }
    }
}