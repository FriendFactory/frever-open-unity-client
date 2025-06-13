using System;
using System.Collections.Generic;
using Modules.CameraSystem.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedOptionTabViews.AdvancedCameraTabViews
{
    public class NoiseProfileTabView : AdvancedCameraTabView
    {
        [SerializeField] private Button _resetButton;
        [SerializeField] private GameObject _content;
        [SerializeField] private ScrollBarItem _scrollBarItemPrefab;
        [SerializeField] private Sprite _scrollBarItemThumbnail;
        [SerializeField] private Sprite _noneNoiseSettingsThumbnail;

        private readonly List<ScrollBarItem> _scrollBarItems = new List<ScrollBarItem>();
        private int _currentSelectedId;
        private int _defaultId = 0;
        private int _savedId;
        
        public override void Setup()
        {
            InstantiateScrollBarItems();
            var startId = CameraSystem.GetNoiseProfileId();
           _scrollBarItems[startId].SetToggle(true);
           CameraSystem.NoiseProfileChanged += OnNoiseSettingsChanged;
        }

        private void OnEnable()
        {
            _resetButton.onClick.AddListener(Reset);
        }

        private void OnDisable()
        {
            _resetButton.onClick.RemoveListener(Reset); 
        }

        public override void Reset()
        {
            if (_scrollBarItems.Count == 0) return;
            _scrollBarItems[_defaultId].SetToggle(true);
        }

        public override void CleanUp()
        {
            _resetButton.onClick.RemoveListener(Reset);
            DestroyScrollBarItems();
        }

        public override void Discard()
        {
            if (_scrollBarItems.Count == 0) return;
            _scrollBarItems[_savedId].SetToggle(true);
        }

        public override void SaveSettings()
        {
            _savedId = CameraSystem.GetNoiseProfileId();
        }

        private void OnItemSelected(int id)
        {
            if (_currentSelectedId == id) return;

            _scrollBarItems[_currentSelectedId].SetToggle(false);
            CameraSystem.SetNoiseProfile(id);
            _currentSelectedId = id;

            if (CurrentCameraController != null)
            {
                CurrentCameraController.CameraNoiseSettingsIndex = id;
            }
            
            if (_currentSelectedId == _defaultId) return;
            OnSettingChanged();
        }

        private void InstantiateScrollBarItems()
        {
            var noiseProfiles = CameraSystem.GetNoiseProfiles();

            for (int i = 0; i < noiseProfiles.Length; i++)
            {
                var scrollBarItem = Instantiate(_scrollBarItemPrefab, _content.transform);
                scrollBarItem.Setup(i);
                scrollBarItem.OnSelected += OnItemSelected;
                
                var isNoiseSettingsItem = i > 0;
                var viewIcon = isNoiseSettingsItem 
                    ? _scrollBarItemThumbnail 
                    : _noneNoiseSettingsThumbnail;

                scrollBarItem.SetThumbnail(viewIcon);

                _scrollBarItems.Add(scrollBarItem);
            }
        }

        private void DestroyScrollBarItems()
        {
            foreach (var scrollBarItem in _scrollBarItems)
            {
                scrollBarItem.OnSelected -= OnItemSelected;
                Destroy(scrollBarItem);
            }
        }

        private void OnNoiseSettingsChanged(int id)
        {
            if (_currentSelectedId == id) return;
            
            _scrollBarItems[id].SetToggle(true);
        }
    }
}