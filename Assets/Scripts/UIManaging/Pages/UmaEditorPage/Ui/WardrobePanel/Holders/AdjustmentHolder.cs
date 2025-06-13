using Bridge.Models.ClientServer.StartPack.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class AdjustmentHolder : BaseWardrobePanelUIHolder
    {
        [Inject] private UmaEditorCharacterParametersLocalization _adjustmentsLocalization;
        
        public event Action<UmaAdjustment, float> AdjustmentChanged;
        public event Action<UmaAdjustment, float, float> AdjustmentChangedDiff;

        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private DefaultAdjustmentValues _femaleDNADefaults;
        [SerializeField]
        private DefaultAdjustmentValues _nonbinaryDNADefaults;

        private Dictionary<WardrobeSubCategory, List<AdjustmentItemBase>> _adjustmentsInSubCategory = new Dictionary<WardrobeSubCategory, List<AdjustmentItemBase>>();

        public void Setup(WardrobeSubCategory subCategory)
        {
            var adjustmentIds = subCategory.UmaAdjustments;
            var adjustmentItems = new List<AdjustmentItemBase>();
            _adjustmentsInSubCategory.Add(subCategory, adjustmentItems);
            foreach (var id in adjustmentIds)
            {
                if (!_clothesCabinet.TryGetUmaAdjustment(id, out var item))
                {
                    continue;
                }
                var adjustmentGO = Instantiate(_itemPrefab, _scrollRect.content);
                var adjustmentItem = adjustmentGO.GetComponent<AdjustmentItemBase>();
                adjustmentItem.Setup(item);
                adjustmentItem.SetLocalizedName(_adjustmentsLocalization);
                adjustmentItem.AdjustmentChanged += (value) => OnAdjustmentChanged(item, value);
                adjustmentItem.AdjustmentChangedDiff += (startValue, endValue) => OnAdjustmentChangedDiff(item, startValue, endValue);
                adjustmentItems.Add(adjustmentItem);
            }
        }

        public void ShowAdjustmentsForSubCategory(WardrobeSubCategory targetSubCategory)
        {
            foreach (var subCategory in _adjustmentsInSubCategory.Keys)
            {
                var activate = subCategory.Id == targetSubCategory.Id;
                var adjustments = _adjustmentsInSubCategory[subCategory];
                foreach (var adjustmentItem in adjustments)
                {
                    adjustmentItem.gameObject.SetActive(activate);
                }
            }
        }

        public void UpdateDnaValues(Dictionary<string, float> dnaValues)
        {
            foreach (var adjItem in _adjustmentsInSubCategory.Values.SelectMany(x => x))
            {
                if (!dnaValues.TryGetValue(adjItem.Entity.Key, out var value)) continue;
                adjItem.SetValue(value);
            }
        }

        public override void Clear()
        {
            var items = _adjustmentsInSubCategory.Values.SelectMany(item => item);
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            _adjustmentsInSubCategory = new Dictionary<WardrobeSubCategory, List<AdjustmentItemBase>>();
            AdjustmentChanged = null;
            AdjustmentChangedDiff = null;
        }

        public void SetStartValues(Gender gender)
        {
            List<DefaultDNAValue> dnaDNAs;
            switch (gender.Id)
            {
                case 2:
                    dnaDNAs = _femaleDNADefaults.DefaultValues;
                    break;
                case 3:
                    dnaDNAs = _nonbinaryDNADefaults.DefaultValues;
                    break;
                default:
                    dnaDNAs = new List<DefaultDNAValue>();
                    break;
            }

            foreach (var item in _adjustmentsInSubCategory.Values.SelectMany(x => x))
            {
                var defaultDNA = dnaDNAs.Find(x=> x.Name == item.Entity.Key);
                item.SetDefaultValue(defaultDNA == null ? 0.5f : defaultDNA.Value/255f);
            }
        }

        private void OnAdjustmentChanged(UmaAdjustment adjustment, float value)
        {
            AdjustmentChanged?.Invoke(adjustment, value);
        }

        private void OnAdjustmentChangedDiff(UmaAdjustment adjustment, float startValue, float endValue)
        {
            AdjustmentChangedDiff?.Invoke(adjustment, startValue, endValue);
        }
    }
}