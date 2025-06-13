using Modules.LevelManaging.Editing.LevelManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AudioPanel
{
    public abstract class BaseAudioSlider : MonoBehaviour
    {
        [SerializeField] private Graphic _isMutedGraphic;
        [SerializeField] private Graphic _isNotMutedGraphic;
        [SerializeField] private Slider _slider;
        [SerializeField] private Button _muteButton;
        [SerializeField] private TextMeshProUGUI _valueText;

        [Inject] protected ILevelManager LevelManager;
        
        private int _valueBeforeMute;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _slider.minValue = 0;
            _slider.maxValue = 100;
            _slider.wholeNumbers = true;
        }

        private void OnEnable()
        {
            _slider.value = GetCurrentValue();
            OnSliderValueChanged(GetCurrentValue());
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _muteButton.onClick.AddListener(OnMuteButtonClicked);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            _muteButton.onClick.RemoveListener(OnMuteButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected int GeSliderValue()
        {
            return (int)_slider.value;
        }

        protected abstract int GetCurrentValue();
        public abstract void ApplyCurrentValue();
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnMuteButtonClicked()
        {
            var isMuted = IsMuted();

            if (isMuted)
            {
                _slider.value = _valueBeforeMute;
            }
            else
            {
                _valueBeforeMute = GeSliderValue();
                _slider.value = 0;
            }
        }

        private bool IsMuted()
        {
            return GeSliderValue() <= 0;
        }
        
        private void OnSliderValueChanged(float value)
        {
            _valueText.text = value.ToString();

            var isMuted = IsMuted();
            _isMutedGraphic.gameObject.SetActive(isMuted);
            _isNotMutedGraphic.gameObject.SetActive(!isMuted);
            _muteButton.targetGraphic = isMuted ? _isMutedGraphic : _isNotMutedGraphic;
        }
    }
}
