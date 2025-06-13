using System.Collections.Generic;
using System.Linq;
using Extensions;
using Modules.FreverUMA;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class ButtonsController: MonoBehaviour
    {
        [SerializeField] private List<Button> _notInteractiveWhileUmaIsBuilding;
        [SerializeField] private List<Button> _hiddenWhileDnaPanelIsShown;
        [SerializeField] private List<GameObject> _hiddenWhileShoppingCartShonw;
        [SerializeField] private UmaEditorPanel _umaEditorPanel;

        [Inject] private AvatarHelper _avatarHelper;

        private Button[] _previouslyDisabledButtons;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _avatarHelper.StartedProcessingAvatar += OnCharacterEditingBegan;
            _avatarHelper.FinishedProcessingAvatar += OnCharacterChanged;

            if (_umaEditorPanel)
            {
                _umaEditorPanel.DNAPanelShown -= OnDnaPanelShown;
                _umaEditorPanel.ShoppingCartShown -= UmaEditorPanelShoppingCartShown;
            }
        }

        private void OnDestroy()
        {
            _avatarHelper.StartedProcessingAvatar -= OnCharacterEditingBegan;
            _avatarHelper.FinishedProcessingAvatar -= OnCharacterChanged;
            if (_umaEditorPanel)
            {
                _umaEditorPanel.DNAPanelShown -= OnDnaPanelShown;
                _umaEditorPanel.ShoppingCartShown -= UmaEditorPanelShoppingCartShown;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnCharacterEditingBegan()
        {
            var buttonsToDisable = _notInteractiveWhileUmaIsBuilding.Where(x => x.interactable).ToArray();
            _previouslyDisabledButtons = buttonsToDisable;

            foreach (var button in buttonsToDisable)
            {
                button.interactable = false;
            }
        }
        
        private void OnCharacterChanged()
        {
            if (_previouslyDisabledButtons == null) return;
            foreach (var button in _previouslyDisabledButtons)
            {
                button.interactable = true;
            }
        }
        
        private void OnDnaPanelShown(bool isActive)
        {
            foreach (var button in _hiddenWhileDnaPanelIsShown)
            {
                button.SetActive(!isActive);
            }
        }

        private void UmaEditorPanelShoppingCartShown(bool isActive)
        {
            foreach (var item in _hiddenWhileShoppingCartShonw)
            {
                item.SetActive(!isActive);
            }
        }
    }
}