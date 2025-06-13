using System;
using TMPro;
using UnityEngine.EventSystems;

namespace UIManaging.Pages.ReportPage.Ui
{
    public class ExtendedTMP_Dropdown : TMP_Dropdown
    {
        public event Action OnPointerClicked;
        public event Action<PointerEventData> PointerDown;
        public event Action<PointerEventData> PointerUp;
        public event Action<BaseEventData> Selected;
        public event Action<BaseEventData> Submitted;
        public event Action<BaseEventData> Canceled;

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            Submitted?.Invoke(eventData);
        }

        public override void OnCancel(BaseEventData eventData)
        {
            base.OnCancel(eventData);
            Canceled?.Invoke(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Selected?.Invoke(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            PointerDown?.Invoke(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            PointerUp?.Invoke(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            OnPointerClicked?.Invoke();
        }
    }
}