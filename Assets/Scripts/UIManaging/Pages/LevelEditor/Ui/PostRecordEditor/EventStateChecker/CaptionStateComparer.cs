using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class CaptionStateComparer : BaseAssetStateComparer
    {
        private CaptionFullInfo[] _original;
        private bool _onlyCompareTextContent;

        public override DbModelType Type => DbModelType.Caption;
        
        public override void SaveState(Event targetEvent)
        {
            _original = targetEvent.Caption.Select(x=>x.Clone()).ToArray();
        }

        public void EnableOnlyCompareText(bool enable)
        {
            _onlyCompareTextContent = enable;
        }

        protected override bool CheckInternal(Event targetEvent)
        {
            foreach (var originCaption in _original)
            {
                var currentCaption = targetEvent.GetCaption(originCaption.Id);
                if (_original != null && currentCaption == null || _original == null && currentCaption != null)
                {
                    return true;
                }
            
                if (_original == null && currentCaption == null)
                {
                    return false;
                }
            
                if (_onlyCompareTextContent)
                {
                    return !string.Equals(currentCaption.Text, originCaption.Text) && !string.IsNullOrEmpty(currentCaption.Text);
                }

                var changed = !string.Equals(currentCaption.Text, originCaption.Text) || currentCaption.FontId != originCaption.FontId ||
                       currentCaption.FontSize != originCaption.FontSize || currentCaption.PositionY != originCaption.PositionY ||
                       currentCaption.PositionX != originCaption.PositionX || currentCaption.RotationDegrees != originCaption.RotationDegrees ||
                       currentCaption.TextColor != originCaption.TextColor || currentCaption.BackgroundColor != originCaption.BackgroundColor ||
                       currentCaption.TextColorRgb != originCaption.TextColorRgb || currentCaption.BackgroundColorRgb != originCaption.BackgroundColorRgb ||
                       currentCaption.TextAlignment != originCaption.TextAlignment;
                if (changed) return true;
            }

            return false;
        }
    }
}