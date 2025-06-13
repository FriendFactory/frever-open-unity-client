using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class ShuffleController: MonoBehaviour
    {
        [SerializeField] private ShuffleOverlay _shuffleOverlay;
        [SerializeField] private ShuffleText _shuffleText;

        [Inject] private ILevelEditor _levelEditor;
        
        private void OnEnable()
        {
            if (_shuffleOverlay != null)
            {
                _levelEditor.ShufflingBegun += _shuffleOverlay.Show;
                _levelEditor.ShufflingFailed += _shuffleOverlay.Hide;
            }
            
            _levelEditor.ShufflingDone += OnShufflingDone;
        }

        private void OnDisable()
        {

            if (_shuffleOverlay != null)
            {
                _levelEditor.ShufflingBegun -= _shuffleOverlay.Show;
                _levelEditor.ShufflingFailed -= _shuffleOverlay.Hide;
            }

            _levelEditor.ShufflingDone -= OnShufflingDone;
        }
        
        private void OnShufflingDone()
        {
            var targetEvent = _levelEditor.TargetEvent;
            var title = targetEvent.GetTargetSpawnPosition().Name;
            var subtitle = targetEvent.GetSetLocation().Name;

            _shuffleText.Show(title, subtitle);

            if (_shuffleOverlay != null)
            {
                _shuffleOverlay.Hide();
            }
        }
    }
}