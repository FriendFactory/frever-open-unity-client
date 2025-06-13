using System;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class EventTimelineItemModel
    {
        public event Action<EventTimelineItemModel> IsSelectedChanged; 
        public event Action ThumbnailChanged;
        public Event Event { get; private set; }
        public bool IsSelected { get; private set; }

        public EventTimelineItemModel(Event @event)
        {
            Event = @event;
        }

        public void SetEvent(Event @event)
        {
            Event = @event;
        }

        public void ReloadThumbnail()
        {
            ThumbnailChanged?.Invoke();
        }

        public void SetIsSelected(bool value)
        {
            if(value == IsSelected) return;
            
            IsSelected = value;
            IsSelectedChanged?.Invoke(this);
        }

        public void CleanUp()
        {
            IsSelectedChanged = null;
        }
    }
}
