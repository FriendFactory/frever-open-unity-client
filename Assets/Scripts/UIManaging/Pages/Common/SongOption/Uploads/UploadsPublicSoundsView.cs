using Abstract;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal sealed class UploadsPublicSoundsView: BaseContextDataView<TrendingUserSoundsListModel>
    {
        [SerializeField] private TrendingUserSoundsPanel _trendingUserSoundsPanel;
        
        protected override void OnInitialized()
        {
            _trendingUserSoundsPanel.Initialize(ContextData);
        }

        protected override void BeforeCleanup()
        {
            _trendingUserSoundsPanel.CleanUp();
        }
    }
}