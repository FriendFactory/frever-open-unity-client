using Modules.InputHandling;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    [RequireComponent(typeof(AssetSelectionViewManager))]
    public class AssetSelectionViewBackButton: MonoBehaviour
    {
        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        private AssetSelectionViewManager AssetSelectionViewManager =>
            _viewManager ? _viewManager : _viewManager = GetComponent<AssetSelectionViewManager>();
        
        private AssetSelectionViewManager _viewManager;

        private void OnEnable()
        {
            AssetSelectionViewManager.Opened += OnAssetSelectionViewOpened;
            AssetSelectionViewManager.Closed += OnAssetSelectionViewClosed;
        }
        
        private void OnDisable()
        {
            AssetSelectionViewManager.Opened -= OnAssetSelectionViewOpened;
            AssetSelectionViewManager.Closed -= OnAssetSelectionViewClosed;
        }

        private void OnAssetSelectionViewOpened() => _backButtonEventHandler.AddButton(gameObject, AssetSelectionViewManager.OnTapOutsideView);
        private void OnAssetSelectionViewClosed() => _backButtonEventHandler.RemoveButton(gameObject);
    }
}