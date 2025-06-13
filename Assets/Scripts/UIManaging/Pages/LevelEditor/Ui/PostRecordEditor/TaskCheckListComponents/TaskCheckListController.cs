using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Common.ModelsMapping;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents
{
    [UsedImplicitly]
    internal sealed class TaskCheckListController
    {
        [Inject] private TaskEventSettingsComparer _eventSettingsStateChecker;
        [Inject] private ILevelManager _levelManager;
        [Inject] private IBridge _bridge;
        [Inject] private IMapper _mapper;

        private DbModelType[] _assetTypes;
        private readonly DbModelType[] _changedInAnyEventAssets = { DbModelType.Caption };

        private readonly Dictionary<DbModelType, DbModelType> _linkedAssets = new Dictionary<DbModelType, DbModelType>
        {
            { DbModelType.SetLocation, DbModelType.CharacterSpawnPosition },
            { DbModelType.CharacterSpawnPosition, DbModelType.SetLocation }
        };

        public bool IsEnabled { get; private set; }
        public Level TaskLevel { get; private set; }
        private Level ComparingLevel => _levelManager.CurrentLevel;

        public async Task Setup(bool enabled, long taskId, DbModelType[] types)
        {
            IsEnabled = false;
            
            if (!enabled)
            {
                return;
            }

            var levelDataResult = await _bridge.GetLevelForTaskAsync(taskId);
            if (!levelDataResult.IsSuccess)
            {
                return;
            }
            
            TaskLevel = _mapper.Map(levelDataResult.Model);
            
            _eventSettingsStateChecker.StoreTaskSettings(TaskLevel);
            _eventSettingsStateChecker.EnableCaptionCompareOnlyTextContent(true);
            _assetTypes = types;
            IsEnabled = true;
        }

        public void CleanUp()
        {
            _assetTypes = null;
            IsEnabled = false;
        }

        public bool HasAllAssetsChanged()
        {
            return GetUnchangedAssetsInLevel().Length == 0;
        }

        public DbModelType[] GetUnchangedAssetInEvent(Event targetEvent)
        {
            return _assetTypes.Where(type => !AssetMeetsChangeRequirementsInEvent(targetEvent, type)).ToArray();
        }
        
        private DbModelType[] GetUnchangedAssetsInLevel()
        {
            return _assetTypes.Where(assetType => !AssetMeetsChangeRequirementsInLevel(assetType)).ToArray();
        }
        
        private bool AssetMeetsChangeRequirementsInEvent(Event targetEvent, DbModelType type)
        {
            if (_changedInAnyEventAssets.Contains(type))
            {
                return _eventSettingsStateChecker.HasAssetChangedInAnyEvents(ComparingLevel, type);
            }

            if (_linkedAssets.ContainsKey(type))
            {
                var linkedAssetType = _linkedAssets[type];
                return _eventSettingsStateChecker.HasAssetChangedInEvent(targetEvent, type) || _eventSettingsStateChecker.HasAssetChangedInEvent(targetEvent, linkedAssetType);
            }
            
            return _eventSettingsStateChecker.HasAssetChangedInEvent(targetEvent, type);
        }

        private bool AssetMeetsChangeRequirementsInLevel(DbModelType type)
        {
            if (_changedInAnyEventAssets.Contains(type))
            {
                return _eventSettingsStateChecker.HasAssetChangedInAnyEvents(ComparingLevel, type);
            }

            if (_linkedAssets.ContainsKey(type))
            {
                var linkedAssetType = _linkedAssets[type];
                return _eventSettingsStateChecker.HasAssetChangedInAllEvents(ComparingLevel, type) || _eventSettingsStateChecker.HasAssetChangedInAllEvents(ComparingLevel, linkedAssetType);
            }
            
            return _eventSettingsStateChecker.HasAssetChangedInAllEvents(ComparingLevel, type);
        }
    }
}
