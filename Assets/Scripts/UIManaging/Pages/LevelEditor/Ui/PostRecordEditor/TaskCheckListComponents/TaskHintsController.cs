using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    public class TaskHintsController : MonoBehaviour
    {
        [SerializeField] private List<TaskAssetHint> _assetHints = new List<TaskAssetHint>();
        [SerializeField] private EventTimelinePostRecordingView _eventsTimelineView;

        public void Show(DbModelType[] types, bool showEventHints)
        {
            foreach (var type in types)
            {
                var hint = _assetHints.FirstOrDefault(x => x.Type == type);
                if (hint == null) continue;
                hint.Show(); 
            }

            if (showEventHints)
            {
                _eventsTimelineView.ShowHints();
            }
        }
        
        public void Hide()
        {
            foreach (var hint in _assetHints)
            {
                hint.Hide();
            }
            
            _eventsTimelineView.HideHints();
        }
    }
}
