using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class ShuffleBindingsInstaller: MonoInstaller
    {
        [SerializeField] private ShuffleAssetTypesSettings _shuffleSettings;
        
        public override void InstallBindings()
        {
            _shuffleSettings.Initialize();
            
            Container.Bind<ShuffleAssetTypesSettings>().FromInstance(_shuffleSettings).AsSingle();
            Container.BindInterfacesAndSelfTo<AssetTypeListModel>().AsSingle();
        }
    }
}