using System.Linq;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Characters
{
    internal sealed class SwitchableCharactersSelectionPanel : CharactersSelectionPanel
    {
        protected override void OnClicked(long characterId)
        {
            PageModel.OnCharacterSwitchableButtonClicked(characterId);
            PageModel.ConfirmCharacterLoaded();
        }

        protected override void OnCharactersUpdated()
        {
            base.OnCharactersUpdated();

            var currentSwitchTargetId = PageModel.SwitchTargetCharacterId;
            var charactersToggles = _characterButtons.Cast<CharacterSelectionToggle>().ToArray();
            charactersToggles.First(x => x.Character.Id == currentSwitchTargetId).Toggle.isOn = true;
            PageModel.OnCharacterSwitchableButtonClicked(currentSwitchTargetId);
            PageModel.ConfirmCharacterLoaded();
        }

        protected override void SetCharacterViewActive(BaseCharacterSelectionElement characterSelectionElement, bool value)
        {
            base.SetCharacterViewActive(characterSelectionElement, value);
            characterSelectionElement.gameObject.SetActive(value);
        }
    }
}
