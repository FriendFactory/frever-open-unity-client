using Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class EditorUtilityView : MonoBehaviour
    {
        [FormerlySerializedAs("_undoButton")]
        public Button UndoButton;
        [FormerlySerializedAs("_redoButton")]
        public Button RedoButton;
        [FormerlySerializedAs("_resetButton")]
        public Button ResetButton;

        [SerializeField] private UmaEditorPrimaryButtonView[] _primaryButtons;

        public void SetCharacterCreationMode(bool characterCreationMode)
        {
            _primaryButtons.ForEach(b => b.SetCharacterCreationMode(characterCreationMode));
        }
    }
}
