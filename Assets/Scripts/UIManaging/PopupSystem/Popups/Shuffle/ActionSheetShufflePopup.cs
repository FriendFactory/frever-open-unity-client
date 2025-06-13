using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.Views;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    internal sealed class ActionSheetShufflePopup: ActionSheetPopup<ActionSheetShufflePopupConfiguration>
    {
        [FormerlySerializedAs("_shuffleButtonPrefab")] [SerializeField] private ShuffleOptionButton _shuffleOptionButtonPrefab;
        
        private ActionSheetShufflePopupConfiguration _configuration;

        protected override void OnConfigure(ActionSheetShufflePopupConfiguration configuration)
        {
            _configuration = configuration;
            base.OnConfigure(configuration);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------


        protected override OptionButton CreateButton(int actionIndex)
        {
            if (_configuration.ShuffleButtonIndex != actionIndex)
            {
                return base.CreateButton(actionIndex);
            }
            
            var variant = _configuration.Variants[actionIndex];
            var button = Instantiate(_shuffleOptionButtonPrefab, _buttonsParent);
            var buttonGameObject = button.gameObject;
            buttonGameObject.SetActive(true);
            ActionGameObjects.Add(buttonGameObject);
                
            button.TargetButton.onClick.AddListener(() =>
            {
                variant.Value?.Invoke();
                Hide();
            });
            button.DescriptionText.text = _configuration.ShuffleButtonDescription;

            return button;
        }
    }
}