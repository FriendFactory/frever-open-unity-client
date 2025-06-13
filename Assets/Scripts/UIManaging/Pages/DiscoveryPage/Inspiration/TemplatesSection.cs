using System.Threading;
using Extensions;
using Navigation.Args;
using UIManaging.Common.Templates;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class TemplatesSection : ItemsDiscoverySection<TemplateRowItem>
    {
        [Header("Search")]
        [SerializeField] private DiscoverySearchView _discoverySearchView;

        [Inject] private TemplatesLoader _loader;
        private CancellationTokenSource _cancellationTokenSource = new();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void Init()
        {
            var templates = await _loader.DownloadTemplates(0, _itemsCount, null, null, _cancellationTokenSource.Token);

            var isCancelled = _cancellationTokenSource.IsCancellationRequested;
            
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            
            if (templates == null || isCancelled) return;

            var templatesCount = templates.Length;
            for (var i = 0; i < _itemsCount; i++)
            {
                if (i < templatesCount)
                {
                    var templateInfo = templates[i];
                    Items[i].Initialize(templateInfo);
                    Items[i].SetActive(true);
                }
                else
                {
                    Items[i].SetActive(false);
                }
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnMoreButtonClicked()
        {
            _discoverySearchView.Show(DiscoverySearchState.Templates);
        }

        protected override void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            base.OnDestroy();
        }
    }
}