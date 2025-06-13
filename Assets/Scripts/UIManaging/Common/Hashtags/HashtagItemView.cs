using System;
using Abstract;
using Bridge.Models.VideoServer;
using Extensions;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.Hashtags
{
    public sealed class HashtagItemView : BaseContextDataView<HashtagInfo>
    {
        [SerializeField] private Text _hashtag;
        [SerializeField] private Text _views;
        [Space]
        [SerializeField] private bool _showPrefix = true;

        [Inject] private HashtagItemViewLocalization _localization;
        
        private Button _button;
        private bool _showViewsNumber;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<HashtagInfo> ItemClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
            _showViewsNumber = _views != null;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnItemClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnItemClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _hashtag.text = _showPrefix ? $"# {ContextData.Name}" : ContextData.Name;

            if (!_showViewsNumber) return;

            var viewsCount = ContextData.ViewsCount;
            _views.text = viewsCount >= 0 
                ? string.Format(_localization.ViewsCounterFormat, viewsCount.ToShortenedString())
                : _localization.AddHashtagTitle;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            ItemClicked = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnItemClicked()
        {
            ItemClicked?.Invoke(ContextData);
        }
    }
}