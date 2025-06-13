using System;
using Bridge.Models.ClientServer.Level.Full;
using Modules.LevelManaging.Assets.Caption;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class EditableCaptionView : CaptionView
    {
        public float RotationEulerAngle => RectTransform.localEulerAngles.z;
        
        public event Action<string> EditButtonClicked;
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2) OnEdit();
        }
        
        private void OnEdit()
        {
            EditButtonClicked?.Invoke(Text);
        }
    }
}