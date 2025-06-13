using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;
using UnityEngine;
using Zenject;
using static Navigation.Args.SeasonPageArgs;

namespace UIManaging.Pages.SeasonPage
{
    public class SeasonPageButton : ButtonBase
    {
        [SerializeField] private Tab _startingTab;

        [Inject] private IDataFetcher _dataFetcher;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = _dataFetcher.CurrentSeason != null;
        }

        protected override void OnClick()
        {
            var args = new SeasonPageArgs(_startingTab);
            Manager.MoveNext(PageId.SeasonInfo, args);
        }
    }
}