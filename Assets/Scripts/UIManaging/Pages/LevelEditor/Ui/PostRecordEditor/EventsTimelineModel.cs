using System;
using System.Linq;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    public class EventsTimelineModel
    {
        public event Action SelectedEventChanged;
        public event Action Initialized;
        public bool IsInitialized { get; private set; }
        public Level LevelData { get; private set; }
        public EventTimelineItemModel SelectedEvent { get; private set; }
        public EventTimelineItemModel[] EventTimelineItemModels { get; private set; }

        private readonly ILevelManager _levelManager;
        
        public EventsTimelineModel(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        private void OnEventDeleted()
        {
            Initialize();
            SelectTargetEvent();
        }
        
        public void Initialize()
        {
            _levelManager.EventDeleted -= OnEventDeleted;
            _levelManager.EventDeleted += OnEventDeleted;
            
            LevelData = _levelManager.CurrentLevel;
            EventTimelineItemModels = new EventTimelineItemModel[LevelData.Event.Count];

            for (var i = 0; i < EventTimelineItemModels.Length; i++)
            {
                EventTimelineItemModels[i] = new EventTimelineItemModel(LevelData.Event.ElementAt(i));
                EventTimelineItemModels[i].IsSelectedChanged += OnSelectedEventChanged;
            }

            IsInitialized = true;
            Initialized?.Invoke();
        }

        public void Cleanup()
        {
            _levelManager.EventDeleted -= OnEventDeleted;
            EventTimelineItemModels = Array.Empty<EventTimelineItemModel>();
        }
        
        public void SelectTargetEvent()
        {
            var newSelectedIndex = 0;
            for (var i = 0; i < EventTimelineItemModels.Length; i++)
            {
                var isSelectedEvent = _levelManager.TargetEvent.Id == EventTimelineItemModels[i].Event.Id;
                if (!isSelectedEvent) continue;

                newSelectedIndex = i;
                break;
            }

            EventTimelineItemModels[newSelectedIndex].SetIsSelected(true);
        }

        private void OnSelectedEventChanged(EventTimelineItemModel model)
        {
            if (!model.IsSelected) return;

            SelectedEvent?.SetIsSelected(false);
            SelectedEvent = model;
            SelectedEventChanged?.Invoke();
        }
    }
}