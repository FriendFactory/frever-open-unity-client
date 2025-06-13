using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.Outfits
{
    /// <summary>
    /// Quick solution for position controlling of Create New Outfit button
    /// </summary>
    internal sealed class CreateNewOutfitButtonPositionControl : MonoBehaviour
    {
        [SerializeField] private Vector2 _upperPosition;
        [SerializeField] private Vector2 _lowerPosition;

        [Inject] private ILevelManager _levelManager;

        private void OnEnable()
        {
            var characterSwitchingUiShown = _levelManager.TargetEvent.CharactersCount() > 1;
            var position = characterSwitchingUiShown ? _upperPosition : _lowerPosition;
            GetComponent<RectTransform>().anchoredPosition = position;
        }
    }
}