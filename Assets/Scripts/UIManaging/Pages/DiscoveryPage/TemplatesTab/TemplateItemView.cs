using System;
using Abstract;
using Bridge.Models.ClientServer.Template;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class TemplateItemView : BaseContextDataView<TemplateInfo>
    {
        [SerializeField] private Text _hashtag;
        [SerializeField] private Text _views;

        private Button _button;
        private bool _showViewsNumber;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<TemplateInfo> ItemClicked;

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
            _hashtag.text = ContextData.Title;

            if (!_showViewsNumber) return;

            var viewsCount = ContextData.UsageCount;
            _views.text = viewsCount >= 0 ? $"{viewsCount.ToShortenedString()} views" : "Add";
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