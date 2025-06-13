using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using UIManaging.Core;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    [RequireComponent(typeof(Button))]
    public class OpenMainCharacterEditorButton : ButtonBase
    {
        [Inject] private CharacterManager _characterManager;
        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;

        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = true;
        }

        protected override async void OnClick()
        {
            Interactable = false;
            
            if(_characterManager.SelectedCharacter == null)
            {
                _popupManagerHelper.OpenMainCharacterIsNotSelectedPopup();
            }
            else
            {
                var fullCharacterResp = await _characterManager.GetCharacterAsync(_characterManager.SelectedCharacter.Id);
                _scenarioManager.ExecuteCharacterEditing(fullCharacterResp);
            }
        }
    }
}