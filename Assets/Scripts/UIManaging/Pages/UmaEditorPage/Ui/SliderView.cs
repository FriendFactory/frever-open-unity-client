using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui {
    public class SliderView : MonoBehaviour
    {
        [FormerlySerializedAs("slider")]
        public Slider Slider;
        [FormerlySerializedAs("thumb")]
        public Image Thumbnail;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Setup(Sprite thumb, Action<float> onValueChanged, Action<float> onBeginDrag) {
            Thumbnail.sprite = thumb;
            SetupEventTriggers(onValueChanged, onBeginDrag);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupEventTriggers(Action<float> onValueChanged, Action<float> onBeginDrag) {
            Slider.onValueChanged.AddListener((float value) => onValueChanged(value));

            var ev = Slider.gameObject.AddComponent<EventTrigger>();
            var onDragEntry = new EventTrigger.Entry {eventID = EventTriggerType.BeginDrag};
            onDragEntry.callback.AddListener((data) => {
                onBeginDrag(Slider.value);
            });
            ev.triggers.Add(onDragEntry);
        }
    }
}

