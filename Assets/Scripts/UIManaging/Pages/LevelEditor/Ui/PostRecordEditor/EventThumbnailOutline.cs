using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class EventThumbnailOutline: MonoBehaviour
    {
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _highlightingColor = new Color32(248, 6, 157, 255);
        [SerializeField] private Image _outlineImage;

        public void Switch(bool highlight)
        {
            _outlineImage.color = highlight ? _highlightingColor : _defaultColor;
        }
    }
}