using Extensions;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class SideBorderHintsControl: MonoBehaviour
    {
        [SerializeField] private CaptionPositioningHint[] _borderHints;

        public void Init(RectTransform viewPort)
        {
            foreach (var borderHint in _borderHints)
            {
                borderHint.SetParent(viewPort);//to apply view port mask
            }
        }

        public void SwitchActiveState(bool isActive)
        {
            SwitchHints(isActive);
        }

        private void SwitchHints(bool isActive)
        {
            foreach (var borderHint in _borderHints)
            {
                borderHint.SetActive(isActive);
            }
        }
    }
}