using Abstract;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class EventTimelineItemView : BaseContextDataView<EventTimelineItemModel>
    {
        [SerializeField] private EventThumbnail _eventThumbnail;
        [SerializeField] private EventThumbnailOutline _outiline;

        protected override void OnInitialized()
        {
            _eventThumbnail.Initialize(ContextData.Event);

            ContextData.IsSelectedChanged += OnIsSelectedChanged;
            ContextData.ThumbnailChanged += OnThumbnailChanged;
        }
        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            if (ContextData != null)
            {
                ContextData.IsSelectedChanged -= OnIsSelectedChanged;
                ContextData.ThumbnailChanged -= OnThumbnailChanged;
            }
        }

        protected virtual void OnIsSelectedChanged(EventTimelineItemModel model)
        {
            RefreshSelectionObject();
        }

        protected void RefreshSelectionObject()
        {
            if (_outiline == null) return;
            _outiline.Switch(ShouldHighlight());
        }

        protected virtual bool ShouldHighlight()
        {
            return ContextData.IsSelected;
        }

        private void OnThumbnailChanged()
        {
            _eventThumbnail.RefreshThumbnail();
        }
    }
}