using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui {
    public class SwitchCharacterPanel : MonoBehaviour
    {
        [SerializeField]
        private Button _switchBackButton;
        [SerializeField]
        private Button _switchForwardButton;

        private int _maxIndex;
        private int _index;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(int numberOfRecipes, Action<int> onBackClicked, Action<int> onForwardClicked) {
            _maxIndex = numberOfRecipes - 1;

            _switchBackButton.onClick.AddListener(() => {
                _index --;
                onBackClicked(_index);
                ToggleButton(_switchBackButton, _index > 0);
                ToggleButton(_switchForwardButton, true);
            });

            _switchForwardButton.onClick.AddListener(() => {
                _index ++;
                onForwardClicked(_index);
                ToggleButton(_switchForwardButton, _index < _maxIndex);
                ToggleButton(_switchBackButton, true);
            });
        }

        public void Reset() {
            ToggleButton(_switchBackButton, false);
            ToggleButton(_switchForwardButton, true);
            _index = 0;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void ToggleButton(Selectable button, bool value) {
            var image = button.image;
            var color = image.color;
            color.a = value? 1f:  0.5f;
            image.color = color;
            button.enabled = value;
        }
    }
}
