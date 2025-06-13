using System;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class CreateCharacterPanel : MonoBehaviour
    {
        [SerializeField] private UmaEditorPanel _editorPanel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private CharacterManager _characterManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        public void Init(CharacterManager characterManager) {
            _characterManager = characterManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(Action<string> onConfirm, Action onCancel) {
            _confirmButton.onClick.AddListener(() => { onConfirm(_editorPanel.GetCharacterRecipe());});
            _cancelButton.onClick.AddListener(() => { onCancel();});
        }

        public void Show() {
            _editorPanel.gameObject.SetActive(true);
            //_editorPanel.Show();
            throw new Exception("Need to be redone or use Uma Editor Page");
            //_editorPanel.LoadCharacter(_characterManager.DefaultMaleRecipe);
        }

        public void Hide() {
            _editorPanel.Clear();
            _editorPanel.gameObject.SetActive(false);
        }
    }
}

