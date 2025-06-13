using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal abstract class EditingStep: MonoBehaviour
    {
        [Inject] protected LevelEditorPageModel LevelEditorPageModel;
        public abstract int OrderIndex { get; }

        public void Run()
        {
            OnRun();
        }

        protected abstract void OnRun();
    }
}