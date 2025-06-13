using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Navigation.Core;
using ThirdPackagesExtends.Zenject;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    public class IPSelectionPopup : BasePopup<IPSelectionPopupConfiguration>
    {
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _overlayButton;
        [SerializeField] private IPItem _ipItemPrefab;
        [SerializeField] private GameObject _itemSeparatorPrefab;
        [SerializeField] private RectTransform _itemsContainer;

        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private PageManager _pageManager;
        [Inject] private IScenarioManager _scenarioManager;
        
        private List<GameObject> _itemsList = new();
        private Action<Universe> _onUniverseChosen;
        private Action _onCancel;

        private void Awake()
        {
            Setup();
        }
        
        protected override void OnConfigure(IPSelectionPopupConfiguration configuration)
        {
            var universes = configuration.Universes;
            _onUniverseChosen = configuration.OnUseClicked;
            _onCancel = configuration.OnCancel;

            foreach (var universe in universes)
            {
                if (_itemsList.Any())
                {
                    var separator = Instantiate(_itemSeparatorPrefab, _itemsContainer);
                    _itemsList.Add(separator);
                }
                var item = Instantiate(_ipItemPrefab, _itemsContainer);
                item.gameObject.InjectDependenciesIfNeeded();
                item.Init(universe, () => OnChosen(universe), () => OnCreate(universe));
                _itemsList.Add(item.gameObject);
            }
        }

        private void Setup()
        {
            _cancelButton.onClick.AddListener(OnCloseButtonClicked);
            _overlayButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnChosen(Universe universe)
        {
            _onUniverseChosen?.Invoke(universe);
            Hide(true);
        }

        private void OnCreate(Universe universe)
        {
            var race = _metadataProvider.MetadataStartPack.GetRaceByUniverseId(universe.Id);
            
            _scenarioManager.ExecuteNewCharacterCreation(race.Id);
            
            Hide(true);
        }

        private void OnCloseButtonClicked()
        {
            Hide(false);
        }

        public override void Hide(object result)
        {
            base.Hide(result);
            foreach (var ipItem in _itemsList)
            {
                Destroy(ipItem);
            }
            _itemsList.Clear();
            if ((bool)result == false)
            {
                _onCancel?.Invoke();
            }
        }
    }
}
