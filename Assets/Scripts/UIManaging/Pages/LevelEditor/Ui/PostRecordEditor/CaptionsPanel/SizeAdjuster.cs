using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class SizeAdjuster : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private SizeAdjusterMovementControl _movementControl;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float CurrentSize
        {
            get => _slider.value;
            set => _slider.value = value;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<float> SizeChanged; 

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(float minSize, float maxSize, float currentSize)
        {
            _slider.minValue = minSize;
            _slider.maxValue = maxSize;
            CurrentSize = currentSize;
            _movementControl.Show(true);
        }

        public void Hide()
        {
            _movementControl.Hide();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnValueChanged(float value)
        {
            SizeChanged?.Invoke(value);
        }
    }
}
