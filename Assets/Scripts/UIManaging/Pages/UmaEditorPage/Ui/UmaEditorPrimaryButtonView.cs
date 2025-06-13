using Extensions;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class UmaEditorPrimaryButtonView : MonoBehaviour
    {
        [SerializeField] private GameObject[] _createCharacterContent;
        [SerializeField] private GameObject[] _editorContent;

        public void SetCharacterCreationMode(bool creationMode)
        {
            _createCharacterContent.ForEach(go => go.SetActive(creationMode));
            _editorContent.ForEach(go => go.SetActive(!creationMode));
        }
    }
}
