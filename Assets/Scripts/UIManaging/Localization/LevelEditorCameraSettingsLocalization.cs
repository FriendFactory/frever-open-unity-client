using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class LevelEditorCameraSettingsLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _resetButton;
        [Space] 
        [SerializeField] private LocalizedString _zoomButton;
        [SerializeField] private LocalizedString _blurButton;
        [SerializeField] private LocalizedString _spinButton;
        [SerializeField] private LocalizedString _followButton;
        [SerializeField] private LocalizedString _shakeButton;
        [SerializeField] private LocalizedString _rangeButton;
        [Space]
        [SerializeField] private LocalizedString _timeLimitReachedSnackbarMessage;


        public string ResetButton => _resetButton;
        
        public string ZoomButton => _zoomButton;
        public string BlurButton => _blurButton;
        public string SpinButton => _spinButton;
        public string FollowButton => _followButton;
        public string ShakeButton => _shakeButton;
        public string RangeButton => _rangeButton;
        public string TimeLimitReachedSnackbarMessage => _timeLimitReachedSnackbarMessage;
    }
}