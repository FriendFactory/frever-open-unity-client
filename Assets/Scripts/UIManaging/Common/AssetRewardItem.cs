using Abstract;
using Bridge;
using Bridge.Models.Common;
using TMPro;
using UIManaging.Common;
using UnityEngine;

namespace UIManaging.Pages.SeasonPage
{
    public class AssetRewardItem : BaseContextDataView<IThumbnailOwner>
    {
        [SerializeField] private ThumbnailLoader _thumbnailLoader;
        [SerializeField] private TMP_Text _label;

        protected override void OnInitialized()
        {
            _thumbnailLoader.Initialize(ContextData);
            _label.text = "x1";
        }
    }
}