using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common;
using Configs;
using Extensions;
using JetBrains.Annotations;
using Modules.CharacterManagement;
using Modules.WardrobeManaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.InputHandling;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Modules.FreverUMA
{
    [UsedImplicitly]
    internal sealed class UMACharacterEditor : ICharacterEditor
    {
        private const string HAIR_ROOT_PATH = "Root/Global/Position/Hips/Spine/Spine1/Spine2/Neck/Head/HeadAdjust/HairRoot";
        
        //---------------------------------------------------------------------
        // Injects
        //---------------------------------------------------------------------

        [Inject] private readonly AvatarHelper _avatarHelper;
        [Inject] private readonly DefaultSubCategoryColors _defaultColors;
        [Inject] private readonly CharacterSlotEditCommand.Factory _slotEditFactory;
        [Inject] private readonly OutfitsManager _outfitsManager;
        [Inject] private readonly IBackButtonEventHandler _backButtonEventHandler;
        [Inject] private readonly IMetadataProvider _metadataProvider;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action CharacterChangingBegan;
        public event Action<IEntity[]> CharacterChanged;
        public event Action CharacterDNAChanged;
        public event Action CharacterUndressed;
        public event Action RemoveAllWardrobesCommandFinished;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsCharacterModified => _undoRedoHelper.UndoNotEmpty;
        public bool IsRedoEmpty => !_undoRedoHelper.RedoNotEmpty;
        public bool IsUndoEmpty => !_undoRedoHelper.UndoNotEmpty;
        public long GenderId => _genderId;
        public IEnumerable<UmaSharedColor> UmaColors => _clothesCabinet.UmaSharedColors;
        public IEnumerable<WardrobeItem> AppliedWardrobeItems => _appliedWardrobeItems;

        private readonly UndoRedoHelper _undoRedoHelper;
        private readonly Dictionary<string, WardrobeFullInfo> _recipeWardrobes = new();
        private readonly Dictionary<PresetItem, FreverDNAPreset> _wardrobeDNAPresets = new();
        private readonly List<WardrobeItem> _appliedWardrobeItems = new();

        private readonly CharacterManager _characterManager;
        private readonly IBridge _bridge;
        private readonly ClothesCabinet _clothesCabinet;

        private WardrobeSubCategory[] _subCategories;
        private DynamicCharacterAvatar _avatar;
        private Dictionary<string, DnaSetter> _avatarDNA;
        private CharacterFullInfo _editingCharacter;
        private OutfitShortInfo _appliedOutfit;
        private long _genderId;
        private long _subCategoryId;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public UMACharacterEditor(CharacterManager characterManager, IBridge bridge, ClothesCabinet clothesCabinet)
        {
            _undoRedoHelper = new UndoRedoHelper();
            _characterManager = characterManager;
            _bridge = bridge;
            _clothesCabinet = clothesCabinet;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(WardrobeSubCategory[] subCategories)
        {
            _subCategories = subCategories;
        }

        public void SetSubCategory(long id)
        {
            _subCategoryId = id;
        }

        public void SetCompressionEnabled(bool isEnabled)
        {
            UMAGeneratorBase.compressRenderTexture = isEnabled;
        }

        public Task<DynamicCharacterAvatar> CreateNewAvatar(long genderId, CancellationToken token = default)
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;
            return _avatarHelper.PrepareAvatar(genderId ,token);
        }

        public void SetTargetAvatar(DynamicCharacterAvatar avatar)
        {
            if (_avatar == avatar) return;
            if (_avatar != null)
            {
                _avatar.CharacterUpdated.RemoveListener(CharacterDidUpdate);
                _avatar.CharacterDnaUpdated.RemoveListener(OnCharacterDNAChanged);
            }
            ClearModificationsHistory();
            _avatar = avatar;
            _avatar.CharacterUpdated.AddListener(CharacterDidUpdate);
            _avatar.CharacterDnaUpdated.AddListener(OnCharacterDNAChanged);
        }

        public void SetEditingCharacterModel(CharacterFullInfo characterModel)
        {
            _editingCharacter = characterModel;
        }

        public async void LoadUmaBundles(CharacterFullInfo character, OutfitFullInfo outfit)
        {
            await _avatarHelper.PrepareBundlesForCharacterList(new List<CharacterAndOutfit>(1)
            {
                new()
                {
                    Character = character,
                    Outfit = outfit
                }
            });
        }

        public void UnloadUmaBundles()
        {
            _avatarHelper.UnloadAllUmaBundles();
        }

        public void UnloadNotUsedUmaBundles(DynamicCharacterAvatar avatar)
        {
            _avatarHelper.UnloadNotUsedByAvatarBundles(avatar);
        }

        public void ResetCharacter()
        {
            var startWardrobes = _recipeWardrobes.Values.ToArray();
            var startRecipe = _avatar.GetCurrentRecipe();

            var finalWardrobes = GetWardrobesFromCharacter(_editingCharacter);
            var finalRecipe = GetCharacterRecipe(_editingCharacter);

            var startCharacter = new KeyValuePair<string, WardrobeFullInfo[]>(startRecipe, startWardrobes);
            var finalCharacter = new KeyValuePair<string, WardrobeFullInfo[]>(finalRecipe, finalWardrobes);

            var resetCharacterCommand = new ResetCharacterCommand(startCharacter, finalCharacter, SetCharacter);

            _undoRedoHelper.RegisterCommand(resetCharacterCommand);
            resetCharacterCommand.ExecuteCommand();
        }

        public async Task LoadCharacter(CharacterFullInfo character, OutfitFullInfo outfit, CancellationToken token = default)
        {
            _appliedOutfit = null;
            _recipeWardrobes.Clear();
            _appliedWardrobeItems.Clear();
            _editingCharacter = character;
          
            SetGenderId(character.GenderId);
            try
            {
                await _avatarHelper.LoadCharacterToAvatar(_avatar, character, outfit, token);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return;
            }
            
            var wardrobes = outfit == null
                ? character.Wardrobes
                : _avatarHelper.SelectWardrobesForGender(outfit, character.GenderId);

            if (wardrobes == null)
            {
                OnCharacterChanged();
                return;
            }

            foreach (var id in wardrobes.Select(uw => uw.Id))
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(id);
                if (wardrobeItem is AccessoryItem accessoryItem)
                {
                    if (_recipeWardrobes.ContainsKey(accessoryItem.Slot))
                    {
                        Debug.LogWarning($"2 or more {accessoryItem.Slot}'s added to character");
                        _recipeWardrobes[accessoryItem.Slot] = accessoryItem.Wardrobe;
                        continue;
                    }
                    _recipeWardrobes.Add(accessoryItem.Slot, accessoryItem.Wardrobe);
                }
                _appliedWardrobeItems.Add(wardrobeItem);
            }
            OnCharacterChanged();
        }

        public void SetGenderId(long genderId)
        {
            _genderId = genderId;
        }
        
        public Dictionary<string, float> GetDNAValues()
        {
            var dnaValues = new Dictionary<string, float>();
            if (_avatarDNA == null) return dnaValues;
            foreach (var key in _avatarDNA.Keys)
            {
                dnaValues.Add(key, _avatarDNA[key].Get());
            }
            return dnaValues;
        }

        public Dictionary<string, Color32> GetColors()
        {
            return _avatar.GetColors();
        }
        
        public List<KeyValuePair<long, int>> GetCharacterColorsInt()
        {
            var colorList = new List<KeyValuePair<long, int>>();
            foreach (var color in GetColors())
            {
                if (color.Key == "Skin") continue;

                var sharedWithName = _clothesCabinet.UmaSharedColors.FirstOrDefault(x => x.Name == color.Key);
                if (sharedWithName == null)
                {
                    continue;
                }
                colorList.Add(new KeyValuePair<long, int>(sharedWithName.Id, color.Value.ConvertToInt()));
            }

            return colorList;
        }

        public void EditCharacterDNA(string bodyPart, float value, bool saveToHistory = false)
        {
            _avatarDNA[bodyPart].Set(value);
            _avatar.ForceUpdate(true, false, false);
        }

        public void EndCharacterDNAEdit(string bodyPart, float startValue, float endValue)
        {
            var dnaCommand = new UmaDnaEditCommand(startValue, endValue, _avatarDNA[bodyPart], _avatar);
            _undoRedoHelper.RegisterCommand(dnaCommand);
            dnaCommand.ExecuteCommand();
        }

        public async void ChangeWardrobeItem(long wardrobeId)
        {
            _ = await _avatarHelper.PrepareUmaBundleForWardrobe(wardrobeId);

            var wardrobeItem = _clothesCabinet.GetWardrobeItem(wardrobeId);
            if (wardrobeItem is PresetItem presetItem)
            {
                ChangePreset(presetItem);
            }
            else if (wardrobeItem is AccessoryItem accessory)
            {
                ChangeAccessory(accessory);
            }
        }

        public void ChangeWardrobeItem(DynamicCharacterAvatar avatar, WardrobeFullInfo wardrobe)
        {
            SetTargetAvatar(avatar);
            _subCategoryId = wardrobe.WardrobeSubCategoryIds.FirstOrDefault();
            ChangeWardrobeItem(wardrobe.Id);
        }
        
        public void UndressCharacter()
        {
            var startWardrobes = _recipeWardrobes.Values.ToArray();
            var command = new RemoveAllWardrobesCommand(startWardrobes, null, RemoveAllWardrobesCommandAction);
            command.CommandExecuted += OnRemoveAllWardrobesCommandExecuted;
            command.CommandCanceled += OnRemoveAllWardrobesCommandCanceled;
            OnCharacterUpdatingBegan();
            _undoRedoHelper.RegisterCommand(command);
            command.ExecuteCommand();
        }

        public void EditCharacterColor(string type, Color newColor, IEnumerable<WardrobeFullInfo> currentWardrobes, Action onCompleted = null)
        {
            var colorData = _avatar.GetColor(type);
            if (colorData == null) return;

            var oldColor = _avatar.GetColor(type).color;

            var colorCommand = new UmaColorEditCommand(oldColor, newColor, type, _avatar);

            _undoRedoHelper.RegisterCommand(colorCommand);
            colorCommand.CommandExecuted += (command) =>
            {
                _avatarHelper.GetOverrideUnderwear(currentWardrobes, out var overrideTop, out var overrideBot);
                _avatarHelper.UpdateUnderwearStates(_avatar, overrideTop, overrideBot);
                onCompleted?.Invoke();
            };
            colorCommand.CommandCanceled += (command) =>
            {
                _avatarHelper.GetOverrideUnderwear(currentWardrobes, out var overrideTop, out var overrideBot);
                _avatarHelper.UpdateUnderwearStates(_avatar, overrideTop, overrideBot);
            };
            colorCommand.ExecuteCommand();
        }
        
        public void ApplyPreset(PresetItem newItem, PresetItem basePreset, Dictionary<string, float> savedDNA = null)
        {
            OnCharacterUpdatingBegan();
            var isReset = newItem == null;
            var presetItem = isReset ? basePreset : newItem;

            if (_wardrobeDNAPresets.TryGetValue(presetItem, out var preset))
            {
                SetPreset(preset, isReset, savedDNA);
            }
            else
            {
                LoadPreset(presetItem, (loaded)=> SetPreset(loaded, isReset, savedDNA));
            }
        }

        public void ApplyOutfit(OutfitShortInfo outfit, long genderId = -1)
        {
            if (genderId == -1)
                genderId = _editingCharacter.GenderId;
            RunOutfitApplyCommand(outfit, genderId);
        }

        public void SetAppliedWardrobeItems(WardrobeFullInfo[] outfitWardrobes)
        {
            _appliedWardrobeItems.Clear();
            _appliedWardrobeItems.AddRange(outfitWardrobes.Select(wardrobe => _clothesCabinet.GetWardrobeItem(wardrobe.Id)));
        }

        public void Undo()
        {
            _undoRedoHelper.Undo();
        }

        public void Redo()
        {
            _undoRedoHelper.Redo();
        }

        public void ClearModificationsHistory()
        {
            _undoRedoHelper.Clear();
        }

        public void Clear()
        {
            _wardrobeDNAPresets.Clear();
            _avatar = null;
        }

        public KeyValuePair<string, List<WardrobeFullInfo>> GetCharacterRecipeAndWardrobes()
        {
            return new KeyValuePair<string, List<WardrobeFullInfo>>(_avatar.GetCurrentRecipe(), _recipeWardrobes.Values.ToList());
        }

        public List<WardrobeFullInfo> GetCharacterWardrobes()
        {
            return _recipeWardrobes.Values.ToList();
        }

        public void ShowHighlightingWardrobe(AccessoryItem wardrobe)
        {
            _avatarHelper.ShowHighlightingWardrobe(wardrobe, _avatar);
        }

        public float GetHeelsHeight(DynamicCharacterAvatar avatar)
        {
            return _avatarHelper.GetHeelsHeight(avatar);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void SetPreset(FreverDNAPreset preset, bool cleanUp, Dictionary<string, float> savedDNA = null)
        {
            var restoreCharacter = cleanUp && savedDNA.Count > 0;
            foreach (var p in preset.GetPresets(_avatar.activeRace.name))
            {
                var value = p.value;
                if (restoreCharacter)
                {
                    value = savedDNA[p.name];
                }
                else if (cleanUp)
                {
                    value = 0.5f;
                }

                _avatarDNA[p.name].Set(value);
            }
            _avatar.ForceUpdate(true, false, false);
           
            await AvatarHelper.WaitWhileAvatarCreated(_avatar);
            OnCharacterChanged();
        }

        private void ChangePreset(PresetItem presetItem)
        {
            var alreadyApplied = _appliedWardrobeItems.Contains(presetItem);
            PresetItem startItem;
            PresetItem finalItem;
            PresetItem basePreset;

            Dictionary<string, float> savedDNA = new Dictionary<string, float>();
            if (!alreadyApplied)
            {
                finalItem = presetItem;
                basePreset = finalItem;
                var wardrobeItem = _appliedWardrobeItems.Find((item) =>
                {
                    if (item is PresetItem preset)
                    {
                        return preset.PresetCategory == presetItem.PresetCategory;
                    }
                    return false;
                });
                startItem = (PresetItem)wardrobeItem;
                {
                    foreach (var item in _avatarDNA)
                    {
                        savedDNA.Add(item.Key, item.Value.Value);
                    }
                }
            }
            else
            {
                startItem = presetItem;
                finalItem = null;
                basePreset = startItem;
            }
            Action<PresetItem, PresetItem> commandAction = (item, samplePreset) => ApplyPreset(item, samplePreset, savedDNA);
            
            var command = new CharacterPresetCommand(startItem, finalItem, commandAction, basePreset);
            _undoRedoHelper.RegisterCommand(command);
            command.CommandExecuted += OnPresetCommandExecuted;
            command.CommandCanceled += OnPresetCommandCanceled;
            OnCharacterUpdatingBegan();
            command.ExecuteCommand();
        }

        private void OnPresetCommandExecuted(UserCommand userCommand)
        {
            var command = userCommand as CharacterPresetCommand;
            if (command.StartValue != null)
            {
                _appliedWardrobeItems.Remove(command.StartValue);
            }

            if (command.FinalValue != null)
            {
                ClearAppliedPresetsBySubCategory(command.FinalValue.PresetCategory);
                _appliedWardrobeItems.Add(command.FinalValue);
            }
            OnCharacterChanged();
        }

        private void OnPresetCommandCanceled(UserCommand userCommand)
        {
            var command = userCommand as CharacterPresetCommand;
            if (command.StartValue != null)
            {
                ClearAppliedPresetsBySubCategory(command.StartValue.PresetCategory);
                _appliedWardrobeItems.Add(command.StartValue);
            }

            if (command.FinalValue != null)
            {
                _appliedWardrobeItems.Remove(command.FinalValue);
            }
            OnCharacterChanged();
        }

        private void ClearAppliedPresetsBySubCategory(string subCategory)
        {
            var appliedPresets = _appliedWardrobeItems.Where(item =>
            {
                if (!(item is PresetItem presetItem)) return false;
                return presetItem != null && presetItem.PresetCategory == subCategory;
            }).ToArray();
            foreach (var preset in appliedPresets)
            {
                _appliedWardrobeItems.Remove(preset);
            }
        }

        private void ChangeAccessory(AccessoryItem accessoryItem)
        {
            var alreadyApplied = _appliedWardrobeItems.Contains(accessoryItem);
            AccessoryItem startItem;
            AccessoryItem finishItem;

            if (alreadyApplied)
            {
                finishItem = new AccessoryItem(null, accessoryItem.Slot, "Empty");
                startItem = accessoryItem;
            }
            else
            {
                finishItem = accessoryItem;
                var startWardrobe = _appliedWardrobeItems.Find((entity) =>
                {
                    if (entity is AccessoryItem accessory)
                    {
                        return accessory.Slot == accessoryItem.Slot;
                    }
                    return false;
                });
                if (startWardrobe is null)
                    startItem = new AccessoryItem(null, accessoryItem.Slot, "Empty");
                else
                    startItem = (AccessoryItem)startWardrobe;
            }
            
            var wardrobes = _appliedWardrobeItems.Where(x => x is AccessoryItem).Cast<AccessoryItem>().ToArray();
            var subCategoryId = accessoryItem.Wardrobe.WardrobeSubCategoryIds.First();
            var command = _slotEditFactory.Create(_avatar, accessoryItem.Slot, startItem, finishItem, wardrobes, GetDefaultColor(subCategoryId));
            _undoRedoHelper.RegisterCommand(command);
            command.CommandExecuted += OnAccessoryCommandExecuted;
            command.CommandCanceled += OnAccessoryCommandCanceled;
            OnCharacterUpdatingBegan();
            command.ExecuteCommand();
        }

        private void OnAccessoryCommandExecuted(UserCommand userCommand)
        {
            var command = userCommand as CharacterSlotEditCommand;
            var accessoriesToRemove = new List<AccessoryItem>(command.ClearedItems) { command.StartValue };
            var accessoriesToAdd = new List<AccessoryItem>() { command.FinalValue };
            UpdateWardrobeRecipes(accessoriesToRemove, accessoriesToAdd);
            _avatarHelper.UnloadNotUsedByAvatarBundles(_avatar);
            Resources.UnloadUnusedAssets();
            OnCharacterChanged();
        }

        private void OnAccessoryCommandCanceled(UserCommand userCommand)
        {
            var command = userCommand as CharacterSlotEditCommand;
            var accessoriesToRemove = new List<AccessoryItem>() { command.FinalValue };
            var accessoriesToAdd = new List<AccessoryItem>(command.ClearedItems) { command.StartValue };
            UpdateWardrobeRecipes(accessoriesToRemove, accessoriesToAdd);
            _avatarHelper.UnloadNotUsedByAvatarBundles(_avatar);
            Resources.UnloadUnusedAssets();
            OnCharacterChanged();          
        }

        private void UpdateWardrobeRecipes(IEnumerable<AccessoryItem> itemsToRemove, IEnumerable<AccessoryItem> itemsToAdd)
        {
            foreach (var item in itemsToRemove)
            {
                if (item == null) continue;

                _appliedWardrobeItems.Remove(item);
                UpdateRecipeWardrobes(item.Slot, null);
            }

            foreach (var item in itemsToAdd)
            {
                if (item == null) continue;

                if(item.Wardrobe != null) _appliedWardrobeItems.Add(item);
                UpdateRecipeWardrobes(item.Slot, item.Wardrobe);
            }
        }

        private void RunOutfitApplyCommand(OutfitShortInfo outfit, long genderId)
        {
            var startWardrobes = _appliedWardrobeItems.Where(x => x is AccessoryItem).Select(x=>x.Wardrobe).ToArray();
            var startOutfit = _appliedOutfit;
            var recipe = _avatar.GetCurrentRecipe();
            var command = new OutfitApplyCommand(startOutfit, outfit, (o, items) => ApplyOutfitAction(o, items, recipe, genderId), startWardrobes);
            _undoRedoHelper.RegisterCommand(command);
            command.CommandExecuted += OnOutfitCommandExecuted;
            command.CommandCanceled += OnOutfitCommandCanceled;
            OnCharacterUpdatingBegan();
            command.ExecuteCommand();
        }

        private async void ApplyOutfitAction(OutfitShortInfo outfit, IEnumerable<IEntity> startItems, string recipe,
            long genderId)
        {
            var startWardrobes = startItems.Select(x => x as WardrobeFullInfo).ToArray();
            if (outfit == null)
            {
                await _avatarHelper.UnapplyOutfit(_avatar, startWardrobes, recipe, _genderId);
            }
            else
            {
                var fullOutfit = await _outfitsManager.GetFullOutfit(outfit);
                await _avatarHelper.ApplyOutfit(_avatar, fullOutfit, genderId);
            }
        }

        private async void OnOutfitCommandExecuted(UserCommand userCommand)
        {
            var command = userCommand as OutfitApplyCommand;
            foreach (var item in command.StartWardrobes)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(item.Id) as AccessoryItem;
                if (wardrobeItem == null) continue;
                UpdateRecipeWardrobes(wardrobeItem.Slot, null);
                _appliedWardrobeItems.Remove(wardrobeItem);
            }
            
            await AvatarHelper.WaitWhileAvatarCreated(_avatar);

            var fullOutfit = await _outfitsManager.GetFullOutfit(command.FinalValue);
            var wardrobeIds = fullOutfit.Wardrobes.Select(x => x.Id);
            foreach (var id in wardrobeIds)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(id) as AccessoryItem;
                if (wardrobeItem == null) continue;
                UpdateRecipeWardrobes(wardrobeItem.Slot, wardrobeItem.Wardrobe);
                _appliedWardrobeItems.Add(wardrobeItem);
            }
            _appliedOutfit = command.FinalValue;
            _avatarHelper.UnloadNotUsedByAvatarBundles(_avatar);
            Resources.UnloadUnusedAssets();
            OnCharacterChanged();
        }

        private async void OnOutfitCommandCanceled(UserCommand userCommand)
        {
            var command = userCommand as OutfitApplyCommand;

            var fullOutfit = await _outfitsManager.GetFullOutfit(command.FinalValue);
            var wardrobeIds = fullOutfit.Wardrobes.Select(x => x.Id);
            foreach (var id in wardrobeIds)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(id) as AccessoryItem;
                if (wardrobeItem == null) continue;
                UpdateRecipeWardrobes(wardrobeItem.Slot, null);
                _appliedWardrobeItems.Remove(wardrobeItem);
            }

            foreach (var item in command.StartWardrobes)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(item.Id) as AccessoryItem;
                if (wardrobeItem == null) continue;
                UpdateRecipeWardrobes(wardrobeItem.Slot, wardrobeItem.Wardrobe);
                _appliedWardrobeItems.Add(wardrobeItem);
            }
            _appliedOutfit = command.StartValue;
            OnCharacterChanged();
        }

        private void OnRemoveAllWardrobesCommandCanceled(UserCommand userCommand)
        {
            OnCharacterUpdatingBegan();
            var command = userCommand as RemoveAllWardrobesCommand;
            foreach (var wardrobe in command.StartValue)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(wardrobe.Id) as AccessoryItem;
                _recipeWardrobes.Add(wardrobeItem.Slot, wardrobeItem.Wardrobe);
                _appliedWardrobeItems.Add(wardrobeItem);
            }
            OnCharacterChanged();
        }

        private void OnRemoveAllWardrobesCommandExecuted(UserCommand obj)
        {
            _appliedWardrobeItems.Clear();
            _recipeWardrobes.Clear();
            OnCharacterChanged();
            
            // should be fired after CharacterChanged event
            CharacterUndressed?.Invoke();
        }

        private async void RemoveAllWardrobesCommandAction(IEnumerable<WardrobeFullInfo> wardrobes, object parameters)
        {
            try
            {
                wardrobes ??= Array.Empty<WardrobeFullInfo>();

                await _avatarHelper.SetWardrobeList(_avatar, wardrobes);

                _avatar.BuildCharacter();
                _avatar.ForceUpdate(false, true, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                RemoveAllWardrobesCommandFinished?.Invoke();
            }
        }

        private static WardrobeFullInfo[] GetWardrobesFromCharacter(CharacterFullInfo character)
        {
            return character.Wardrobes != null ? character.Wardrobes.ToArray(): Array.Empty<WardrobeFullInfo>();
        }

        private string GetCharacterRecipe(CharacterFullInfo character, CancellationToken token = default)
        {
            if (character == null)
                return _characterManager.DefaultMaleRecipe;

            return System.Text.Encoding.UTF8.GetString(character.UmaRecipe.J);
        }

        private async void LoadPreset(PresetItem preset, Action<FreverDNAPreset> onSuccess)
        {
            var umaBundle = new UmaBundleFullInfo
            {
                Id = preset.PresetBundle.Id,
                Files = preset.PresetBundle.Files
            };
            var result = await _bridge.GetAssetAsync(umaBundle);
            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }
            var bundle = result.Object as AssetBundle;
            if (bundle == null)
            {
                return;
            }
            var asset = bundle.LoadAsset<FreverDNAPreset>(preset.AssetName);

            if (asset == null)
            {
                Debug.LogWarning($"Asset for preset {preset.AssetName} is null.");
            }

            _wardrobeDNAPresets.Add(preset, asset);

            bundle.Unload(true);
            onSuccess?.Invoke(asset);
        }

        private async void SetCharacter(KeyValuePair<string, WardrobeFullInfo[]> characterKeyValue, object parameters = null)
        {
            try
            {
                await _avatarHelper.LoadAvatar(_avatar, characterKeyValue.Key, _genderId, characterKeyValue.Value);
                await AvatarHelper.WaitWhileAvatarCreated(_avatar);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }

            _recipeWardrobes.Clear();
            _appliedWardrobeItems.Clear();

            foreach (var wardrobe in characterKeyValue.Value)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(wardrobe.Id);
                if (wardrobeItem is AccessoryItem accessoryItem)
                {
                    if (_recipeWardrobes.ContainsKey(accessoryItem.Slot))
                    {
                        Debug.LogWarning($"2 or more {accessoryItem.Slot}'s added to character");
                        _recipeWardrobes[accessoryItem.Slot] = accessoryItem.Wardrobe;
                        continue;
                    }
                    _recipeWardrobes.Add(accessoryItem.Slot, accessoryItem.Wardrobe);
                }
                _appliedWardrobeItems.Add(wardrobeItem);
            }
            OnCharacterChanged();
        }

        private void UpdateRecipeWardrobes(string slotName, WardrobeFullInfo slotWardrobe)
        {
            if (slotWardrobe == null)
            {
                if (_recipeWardrobes.ContainsKey(slotName))
                {
                    _recipeWardrobes.Remove(slotName);
                }
                return;
            }

            _recipeWardrobes[slotName] = slotWardrobe;
        }

        private void CharacterDidUpdate(UMAData data)
        {
            _avatarDNA = _avatar.GetDNA();
        }

        private void OnCharacterChanged()
        {
            var items = new List<IEntity>(_appliedWardrobeItems.Select(x => x.Wardrobe));
            if (_appliedOutfit != null)
            {
                items.Add(_appliedOutfit);
            }
            CharacterChanged?.Invoke(items.ToArray());
            _backButtonEventHandler.ProcessEvents(true);
            ApplyHairPhysicsSettings(_avatar.transform);
        }

        private void OnCharacterDNAChanged(UMAData data)
        {
            _avatarDNA = _avatar.GetDNA();
            CharacterDNAChanged?.Invoke();
        }

        private ColorData GetDefaultColor(long subcategoryId)
        {
            var subCat = _subCategories.FirstOrDefault(x => x.Id == subcategoryId);
            if (subCat is { UmaSharedColorId : null }) return null;
            
            var sharedColor = _clothesCabinet.GetUmaSharedColor(subCat.UmaSharedColorId.Value);
            var colorName = sharedColor.Name;
            var currentColor = _avatar.GetColor(colorName);
            if (currentColor != null) return null;

            var defaultColorIndex = _defaultColors.GetDefaultColorIndex(colorName);
            if (defaultColorIndex < 0)
            {
                Debug.LogWarning($"Default color index not setted for '{colorName}'");
                return null;
            }

            var defaultIntColor = sharedColor.Colors[defaultColorIndex];
            var defaultColor = new Color32().ConvertFromIntColor(defaultIntColor);
            return new ColorData(colorName, defaultColor);
        }

        private void OnCharacterUpdatingBegan()
        {
            _backButtonEventHandler.ProcessEvents(false);
            CharacterChangingBegan?.Invoke();
        }
        
        private void ApplyHairPhysicsSettings(Transform avatar)
        {
            var hairRoot = avatar.Find(HAIR_ROOT_PATH);
            if (hairRoot== null) return;
            var dynamicBone = hairRoot.GetComponent<DynamicBone>();
            if (dynamicBone == null) return;
            var hair = _appliedWardrobeItems.First(x => x.Wardrobe.IsHair());
            dynamicBone.ApplyPhysicsSettings(hair.Wardrobe.PhysicsSettings);
        }
    }
}