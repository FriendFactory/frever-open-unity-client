using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Level.Shuffle;
using I2.Loc;
using TMPro;
using UIManaging.Common.SelectionPanel;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class AssetTypeView: SelectionCheckmarkView<AssetTypeModel>
    {
        [Serializable]
        private struct AssetTypeToDisplay
        {
            public ShuffleAssets AssetType;
            public LocalizedString Name;
            public Sprite Sprite;
        }
        
        [SerializeField] private List<AssetTypeToDisplay> _assetTypeIcons;
        [SerializeField] private Image _assetTypeIconImg;
        [SerializeField] private TextMeshProUGUI _assetTypeNameTxt;
        [SerializeField] private GameObject _enabledBgObj;
        [SerializeField] private GameObject _disabledBgObj;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var display = _assetTypeIcons.FirstOrDefault(assetType => assetType.AssetType == ContextData.Type);

            _assetTypeNameTxt.text = display.Name;
            _assetTypeIconImg.sprite = display.Sprite;
            
            UpdateCheckmark();

            ContextData.SelectionChanged += UpdateCheckmark;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            if (ContextData == null) return;
            
            ContextData.SelectionChanged -= UpdateCheckmark;
        }


        private void UpdateCheckmark()
        {
            _enabledBgObj.SetActive(ContextData.IsSelected);
            _disabledBgObj.SetActive(!ContextData.IsSelected);
        }
    }
}