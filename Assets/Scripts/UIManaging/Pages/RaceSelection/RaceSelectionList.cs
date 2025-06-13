using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RaceSelection
{
    internal sealed class RaceSelectionList: MonoBehaviour
    {
        private readonly List<RaceItem> _raceItems = new();
        
        [SerializeField] private RaceItem _raceItemPrefab;
        
        [Inject] private IBridge _bridge;
        
        public event Action<Race> RaceSelected; 
        
        public void Init(Race[] races, IMetadataProvider metadataProvider)
        {
            foreach (var race in races)
            {
                var raceItem = Instantiate(_raceItemPrefab, transform);
                raceItem.Init(race, metadataProvider, _bridge);
                raceItem.Clicked += OnRaceSelected;
                _raceItems.Add(raceItem);
            }
        }

        public void Cleanup()
        {
            foreach (var raceItem in _raceItems)
            {
                raceItem.Clicked -= OnRaceSelected;
                Destroy(raceItem.gameObject);
            }
            _raceItems.Clear();
        }

        private void OnRaceSelected(Race race)
        {
            RaceSelected?.Invoke(race);
        }
    }
}