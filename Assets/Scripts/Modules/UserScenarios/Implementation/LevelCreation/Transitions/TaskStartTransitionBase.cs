using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Common;
using Common.ModelsMapping;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;
using UIManaging.SnackBarSystem;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    internal abstract class TaskStartTransitionBase<TContext>: TransitionBase<TContext>, IResolvable where TContext: class, ILevelCreationScenarioContext
    {
        private readonly IBridge _bridge;
        private readonly IMapper _mapper;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly ILevelHelper _levelHelper;
        private readonly ITemplateProvider _templateProvider;
        private readonly CharacterManager _characterManager;
        private readonly IMetadataProvider _metadataProvider;

        public TaskStartTransitionBase(IBridge bridge, IMapper mapper, SnackBarHelper snackBarHelper,
            ILevelHelper levelHelper, ITemplateProvider templateProvider, CharacterManager characterManager,
            IMetadataProvider metadataProvider)
        {
            _bridge = bridge;
            _mapper = mapper;
            _snackBarHelper = snackBarHelper;
            _levelHelper = levelHelper;
            _templateProvider = templateProvider;
            _characterManager = characterManager;
            _metadataProvider = metadataProvider;
        }
        
        protected async Task InitializeTask(TaskFullInfo taskInfo)
        {
            var hasLevel = taskInfo.LevelId.HasValue;
            if (hasLevel)
            {
                await SetupTaskLevelData(taskInfo.Id);
            }

            Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = hasLevel
                ? Context.LevelData.GetOrderedEvents().First().LevelSequence
                : 1;
            
            Context.Task = taskInfo;
            Context.LevelEditor.TemplateId = taskInfo.TemplateId;
            Context.InitialTemplateId = taskInfo.TemplateId;
        }

        protected async Task InitializeCharacter(bool isDressed)
        {
            
            var characterId = _characterManager.RaceMainCharacters[Context.CharacterSelection.Race.Id];
            var result = await _characterManager.GetCharacterFullInfos(new [] {characterId });
            Context.CharacterEditor.Character = result.FirstOrDefault();
            
            if (!isDressed)
            {
                Context.CharacterEditor.Outfit = CreateOutfitWithFaceItemsOnly(Context.CharacterEditor.Character);
            }
        }

        private OutfitFullInfo CreateOutfitWithFaceItemsOnly(CharacterFullInfo character)
        {
            var output = new OutfitFullInfo();
            if (character.Wardrobes == null) {return output;}
            
            var faceItems = SelectFaceWardrobes(character);

            output.Wardrobes = faceItems;
            output.GenderWardrobes = new Dictionary<long, List<long>>
            {
                { character.GenderId, faceItems.Select(x => x.Id).ToList() }
            };
            return output;
        }

        private List<WardrobeFullInfo> SelectFaceWardrobes(CharacterFullInfo character)
        {
            var faceCategories =
                _metadataProvider.WardrobeCategories.Where(IsFaceCategory).Select(x => x.Id).ToArray();
            var faceItems = character.Wardrobes.Where(
                x => faceCategories.Contains(x.WardrobeCategoryId)).ToList();
            return faceItems;
        }

        private static bool IsFaceCategory(WardrobeCategory x)
        {
            return x.Name == Constants.Wardrobes.HAIR_CATEGORY_NAME
                || x.Name == Constants.Wardrobes.FACIAL_HAIR_CATEGORY_NAME
                || x.Name == Constants.Wardrobes.EYE_BROWS_CATEGORY_NAME;
        }

        protected async Task SetupInitialState(TaskFullInfo taskInfo, string reasonText)
        {
            if (Context.LevelData != null)
            {
                SetupInitialStateForLevelBasedTask(taskInfo);
            }
            else
            {
                await SetupInitialStateForTemplateBasedTask(taskInfo);
            }
            
            Context.CharacterSelection.ReasonText = reasonText;
        }

        private void SetupInitialStateForLevelBasedTask(TaskFullInfo taskInfo)
        {
            var needCharacterPicking = GetMissedCharacterIds(Context.LevelData).Length > 0;
            if (needCharacterPicking)
            {
                SetupCharacterSelection();
                Context.CharacterSelection.CharacterToReplaceIds = GetMissedCharacterIds(Context.LevelData);
                Context.CharacterSelection.AutoPickedCharacters = GetSelectedByServerCharacterIds(Context.LevelData);
            }
            else
            {
                SetupSkippingCharacterSelection(taskInfo);
            }
        }

        private async Task SetupInitialStateForTemplateBasedTask(TaskFullInfo taskInfo)
        {
            var templateEvent = await _templateProvider.GetTemplateEvent(Context.InitialTemplateId.Value);
            var needCharacterPicking = templateEvent.GetCharacters().Length > 1;
            if (needCharacterPicking)
            {
                SetupCharacterSelection();
                Context.CharacterSelection.CharacterToReplaceIds = templateEvent.GetUniqueCharacterIds();
            }
            else
            {
                var mainCharacterFullInfo = await  _characterManager.GetSelectedCharacterFullInfo();
                var characterIdInTemplate = templateEvent.GetUniqueCharacterIds().First();
                Context.LevelEditor.CharactersToUseInTemplate = new Dictionary<long, CharacterFullInfo> {{characterIdInTemplate, mainCharacterFullInfo}};
                SetupSkippingCharacterSelection(taskInfo);
            }
        }

        protected virtual void SetupSkippingCharacterSelection(TaskFullInfo taskInfo)
        {
            
        }

        protected virtual void SetupCharacterSelection()
        {
            
        }

        private async Task SetupTaskLevelData(long taskId)
        {
            var levelResp = await _bridge.GetLevelForTaskAsync(taskId);
            if (levelResp.IsError)
            {
                if (levelResp.ErrorMessage.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
                {
                    _snackBarHelper.ShowAssetInaccessibleSnackBar();
                    return;
                }
                
                throw new Exception($"Failed to load level for the task {taskId}. Reason: {levelResp.ErrorMessage}");
            }

            var level = _mapper.Map(levelResp.Model);
            await _levelHelper.PrepareLevelForTask(level);
            level.SchoolTaskId = taskId;
            level.Description = string.Empty;
            Context.LevelData = level;
            Context.OriginalLevelData = level.Clone();
        }

        protected void InitializeShowingTaskInfo(Page initialPage)
        {
            switch (initialPage)
            {
                case Page.LevelEditor:
                    Context.LevelEditor.ShowTaskInfo = true;
                    break;
                case Page.PostRecordEditor:
                    Context.PostRecordEditor.ShowTaskInfo = true;
                    break;
                case Page.CharacterEditor:
                    Context.CharacterEditor.ShowTaskInfo = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(initialPage), initialPage, null);
            }
        }

        private static long[] GetMissedCharacterIds(Level level)
        {
            return level.Event.SelectMany(x => x.CharacterController).Where(x => x.Character == null)
                        .Select(x => x.CharacterId).Distinct().ToArray();
        }

        private static long[] GetSelectedByServerCharacterIds(Level level)
        {
            return level.Event.SelectMany(x => x.CharacterController).Where(x => x.Character != null)
                        .Select(x => x.Character.Id).Distinct().ToArray();
        }
    }
}