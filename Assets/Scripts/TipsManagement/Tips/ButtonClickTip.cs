using System.Threading.Tasks;
using UnityEngine.UI;

namespace TipsManagment
{
    internal sealed class ButtonClickTip : TextTip
    {
        private Button _targetButton;

        public override async Task Activate()
        {
            await base.Activate();
            if (TargetTransform == null) return;
            _targetButton = TargetTransform.GetComponentInChildren<Button>();
            if (_targetButton == null) return;
            _targetButton.onClick.AddListener(TargetButtonClicked);

            StartTip();
        }

        public override void Hide()
        {
            _targetButton.onClick.RemoveListener(TargetButtonClicked);
            base.Hide();
        }

        private void TargetButtonClicked()
        {
            if (TaskSource == null) return;
            TaskSource.TrySetResult(true);
        }
    }
}