using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [RequireComponent(typeof(Button))]
    public class LevelPreviewPauseButton: MonoBehaviour
    {
        private Action _onPaused;

        public void Init()
        {
            GetComponent<Button>().onClick.AddListener(PauseEventPreview);
        }

        private void PauseEventPreview()
        {
            _onPaused?.Invoke();
        }
    }
}