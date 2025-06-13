using TMPro;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionItems
{
    public class SelectableTabItem : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _nameText;

        public void SetSelected(bool selected)
        {
            _canvasGroup.alpha = selected ? 1f : 0.6f;
        }

        public void SetNameText(string name)
        {
            _nameText.text = name;
        }
    }
}
