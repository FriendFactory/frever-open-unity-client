using Modules.Amplitude;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AssetButtons
{
    internal abstract class BaseAssetButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        protected LevelEditorPageModel LevelEditorPageModel;
        protected AmplitudeManager AmplitudeManager;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool Interactable
        {
            get => _button.interactable;
            protected set => _button.interactable = value;
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject, UsedImplicitly]
        private void Construct(LevelEditorPageModel levelEditorPageModel, AmplitudeManager amplitudeManager)
        {
            LevelEditorPageModel = levelEditorPageModel;
            AmplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        protected virtual void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnClicked();

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }
    }
}