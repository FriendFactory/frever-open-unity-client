using TMPro;
using UIManaging.Common.Args.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.PageHeader
{
    public class PageHeaderActionView : MonoBehaviour
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private Button _rightButton;
        [SerializeField] private TextMeshProUGUI _rightButtonText;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnDestroy()
        {
            _rightButton.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(PageHeaderActionArgs pageHeaderActionArgs)
        {
            InitializeRightButton(pageHeaderActionArgs.RightButtonArgs);
            _pageHeaderView.Init(pageHeaderActionArgs);
        }
        
        public void InitializeRightButton(ButtonArgs buttonArgs)
        {
            InitializeButton(_rightButton, _rightButtonText, buttonArgs);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private static void InitializeButton(Button button, TextMeshProUGUI buttonText, ButtonArgs buttonArgs)
        {
            button.onClick.RemoveAllListeners();
            
            if(buttonArgs?.Action != null)
            {
                button.onClick.AddListener(buttonArgs.Action.Invoke);
            }

            var hasText = buttonArgs != null && !string.IsNullOrWhiteSpace(buttonArgs.Text);
            var buttonTextString = hasText ? buttonArgs.Text : string.Empty;
            buttonText.text = buttonTextString;
        }
    }
}