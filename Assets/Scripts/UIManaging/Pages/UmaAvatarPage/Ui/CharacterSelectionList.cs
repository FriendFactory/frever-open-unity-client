using Abstract;
using EnhancedUI.EnhancedScroller;
using Extensions;
using Modules.InputHandling;
using System;
using UIManaging.Common;
using UIManaging.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    internal sealed class CharacterSelectionList : BaseContextDataView<CharacterCarouselModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private int _cellSize;

        [SerializeField] private ScrollRectEnhancedScrollerSnapping _enhancedScrollerSnapping;
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private ListPositionIndicator _positionIndicator;
        [SerializeField] private EnhancedScrollerCellView _cellViewPrefab;
        [SerializeField] private Button _setAsMainButton;
        [SerializeField] private GameObject _disabledMainButton;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private EnhancedScrollerCellView _addViewPrefab;
        
        [Inject] private IInputManager _inputManager;
        
        public event Action<CharacterInfo> CharacterEditClicked;
        public event Action<CharacterInfo> CharacterDeleteClicked;
        public event Action<CharacterInfo> CharacterAsMainClicked;
        public event Action<CharacterInfo, string> CharacterNameChanged;
        public event Action CreateNewClicked;

        private int _characterIndex;
        
        private CharacterInfo[] Characters => ContextData.Characters;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        private void OnEnable()
        {
            _enhancedScrollerSnapping.Snapping += OnScrollerSnapped;
            _setAsMainButton.onClick.AddListener(SetCharacterAsMain);
            _previousButton.onClick.AddListener(ScrollToPrevious);
            _nextButton.onClick.AddListener(ScrollToNext);
            _inputManager.Enable(true);
        }

        private void OnDisable()
        {
            _enhancedScrollerSnapping.Snapping -= OnScrollerSnapped;
            _setAsMainButton.onClick.RemoveListener(SetCharacterAsMain);
            _previousButton.onClick.RemoveListener(ScrollToPrevious);
            _nextButton.onClick.RemoveListener(ScrollToNext);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Show(int startIndex = 0)
        {
            gameObject.SetActive(true);
            _enhancedScroller.ReloadData();
            _enhancedScroller.JumpToDataIndex(startIndex, _enhancedScroller.snapJumpToOffset, _enhancedScroller.snapCellCenterOffset, _enhancedScroller.snapUseCellSpacing);
            _enhancedScroller._RefreshActive();
            _positionIndicator.Initialilze(ContextData.Characters.Length + 1);
            _positionIndicator.Refresh(startIndex);
            _characterIndex = startIndex;
            UpdateMainButtonState();
        }

        public void Hide() 
        {
            gameObject.SetActive(false);
        }

        public void UpdateCarousel()
        {
            if (_characterIndex >= ContextData.Characters.Length + 1)
            {
                _characterIndex = ContextData.Characters.Length;
            }
            _positionIndicator.CleanUp();
            _positionIndicator.Initialilze(ContextData.Characters.Length + 1);
            _positionIndicator.Refresh(_characterIndex);
            _enhancedScroller.ReloadData();
            _enhancedScroller.JumpToDataIndex(_characterIndex, _enhancedScroller.snapJumpToOffset, _enhancedScroller.snapCellCenterOffset, _enhancedScroller.snapUseCellSpacing);
            _enhancedScroller._RefreshActive();
            UpdateMainButtonState();
        }

        #region EnchancedScroller Interface
        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return Characters.Length + 1;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellSize;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if (dataIndex >= Characters.Length)
            {
                var addView = scroller.GetCellView(_addViewPrefab);

                var addNewButton = addView.GetComponent<AddCharacterCarouselItem>();
                addNewButton.Initialize(OnNewClicked);

                return addView;
            }

            var cellView = scroller.GetCellView(_cellViewPrefab);
            
            var element = cellView.GetComponent<CharacterSelectionElement>();

            var character = ContextData.Characters[dataIndex];
            var thumbnail = ContextData.CharacterThumbnails[character];

            element.Initialize(thumbnail, character.Name,
                                deleteAction: ()=>CharacterDeleteClicked?.Invoke(character), 
                                editAction: ()=> CharacterEditClicked?.Invoke(character),
                                nameChanged: (newName) => CharacterNameChanged?.Invoke(character, newName));
            
            return cellView;
        }
        #endregion

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
            ContextData.CharacterThumbnailLoaded += OnCharacterThumbnailLoaded;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnScrollerSnapped(int index)
        {
            _positionIndicator.Refresh(index);
            _characterIndex = index;
            UpdateMainButtonState();
        }

        private void SetCharacterAsMain()
        {
            if (ContextData.Characters.Length == 0) return;

            CharacterAsMainClicked?.Invoke(ContextData.Characters[_characterIndex]);
            UpdateMainButtonState();
        }

        private void UpdateMainButtonState()
        {
            if (_characterIndex >= ContextData.Characters.Length)
            {
                _disabledMainButton.SetActive(false);
                _setAsMainButton.SetActive(false);
                return;
            }

            var isCurrentCharacterMain = ContextData.IsCharacterMain(ContextData.Characters[_characterIndex]);
            _disabledMainButton.SetActive(isCurrentCharacterMain);
            _setAsMainButton.SetActive(!isCurrentCharacterMain);
        }

        private void OnNewClicked()
        {
            CreateNewClicked?.Invoke();
        }

        private void ScrollToNext()
        {
            if (_characterIndex >= Characters.Length) return;

            _enhancedScrollerSnapping.Snap(_characterIndex + 1);
        }

        private void ScrollToPrevious()
        {
            if (_characterIndex == 0) return;

            _enhancedScrollerSnapping.Snap(_characterIndex - 1);
        }

        private void OnCharacterThumbnailLoaded(CharacterInfo character, Texture2D texture)
        {
            if(!isActiveAndEnabled) return;

            var characterIndex = Array.FindIndex(ContextData.Characters, (x) => x == character);
            var cellView = _enhancedScroller.GetCellViewAtDataIndex(characterIndex);

            if(cellView == null) return;

            var element = cellView.GetComponent<CharacterSelectionElement>();
            element.UpdateThumbnail(texture);
        }
    }
}