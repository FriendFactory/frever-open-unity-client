using Common.UI;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls.ViewControls
{
    internal sealed class AssetsButtonView: MonoBehaviour
    {
        [SerializeField] private bool _activeOnStart;
        [SerializeField] private bool _showTextOnStart;
        [SerializeField] private TransparencyControl _buttonTransparencyControl;
        [SerializeField] private TransparencyControl _textTransparencyControl;
        
        private void Awake()
        {
            _buttonTransparencyControl.Switch(_activeOnStart, 0);
            _textTransparencyControl.Switch(_showTextOnStart, 0);
        }

        public void Show(float animationDuration)
        {
            _buttonTransparencyControl.Switch(true, animationDuration);
        }

        public void Hide(float animationDuration)
        {
            _buttonTransparencyControl.Switch(false, animationDuration);
        }
        
        public void ShowText(float animationDuration)
        {
            _textTransparencyControl.Switch(true, animationDuration);
        }

        public void HideText(float animationDuration)
        {
            _textTransparencyControl.Switch(false, animationDuration);
        }
    }
}