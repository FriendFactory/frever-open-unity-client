using System;
using System.Linq;
using Extensions;
using Models;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public sealed class EventSettingsStateChecker
    {
        private readonly IAssetStateComparer[] _assetStateComparers;
        private Event _targetEvent;
        
        public EventSettingsStateChecker(IAssetStateComparer[] assetStateComparers)
        {
            _assetStateComparers = assetStateComparers;
        }

        public void StoreSettings(Event targetEvent)
        {
            _targetEvent = targetEvent;
            _assetStateComparers.ForEach(comparer => comparer.SaveState(targetEvent));
        }

        public bool HasAnyAssetsChanged(params DbModelType[] assetTypes)
        {
            if (assetTypes.Length == 0) throw new InvalidOperationException("No types were provided");
            if (_targetEvent == null) throw new InvalidOperationException("No event has been stored to check for settings changes");

            return assetTypes.Any(HasAssetChanged);
        }

        public bool HasAnyChangeWhichAffectCameraAnimationPath(Event targetEvent)
        {
            var assetStateComparer = (ICameraAnimationStateComparer) GetAssetStateComparer(DbModelType.CameraAnimationTemplate);
            assetStateComparer.Check(targetEvent);
            return assetStateComparer.IsCameraPathAffectedByChanges;
        }

        private bool HasAssetChanged(DbModelType type)
        {
            var assetStateComparer = GetAssetStateComparer(type);
            assetStateComparer.Check(_targetEvent);
            return assetStateComparer.AssetStateChanged;
        }

        private IAssetStateComparer GetAssetStateComparer(DbModelType type)
        {
            var assetStateComparer = _assetStateComparers.FirstOrDefault(x => x.Type == type);
            if (assetStateComparer == null)
            {
                throw new InvalidOperationException($"Missing implementation for asset type {type}");
            }

            return assetStateComparer;
        }
    }
}
