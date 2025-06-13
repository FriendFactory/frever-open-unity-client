using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    public class LoadingOverlayLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _savingProgressMessage;
        [SerializeField] private LocalizedString _loadingProgressMessage;

        [SerializeField] private LocalizedString _levelEditorHeader;
        [SerializeField] private LocalizedString _taskVideoSubmittingHeader;
        [SerializeField] private LocalizedString _wardrobeHeader;
        [SerializeField] private LocalizedString _videoMessageHeader;
        [SerializeField] private LocalizedString _videoPublishingHeader;
        [SerializeField] private LocalizedString _collectingClipsHeader;
        [SerializeField] private LocalizedString _settingTheStageHeader;
        [SerializeField] private LocalizedString _remixHeader;
        [SerializeField] private LocalizedString _goingBackToClipEditorHeader;
        [SerializeField] private LocalizedString _goingBackToRecordHeader;
        public string SavingProgressMessage => _savingProgressMessage;
        public string LoadingProgressMessage => _loadingProgressMessage;
        public string LevelEditorHeader => _levelEditorHeader; 
        public string TaskVideoSubmittingHeader => _taskVideoSubmittingHeader;
        public string WardrobeHeader => _wardrobeHeader;
        public string VideoMessageHeader => _videoMessageHeader;
        public string VideoPublishingHeader => _videoPublishingHeader;
        public string CollectingClipsHeader => _collectingClipsHeader;
        public string SettingTheStageHeader => _settingTheStageHeader;
        public string RemixHeader => _remixHeader;
        public string GoingBackToClipEditorHeader => _goingBackToClipEditorHeader;
        public string GoingBackToRecordHeader => _goingBackToRecordHeader;
    }
}