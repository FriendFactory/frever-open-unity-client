using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class EditingFlowTransitionManager
    {
        private readonly List<BaseEditingStepView> _views;

        public EditingFlowTransitionManager(List<BaseEditingStepView> views)
        {
            _views = views;
        }
        
        public async Task InitializeAsync(CancellationToken token = default)
        {
            await Task.WhenAll(_views.Select(async view =>
            {
                await view.TransitOutAsync(true);
                view.Hide();
            }));
        }
        
        public async Task SwitchAsync(LevelEditorState from, LevelEditorState to, bool forward = true, bool instant = false)
        {
            var fromView = _views.FirstOrDefault(view => view.State == from);
            var toView = _views.FirstOrDefault(view => view.State == to);
            
            var fromState = fromView != null ? fromView.State : LevelEditorState.None;
            var toState = toView != null ? toView.State : LevelEditorState.None;
            
            instant = fromState == LevelEditorState.Default || toState == LevelEditorState.Default || instant;
            
            await Task.WhenAll(
                TransitAsync(fromView, forward, true, instant),
                TransitAsync(toView, forward, false, instant)
            );
        }

        private async Task TransitAsync(BaseEditingStepView view, bool forward, bool hideOnComplete = false, bool instant = false)
        {
            if (view == null) return;

            view.Show();

            if (forward)
            {
                await view.TransitInAsync(instant);
            }
            else
            {
                await view.TransitOutAsync(instant);
            }

            if (hideOnComplete)
            {
                view.Hide();
            }
        }
    }
}