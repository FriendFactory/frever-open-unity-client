using System;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Template;
using Extensions;
using Navigation.Args;
using TMPro;
using UIManaging.Common.SelectableGrid;
using UnityEngine;
using Zenject;

namespace UIManaging.Common.Templates
{
    public class TemplateRowItem : BaseContextDataView<TemplateInfo>, ISelectableGridViewProvider
    {
        [SerializeField] protected TemplateThumbnail _thumbnail;
        [SerializeField] protected TextMeshProUGUI _title;
        [SerializeField] protected TextMeshProUGUI _usages;

        [Inject] private IBridge _bridge;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action OnIdChangedEvent;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public long Id => ContextData?.Id ?? 0;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            UpdateTitleText();
            UpdateUsagesText();
            LoadThumbnail();

            OnIdChangedEvent?.Invoke();
        }

        protected virtual void UpdateTitleText()
        {
            _title.text = ContextData.Title;
        }

        protected virtual void UpdateUsagesText()
        {
            _usages.text = ContextData.UsageCount.ToShortenedString();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void LoadThumbnail()
        {
            var templateVideoUrl = _bridge.GetTemplateVideoUrl(ContextData);
            _thumbnail.Initialize(new VideoThumbnailModel(templateVideoUrl));
        }
    }
}