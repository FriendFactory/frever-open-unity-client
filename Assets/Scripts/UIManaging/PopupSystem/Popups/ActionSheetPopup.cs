using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public abstract class ActionSheetPopup<TConfiguration> : InformationPopup<TConfiguration>
        where TConfiguration : ActionSheetPopupConfiguration
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] protected RectTransform _buttonsParent;
        [SerializeField] private OptionButton _buttonPrefab;
        [SerializeField] private Color _mainButtonColor;
        [SerializeField] private TMP_Text _cancelButtonText;
        [SerializeField] private GameObject _descriptionSeparator;
        [SerializeField] private LocalizedString _cancelButtonDefaultText;
        
        protected readonly List<GameObject> ActionGameObjects = new List<GameObject>();

        private ActionSheetPopupConfiguration _config;
        

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            _closeButton.onClick.AddListener(Cancel);
            _backgroundButton.onClick.AddListener(Cancel);
        }

        protected virtual void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(Cancel);
            _backgroundButton.onClick.RemoveListener(Cancel);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(TConfiguration configuration)
        {
            base.OnConfigure(configuration);
            _config = configuration;
            
            var hasDescription = !string.IsNullOrEmpty(configuration.Description);
            _descriptionText.gameObject.SetActive(hasDescription);
            _descriptionSeparator.SetActive(hasDescription);
            _cancelButtonText.text = configuration.CancelButtonText ?? _cancelButtonDefaultText;
            CreateActionItems();

        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Hide(object result)
        {
            base.Hide(result);
            ClearPopup();
        }
        
        public void Cancel()
        {
            _config?.OnCancel?.Invoke();
            Hide();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CreateActionItems()
        {
            for (var i = 0; i < _config.Variants.Count; i++)
            {
                var button = CreateButton(i);
                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                SetActionText(text, i);
            }
        }

        protected virtual OptionButton CreateButton(int actionIndex)
        {
            var variant = _config.Variants[actionIndex];
            var button = Instantiate(_buttonPrefab, _buttonsParent);
            var buttonGameObject = button.gameObject;
            buttonGameObject.SetActive(true);
            ActionGameObjects.Add(buttonGameObject);
                
            button.TargetButton.onClick.AddListener(() =>
            {
                variant.Value?.Invoke();
                Hide();
            });

            return button;
        }

        private void SetActionText(TextMeshProUGUI text, int actionIndex)
        {
            var useMainColor = _config.MainVariantIndexes.Contains(actionIndex);
            text.color = useMainColor ? _mainButtonColor : text.color;
            text.text = _config.Variants[actionIndex].Key;
        }

        private void ClearPopup()
        {
            foreach (var button in ActionGameObjects)
            {
                Destroy(button.gameObject);
            }
            
            ActionGameObjects.Clear();
        }
    }

    public class ActionSheetPopup : ActionSheetPopup<ActionSheetPopupConfiguration>
    {

    }
}