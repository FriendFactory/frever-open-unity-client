using System.Linq;
using Extensions;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.FeatureControls
{
    internal sealed class MultiObjectsControlStateListener: StateListenerBase<LevelEditorState>
    {
        [SerializeField] private GameObject[] _targets;
        [SerializeField] private LevelEditorState[] _activeOnStates;

        protected override void OnInitialize()
        {
            if (_targets.IsNullOrEmpty()) return;
            
            StartListenToStateChanging();
        }

        public override void OnStateChanged(LevelEditorState state)
        {
            var shouldBeActive = _activeOnStates.Contains(state);
            foreach (var target in _targets)
            {
                if (target == null)
                {
                    Debug.LogWarning($"{gameObject.name}/{nameof(MultiObjectsControlStateListener)} : Missed object in the list");
                    continue;
                }
                target.SetActive(shouldBeActive);
            }
        }

        private void OnValidate()
        {
            _targets = _targets?.Where(x => x != null).Distinct().ToArray();
        }
    }
}