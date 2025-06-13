using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class InputBlocker: MonoBehaviour
    {
        [SerializeField] private Image _transparentOverlay;

        private void Start()
        {
            _transparentOverlay.raycastTarget = true;
            _transparentOverlay.SetAlpha(0);
        }

        public void Block()
        {
            Switch(true);
        }

        public void UnBlock()
        {
            Switch(false);
        }

        private void Switch(bool isOn)
        {
            _transparentOverlay.SetActive(isOn);
        }
    }
}