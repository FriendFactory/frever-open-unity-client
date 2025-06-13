using System;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking.Steps;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Outfits
{
    [RequireComponent(typeof(Button))]
    internal sealed class CreateNewOutfitButton : MonoBehaviour
    {
        [SerializeField] private float _disabledStateTransparency = 0.7f;
        [SerializeField] private MaskableGraphic[] _graphics;
        [SerializeField] private WardrobeSelectionCategoryData _wardrobeSelectionCategoryData;
        
        private BaseEditorPageModel _pageModel;
        private ILevelManager _levelManager;
        private IAssetManager _assetManager;
        
        private Button _button;

        public WardrobeSelectionProgressStepType ProgressStepType => _wardrobeSelectionCategoryData.ProgressStepType;

        public event Action Clicked;

        [Inject, UsedImplicitly]
        private void Construct(BaseEditorPageModel pageModel, ILevelManager levelManager, IAssetManager assetManager)
        {
            _pageModel = pageModel;
            _levelManager = levelManager;
            _assetManager = assetManager;
        }
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            _levelManager.EditingCharacterSequenceNumberChanged += RefreshState;
            _levelManager.TargetCharacterSequenceNumberChanged += RefreshState;
            _levelManager.CharactersPositionsSwapped += RefreshState;
            _levelManager.CharactersOutfitsUpdatingBegan += RefreshState;
            _levelManager.CharactersOutfitsUpdated += RefreshState;

            _assetManager.AssetLoaded += OnAssetLoaded;
        }

        private void OnDisable()
        {
            _levelManager.EditingCharacterSequenceNumberChanged -= RefreshState;
            _levelManager.TargetCharacterSequenceNumberChanged -= RefreshState;
            _levelManager.CharactersPositionsSwapped -= RefreshState;
            _levelManager.CharactersOutfitsUpdatingBegan -= RefreshState;
            _levelManager.CharactersOutfitsUpdated -= RefreshState;
            
            _assetManager.AssetLoaded -= OnAssetLoaded;
        }

        private void OnClick()
        {
            // need to fire before opening wardrobe panel to update the state of the buttons
            Clicked?.Invoke();
            
            var wardrobeCategoryData = _wardrobeSelectionCategoryData.StartCategory;
            _pageModel.OnCreateNewOutfitClicked(wardrobeCategoryData.CategoryId, wardrobeCategoryData.SubCategoryId);
        }

        private void RefreshState()
        {
            var canCreate = _pageModel.CanCreateNewOutfitForTargetCharacter();
            var alpha = canCreate ? 1 : _disabledStateTransparency;
            foreach (var graphic in _graphics)
            {
                graphic.SetAlpha(alpha);
            }

            _button.enabled = canCreate;
        }

        private void OnAssetLoaded(IEntity _) => RefreshState();
    }
}
