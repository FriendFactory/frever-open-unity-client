using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Level.Shuffle;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    [CreateAssetMenu(fileName = "ShuffleSettings", menuName = "Friend Factory/Configs/Shuffle Settings", order = 4)]
    public class ShuffleAssetTypesSettings: ScriptableObject, IInitializable
    {
        [SerializeField] private ShuffleAssets _assetTypes = ShuffleAssets.SetLocation | ShuffleAssets.BodyAnimation;
        [SerializeField] private ShuffleAssets _selectedAssetTypes = ShuffleAssets.SetLocation | ShuffleAssets.BodyAnimation;
        [SerializeField] private ShuffleAssets _lockedAssetTypes;
        
        private List<AssetTypeModel> _assetTypeModels;
        private List<long> _selectedIds;
        private List<long> _lockedIds;

        public ICollection<AssetTypeModel> AssetTypeModels => _assetTypeModels;
        public ICollection<long> SelectedIds => _selectedIds;
        public ICollection<long> LockedIds => _lockedIds;
        public int MaxSelected => _assetTypeModels.Count;
        
        public void Initialize()
        {
            _assetTypeModels = Enum.GetValues(typeof(ShuffleAssets)).Cast<ShuffleAssets>()
                            .Where(assetType => (assetType & _assetTypes) == assetType)
                            .Select(assetType => new AssetTypeModel(assetType))
                            .ToList();
            _selectedIds = Enum.GetValues(typeof(ShuffleAssets)).Cast<ShuffleAssets>()
                                  .Where(assetType => (assetType & _selectedAssetTypes) == assetType)
                                  .Select(assetType => (long)assetType)
                                  .ToList();
            _lockedIds = Enum.GetValues(typeof(ShuffleAssets)).Cast<ShuffleAssets>()
                                  .Where(assetType => (assetType & _lockedAssetTypes) == assetType)
                                  .Select(assetType => (long)assetType)
                                  .ToList();
        }
    }
}