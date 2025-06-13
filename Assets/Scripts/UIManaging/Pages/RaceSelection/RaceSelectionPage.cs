using System;
using System.Linq;
using Extensions;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.RaceSelection
{
    internal sealed class RaceSelectionPage : GenericPage<RaceSelectionPageArs>
    {
        [SerializeField] private RaceSelectionList _raceSelectionList;
        [SerializeField] private Button _backButton;
        
        [Inject] private IMetadataProvider _metadataProvider;
        
        public override PageId Id => PageId.RaceSelection;
    
        protected override void OnInit(PageManager pageManager)
        {
            _raceSelectionList.RaceSelected += race => OpenPageArgs.RaceSelected?.Invoke(race);
            _backButton.onClick.AddListener(() => OpenPageArgs.MoveBackRequested?.Invoke());
        }

        protected override void OnDisplayStart(RaceSelectionPageArs args)
        {
            base.OnDisplayStart(args);
            
            var races = _metadataProvider.MetadataStartPack.GetRaces()
                .OrderBy(race => _metadataProvider.MetadataStartPack.GetUniverseByRaceId(race.Id).SortOrder)
                .ToArray();
            _raceSelectionList.Init(races, _metadataProvider);
            _backButton.SetActive(args.MoveBackRequested != null);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _raceSelectionList.Cleanup();
        }
    }
}
