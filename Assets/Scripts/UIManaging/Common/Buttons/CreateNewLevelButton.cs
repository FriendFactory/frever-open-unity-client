using Modules.CharacterManagement;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using UIManaging.Core;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Button))]
    internal sealed class CreateNewLevelButton : ButtonBase
    {
        [Inject] private CharacterManager _characterManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private IUniverseManager _universeManager;
        [Inject] private IScenarioManager _scenarioManager;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = true;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClick()
        {
            GoToCreateNewLevelPage();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void GoToCreateNewLevelPage()
        {
            if (_characterManager.SelectedCharacter == null)
            {
                Interactable = true;
                _popupManagerHelper.OpenMainCharacterIsNotSelectedPopup();
                return;
            }

            _universeManager.SelectUniverse(universe =>
            {
                Interactable = false;
                _scenarioManager.ExecuteNewLevelCreation(universe);
            });
        }
    }
}