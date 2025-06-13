using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    public class TaskCheckListInfoButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TaskCheckList _taskCheckList;

        private void Awake()
        {
            _button.onClick.AddListener(ShowCheckList);
        }

        private void ShowCheckList()
        {
            _taskCheckList.ShowOverlay();
        }
    }
}
