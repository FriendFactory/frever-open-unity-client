using System.Threading.Tasks;
using BrunoMikoski.AnimationSequencer;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    [RequireComponent(typeof(BaseEditingStepView))]
    public sealed class AnimatedTransition: MonoBehaviour
    {
        [SerializeField] private AnimationSequencerController _animationSequencerIn;
        [SerializeField] private AnimationSequencerController _animationSequencerOut;

        private BaseEditingStepView View => _view ? _view : _view = GetComponent<BaseEditingStepView>();
        
        private BaseEditingStepView _view;
        
        private void Awake()
        {
            _view = GetComponent<BaseEditingStepView>();
        }

        private void OnDestroy()
        {
            _animationSequencerIn.Kill();
            _animationSequencerOut.Kill();
        }

        public async Task FadeInAsync(bool instant = false)
        {
            await PlayAsync(_animationSequencerIn, instant);
        }
        
        public async Task FadeOutAsync(bool instant = false)
        {
            await PlayAsync(_animationSequencerOut, instant);
        }
        
        private async Task PlayAsync(AnimationSequencerController animationSequencer, bool instant = false)
        {
            var completionSource = new TaskCompletionSource<bool>();
            
            animationSequencer.OnStartEvent.AddListener(Complete);

            animationSequencer.Play(() =>
            {
                animationSequencer.OnStartEvent.RemoveListener(Complete);
                completionSource.SetResult(true);
            });

            await completionSource.Task;

            void Complete()
            {
                if (!instant) return;
                
                animationSequencer.Complete();
            }
        }
    }
}