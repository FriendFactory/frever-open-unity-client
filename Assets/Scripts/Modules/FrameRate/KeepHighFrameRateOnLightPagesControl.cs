using System.Linq;
using JetBrains.Annotations;
using Navigation.Core;
using UnityEngine;

namespace Modules.FrameRate
{
    [UsedImplicitly]
    internal sealed class KeepHighFrameRateOnLightPagesControl: IFrameRateControl
    {
        private readonly PageManager _pageManager;
        private readonly PageId[] _heavyPages;

        public KeepHighFrameRateOnLightPagesControl(PageManager pageManager, PageId[] heavyPages)
        {
            _pageManager = pageManager;
            _heavyPages = heavyPages;
        }

        public void Initialize()
        {
            _pageManager.PageDisplayed += OnPageDisplayed;
        }

        private void OnPageDisplayed(PageData currentPage)
        {
            Application.targetFrameRate = _heavyPages.Contains(currentPage.PageId) ? 30 : 60;
        }
    }
}