using Modules.InputHandling;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Common.PageHeader
{
    public sealed class PageHeaderView : MonoBehaviour
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private TextMeshProUGUI _leftButtonText;
        [SerializeField] private TextMeshProUGUI _headerText;

        [Inject] private IBackButtonEventHandler _backButtonEventHandler;

        public string Header
        {
            get => _headerText.text;
            set => _headerText.text = value;
        }

        //---------------------------------------------------------------------
        // Messages 
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButtonEventHandler.AddButton(_leftButton.gameObject, () => _leftButton.onClick?.Invoke());
        }

        private void OnDisable()
        {
            _backButtonEventHandler.RemoveButton(_leftButton.gameObject);
        }

        private void OnDestroy()
        {
            _leftButton.onClick.RemoveAllListeners();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(PageHeaderArgs pageHeaderArgs)
        {
            if(pageHeaderArgs.HeaderText != null) _headerText.text = pageHeaderArgs.HeaderText;
            InitializeLeftButton(pageHeaderArgs.LeftButtonArgs);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetBackButtonInteractivity(bool interactable)
        {
            _leftButton.interactable = interactable;
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void InitializeLeftButton(ButtonArgs buttonArgs)
        {
            if (buttonArgs == null)
            {
                _leftButtonText.text = string.Empty;
                return;
            }
            
            InitializeButton(_leftButton, _leftButtonText, buttonArgs);
        }

        private void InitializeButton(Button button, TextMeshProUGUI buttonText, ButtonArgs buttonArgs)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(buttonArgs.Action.Invoke);

            if (buttonText)
            {
                buttonText.text = buttonArgs.Text;
            }
        }
    }
}