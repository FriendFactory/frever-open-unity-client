using System;
using Extensions;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.MusicCue
{
    [RequireComponent(typeof(ScrollRect))]
    internal class MusicCueSlider : MonoBehaviour, IEndDragHandler
    {
        private const float MAX_SONG_SNIPPET = 15f;
        
        [SerializeField] private TextMeshProUGUI _selectedTimeText;
        [SerializeField] private TextMeshProUGUI _selectedEndTimeText;
        [SerializeField] private RectTransform _waveformImage;
        [SerializeField] private RectTransform _waveformSpace;
        [SerializeField] private Image _fillerImage;
        [Header("L10N")] 
        [SerializeField] private LocalizedString _startFromLoc;
        
        private ScrollRect _scrollRect;
        private RectTransform _fillerRect;

        private event Action OnActivationCueChanged;

        private float _songLength;

        private int _activationCue;
        private float _holderSize;

        private float _pixelsPerSecond;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _fillerRect = _fillerImage.GetComponent<RectTransform>();
            _holderSize = ((RectTransform) transform).rect.width;
            _pixelsPerSecond = _holderSize / MAX_SONG_SNIPPET;
        }

        private void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollChanged);
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        }

        public void Init(int activationCue, Action onActivationCueChanged, float length)
        {
            _activationCue = activationCue;
            OnActivationCueChanged = onActivationCueChanged;

            _songLength = length;
            var basicWidth = _songLength / MAX_SONG_SNIPPET * _holderSize;
            _waveformImage.sizeDelta = new Vector2(basicWidth, _waveformImage.sizeDelta.y);
            _waveformSpace.sizeDelta = new Vector2(basicWidth, _waveformSpace.sizeDelta.y);
            _fillerRect.sizeDelta = _waveformImage.sizeDelta;

            _scrollRect.horizontalNormalizedPosition = _activationCue == 0 ? 0 : _activationCue / (_songLength * 1000f) * 2;
            OnScrollChanged(Vector2.zero);
        }

        private void OnScrollChanged(Vector2 pos)
        {
            var startValue = _waveformSpace.anchoredPosition.x > 0 ? 0 :Mathf.Abs(_waveformSpace.anchoredPosition.x) / _pixelsPerSecond;
            
            var clampedSeconds = startValue + MAX_SONG_SNIPPET;
            clampedSeconds = Mathf.Clamp(clampedSeconds, startValue, _songLength);
            var endTime = TimeSpan.FromSeconds(clampedSeconds).ToString(@"mm\:ss");
            
            _selectedEndTimeText.text = $"{endTime}";
            
            var startTime = TimeSpan.FromSeconds(_activationCue.ToSeconds()).ToString(@"mm\:ss");
            _selectedTimeText.text = string.Format(_startFromLoc, startTime);
            _activationCue = Mathf.FloorToInt(Mathf.Clamp(startValue, 0, _songLength)).ToMilliseconds();
        }

        public void UpdatePlayerTime(float currentTime)
        {
            var endPositionPercentage = currentTime / _songLength;
            _fillerImage.fillAmount = endPositionPercentage;
        }
        
        public void OnEndDrag (PointerEventData data) 
        {
            OnActivationCueChanged?.Invoke();
        }

        public int GetActivationCue()
        {
            var max = _songLength;
            if(_songLength > MAX_SONG_SNIPPET) max -= MAX_SONG_SNIPPET;
            
            return Mathf.Clamp(_activationCue, 0, (int)max.ToMilliseconds());
        }
    }
}
