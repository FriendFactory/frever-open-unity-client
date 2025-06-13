using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    public class OutfitSaveStateChangedEventListener: MonoBehaviour
    {
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        
        public UnityEvent SaveStarted = new ();
        public UnityEvent SaveFinished = new ();
        
        private void OnEnable()
        {
            _levelEditorPageModel.OutfitSaveStarted += OnOutfitSaveStarted;
            _levelEditorPageModel.OutfitSaved += OnOutfitSaved;
        }
        
        private void OnDisable()
        {
            _levelEditorPageModel.OutfitSaveStarted -= OnOutfitSaveStarted;
            _levelEditorPageModel.OutfitSaved -= OnOutfitSaved;
        }
        
        private void OnOutfitSaveStarted() => SaveStarted.Invoke();
        private void OnOutfitSaved() => SaveFinished.Invoke();
    }
}