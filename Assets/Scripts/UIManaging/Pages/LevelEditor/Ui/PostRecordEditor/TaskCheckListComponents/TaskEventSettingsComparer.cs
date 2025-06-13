using System;
using System.Linq;
using Extensions;
using Models;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    public class TaskEventSettingsComparer
    {
        private readonly IAssetStateComparer[] _assetStateComparers;
        private readonly CaptionStateComparer _captionStateComparer;
        private Level _level;

        [Inject]
        public TaskEventSettingsComparer(IAssetStateComparer[] assetStateComparers)
        {
            _assetStateComparers = assetStateComparers;
            _captionStateComparer = GetAssetComparer<CaptionStateComparer>(DbModelType.Caption);
        }

        public void StoreTaskSettings(Level taskLevel)
        {
            _level = taskLevel;
        }

        public void EnableCaptionCompareOnlyTextContent(bool enable)
        {
            _captionStateComparer.EnableOnlyCompareText(enable);
        }

        public bool HasAssetChangedInEvent(Event comparingEvent, DbModelType type)
        {
            var assetStateComparer = GetAssetComparer(type);

            var taskEvent = _level.GetEventBySequenceNumber(comparingEvent.LevelSequence);
            assetStateComparer.SaveState(taskEvent);
            assetStateComparer.Check(comparingEvent);
            return assetStateComparer.AssetStateChanged;
        }

        public bool HasAssetChangedInAllEvents(Level comparingLevel, DbModelType type)
        {
            var assetStateComparer = GetAssetComparer(type);

            foreach (var taskEvent in _level.Event)
            {
                assetStateComparer.SaveState(taskEvent);

                var comparingEvent = comparingLevel.GetEventBySequenceNumber(taskEvent.LevelSequence);
                assetStateComparer.Check(comparingEvent);
                if (!assetStateComparer.AssetStateChanged)
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool HasAssetChangedInAnyEvents(Level comparingLevel, DbModelType type)
        {
            var assetStateComparer = GetAssetComparer(type);
            
            foreach (var taskEvent in _level.Event)
            {
                assetStateComparer.SaveState(taskEvent);

                var comparingEvent = comparingLevel.GetEventBySequenceNumber(taskEvent.LevelSequence);
                assetStateComparer.Check(comparingEvent);
                if (assetStateComparer.AssetStateChanged)
                {
                    return true;
                }
            }

            return false;
        }

        private IAssetStateComparer GetAssetComparer(DbModelType type)
        {
            var assetStateComparer = _assetStateComparers.FirstOrDefault(x => x.Type == type);
            if (assetStateComparer == null)
            {
                throw new InvalidOperationException($"Missing implementation for asset type {type}");
            }

            return assetStateComparer;
        }
        
        private T GetAssetComparer<T>(DbModelType type) where T : IAssetStateComparer
        {
            return (T) GetAssetComparer(type);
        }
    }
}