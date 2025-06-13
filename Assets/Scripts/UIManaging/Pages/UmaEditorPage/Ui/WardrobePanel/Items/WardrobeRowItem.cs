using Bridge.Models.Common;
using Bridge;
using EnhancedUI.EnhancedScroller;
using Modules.WardrobeManaging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class WardrobeRowItem : EnhancedScrollerCellView
    {
        [SerializeField]
        private List<WardrobeUIItem> _wardrobeUIItems = new List<WardrobeUIItem>();
        [SerializeField]
        private Button _adjustmentsButton;
        [SerializeField]
        private Button _createOutfitButton;
        [SerializeField]
        private Button _clearButton;

        public Action<WardrobeUIItem> LoadingStateCallback;

        private Resolution _thumbnailResolution;

        private List<IEntity> _selectedItems;
        private ClothesCabinet _clothesCabinet;

        public void Setup(WardrobeRowArgs rowArgs)
        {
            _thumbnailResolution = rowArgs.ThumbnailResolution;

            if (rowArgs.AdditionalButtonStyle != AdditionalButtonStyle.Nothing)
            {
                var additionalButton = ShowAdditionalButton(rowArgs.AdditionalButtonStyle);
                additionalButton.onClick.RemoveAllListeners();
                additionalButton.onClick.AddListener(rowArgs.OnAdditionalPressed);
            }
            else
            {
                ShowAdditionalButton(AdditionalButtonStyle.Nothing);
            }

            _selectedItems = rowArgs.SelectedItems;
            _clothesCabinet = rowArgs.ClothesCabinet;

            for (int i = 0; i < _wardrobeUIItems.Count; i++)
            {
                var uiItem = _wardrobeUIItems[i];
                if (i >= rowArgs.Items.Length)
                {
                    uiItem.gameObject.SetActive(false);
                    continue;
                }
                uiItem.ThumbnailResolution = _thumbnailResolution;
                uiItem.gameObject.SetActive(true);
                var item = rowArgs.Items[i] as IThumbnailOwner;

                uiItem.Init(rowArgs.Bridge);
                uiItem.Setup(item);
                uiItem.IsOwned = _clothesCabinet.IsWardrobePurchased(item.Id);

                uiItem.SetupInputHandler(rowArgs.WardrobesInputHandler);
                uiItem.ClearEvents();
                uiItem.ItemSelected += rowArgs.OnItemSelected;
                uiItem.PurchaseRequested += rowArgs.OnPurchaseRequested;
                LoadingStateCallback?.Invoke(uiItem);
                if (uiItem.Entity == null) continue;
                uiItem.Selected = _selectedItems.Exists(x => x.Id == uiItem.Entity.Id);
            }
        }

        public override void RefreshCellView()
        {
            base.RefreshCellView();
            foreach (var uiItem in _wardrobeUIItems)
            {
                if (uiItem.Entity == null) continue;
                LoadingStateCallback?.Invoke(uiItem);
                uiItem.UpdateOwnershipAnimated(_clothesCabinet.IsWardrobePurchased(uiItem.Entity.Id));
                uiItem.Selected = _selectedItems.Exists(x => x.Id == uiItem.Entity.Id);
            }
        }

        private Button ShowAdditionalButton(AdditionalButtonStyle additionalButtonStyle)
        {
            _adjustmentsButton.gameObject.SetActive(additionalButtonStyle == AdditionalButtonStyle.Adjustments);
            _createOutfitButton.gameObject.SetActive(additionalButtonStyle == AdditionalButtonStyle.CreateOutfit);
            _clearButton.gameObject.SetActive(additionalButtonStyle == AdditionalButtonStyle.Clear);
            switch (additionalButtonStyle)
            {
                case AdditionalButtonStyle.Adjustments:
                    return _adjustmentsButton;
                case AdditionalButtonStyle.CreateOutfit:
                    return _createOutfitButton;
                case AdditionalButtonStyle.Clear:
                default:
                    return _clearButton;
            }
        }
    }

    public enum AdditionalButtonStyle
    {
        Nothing,
        Adjustments,
        CreateOutfit,
        Clear
    }

    public class WardrobeRowArgs
    {
        public IBridge Bridge;
        public ClothesCabinet ClothesCabinet;
        public IEntity[] Items;
        public List<IEntity> SelectedItems;
        public Action<IEntity> OnItemSelected;
        public Action<IEntity> OnPurchaseRequested;
        public UnityAction OnAdditionalPressed;
        public AdditionalButtonStyle AdditionalButtonStyle = AdditionalButtonStyle.Nothing;
        public Resolution ThumbnailResolution = Resolution._128x128;
        public WardrobesInputHandler WardrobesInputHandler;
    }
}