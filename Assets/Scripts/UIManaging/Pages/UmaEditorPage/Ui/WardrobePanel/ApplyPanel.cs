using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class ApplyPanel : MonoBehaviour
    {
        private const float CLICK_THRESHOLD = 0.1f;

        public Action ColorButtonClicked;
        public Action ChangeConfirmed;

        [SerializeField]
        private Button _adjustmentButton;

        [SerializeField]
        private Button _colorButton;

        private Vector2 _pointerStartPosition;

        private void Start()
        {
            _colorButton.onClick.AddListener(OnConfirmOrCancelClick);
            _adjustmentButton.onClick.AddListener(OnConfirmOrCancelClick);
        }

        public void ShowApplyPanel(bool isColorApply)
        {
            gameObject.SetActive(true);
            _colorButton.gameObject.SetActive(isColorApply);
            _adjustmentButton.gameObject.SetActive(!isColorApply);
        }

        public void HideApplyPanel()
        {
            gameObject.SetActive(false);
        }

        public void OnConfirmOrCancelClick()
        {
            ChangeConfirmed?.Invoke();
        }

        public void PointerDown(BaseEventData data)
        {
            var pointData = data as PointerEventData;
            _pointerStartPosition = pointData.position;
        }

        public void PointerUp(BaseEventData data)
        {
            var pointData = data as PointerEventData;
            if (Vector2.Distance(_pointerStartPosition, pointData.position) > CLICK_THRESHOLD) return;
            OnConfirmOrCancelClick();
        }
    }
}