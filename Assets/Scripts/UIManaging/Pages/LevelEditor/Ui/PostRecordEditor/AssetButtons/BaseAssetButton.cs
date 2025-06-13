using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Pages.LevelEditor.Ui;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PostRecordEditor.AssetButtons
{
    internal abstract class BaseAssetButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        
        [Inject] protected PostRecordEditorPageModel EditorPageModel;
        [Inject] protected AmplitudeManager AmplitudeManager;
        
        protected Button Button => _button;
        
        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        protected abstract void OnClicked();

        protected void SetColor(Color color)
        {
            _button.image.color = color;
        }
        
        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }
    }
}
