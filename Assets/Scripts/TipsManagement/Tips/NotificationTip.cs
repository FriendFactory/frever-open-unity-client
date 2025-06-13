using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TipsManagment
{
    internal class NotificationTip : TextTip
    {
        [SerializeField]
        private TextMeshProUGUI _title;

        public override async Task Activate()
        {
            await base.Activate();
            StartTip();
        }

        public void ForceTipDone()
        {
            RaiseTipDone();
        }

        protected override void SetupText(string text)
        {
            var lines = text.Split(new[] { '\r', '\n' });
            if (lines.Length > 1)
            {
                _title.text = lines[0];
                var textWithoutTitle = text.Remove(0, lines[0].Length+1);
                base.SetupText(textWithoutTitle);
            }
            else
            {
                _title.text = text;
            }
        }

        protected override void PositionateTip() { }
    }
}