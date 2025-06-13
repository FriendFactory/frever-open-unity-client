using System.Threading.Tasks;
using UnityEngine.UI;

namespace TipsManagment
{
    internal sealed class ToggleCheckTip : TextTip
    {
        private Toggle _targetToggle;

        public override async Task Activate()
       {
            await base.Activate();
            if (TargetTransform == null) return;

            _targetToggle = Args.Target is ToggleTipTarget toggleTipTarget
                ? toggleTipTarget.Toggle
                : TargetTransform.GetComponent<Toggle>();
            
            if (_targetToggle == null) return;
            
            _targetToggle.onValueChanged.AddListener(ToggleValueChanged);

            StartTip();
        }

        private void ToggleValueChanged(bool value)
        {
            if (TaskSource == null) 
                return;
            _targetToggle.onValueChanged.RemoveListener(ToggleValueChanged);
            TaskSource.TrySetResult(value);
        }
    }
}