using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Common;
using Configs;
using Extensions;
using Modules.CharacterManagement;
using Modules.FreverUMA;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Modules.LocalStorage;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Random = UnityEngine.Random;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal sealed partial class LevelManager
    {
        private static readonly CharacterOutfitDefaultDataProvider CHARACTER_OUTFIT_DEFAULT_DATA_PROVIDER = new ();
        
        public event Action<WardrobeFullInfo[]> OnOutfitUpdated; 
        public event Action OnTrySavingEmptyOutfit; 
        
        private readonly ICharacterEditor _characterEditor;
        private readonly AvatarHelper _avatarHelper;
        private readonly CharacterViewContainer _characterViewContainer;
        private readonly CharacterManagerConfig _characterManagerConfig;
        private readonly Stack<OutfitFullInfo> _undoOutfitsStack = new();
        private readonly Stack<OutfitFullInfo> _redoOutfitsStack = new();

        private readonly Dictionary<long, BodyAnimationInfo> _characterBodyAnimations = new();
        private long _originalSpawnFormation;
        private bool _wasOriginalSpawnFormationApplied;
        private bool _wasOriginalSpawnPositionsChanged;
        private long _originalSpawnPosId;

        private readonly Dictionary<long, long> _originalCharacterSpawnPointIds = new();

        public bool IsChangingWardrobe { get; private set; }

        public bool AllowReducingCharactersQuality
        {
            get => _avatarHelper.ReduceTexturesQuality;
            set => _avatarHelper.ReduceTexturesQuality = value;
        }

        private static bool CompressCharacterTextures => DeviceInformationHelper.IsLowEndDevice();
        
        public async Task ChangeOutfit(OutfitFullInfo outfitFullInfo)
        {
            await _eventEditor.ChangeOutfit(outfitFullInfo);
            //Commented because it changes camera position
            //RefreshCameraFocusTargetGameObject();
            OnCharactersOutfitsUpdated();

            var gender = EditingCharacterController.Character.GenderId;
            _characterEditor.SetAppliedWardrobeItems(outfitFullInfo.GetWardrobesForGender(gender));
            OnOutfitUpdated?.Invoke(outfitFullInfo.Wardrobes.ToArray());
        }
        
        public async Task RemoveOutfit()
        {
            await _eventEditor.RemoveOutfit();
            RefreshCameraFocusTargetGameObject();
            OnCharactersOutfitsUpdated();
        }

        public async Task LoadAndKeepEditingAnimations()
        {
            var editingAnims = _metadataStartPack.EditorMetadata.GetAllDressUpAnimations();
            _notUsedAssetsUnloader.LockFromUnloading(DbModelType.BodyAnimation, editingAnims.Select(x=>x.Id).ToArray());
            var counter = editingAnims.Length;
            foreach (var anim in editingAnims)
            {
                _assetManager.Load(anim, onCompleted: _ => counter--);
            }

            while (counter > 0)
            {
                await Task.Delay(33);
            }
        }

        public void ReleaseEditingAnimations()
        {
            var eventAnimations = TargetEvent.GetUniqueBodyAnimationIds();
            var characterEditAnimations = _metadataStartPack.EditorMetadata.GetAllDressUpAnimations();
            var animsToUnlock = characterEditAnimations.Where(anim => eventAnimations.All(id => id != anim.Id))
                                                       .Select(x=> x.Id).ToArray();
            _notUsedAssetsUnloader.UnlockForUnloading(DbModelType.BodyAnimation, animsToUnlock);
        }

        public async Task SetupCharactersForEditing()
        {
            _wasOriginalSpawnPositionsChanged = false;
            _wasOriginalSpawnFormationApplied = false;
            
            await SetCharacterEditingAnimations();
            StoreOriginalSpawnPositions();
            SetCharacterEditingFormation();
            SetCharactersOnStandingSpawnPoints();
            
            ToggleActiveVfx(false);
        }

        public async Task StopCharacterEditingMode()
        {
            await RestoreAnimations();
            RestoreCharacterSpawnPoints();
            RestoreFormation();
            
            ToggleActiveVfx(true);
        }

        private async Task SetCharacterEditingAnimations()
        {
            var eventAnimations = TargetEvent.CharacterController.Select(x => x.GetBodyAnimationId()).ToArray();
            _notUsedAssetsUnloader.LockFromUnloading(DbModelType.BodyAnimation, eventAnimations);
            _characterBodyAnimations[EditingCharacterController.CharacterId] = EditingCharacterController.GetBodyAnimation();
            var focusedCharacterAnim = _metadataStartPack.EditorMetadata.CharacterEditingAnimation;
            await ChangeBodyAnimationSilent(focusedCharacterAnim, EditingCharacterController);
            if (TargetEvent.CharactersCount() == 1) return;

            var allAvailableBackgroundAnims = _metadataStartPack.EditorMetadata.BackgroundCharacterAnimations;
            if(allAvailableBackgroundAnims.Length == 0) return;
            
            var notUsedBackgroundAnims = allAvailableBackgroundAnims.ToList();
            var characters = TargetEvent.GetCharacters();
            
            foreach (var character in characters)
            {
                if (character.Id == EditingCharacterController.CharacterId) continue;
                BodyAnimationInfo anim;
                if (notUsedBackgroundAnims.Count > 0)
                {
                    var randomIndex = Random.Range(0, notUsedBackgroundAnims.Count);
                    anim = notUsedBackgroundAnims[randomIndex];
                    notUsedBackgroundAnims.RemoveAt(randomIndex);
                }
                else
                {
                    anim = allAvailableBackgroundAnims.First();
                }

                var controller = TargetEvent.GetCharacterControllerByCharacterId(character.Id);
                _characterBodyAnimations[character.Id] = controller.GetBodyAnimation();
                await ChangeBodyAnimationSilent(anim, controller);
            }
        }

        private void ToggleActiveVfx(bool play)
        {
            var vfxAssets = _assetManager.GetActiveAssets<IVfxAsset>();
            
            if (vfxAssets.IsNullOrEmpty()) return;

            vfxAssets.ForEach(asset => asset.Components.ForEach(vfx => 
            {
                if (play)
                {
                    vfx.Play();
                }
                else
                {
                    vfx.Stop();
                }
            }));
        }

        private async Task RestoreAnimations()
        {
            foreach (var cc in TargetEvent.CharacterController)
            {
                var bodyAnimation = _characterBodyAnimations[cc.CharacterId];
                await ChangeBodyAnimationSilent(bodyAnimation, cc);
            }

            var eventAnimations = TargetEvent.GetUniqueBodyAnimationIds();
            var characterEditAnimations = _metadataStartPack.EditorMetadata.GetAllDressUpAnimations();
            var animsToUnlock = eventAnimations.Where(id => characterEditAnimations.All(anim => anim.Id != id)).ToArray();
            _notUsedAssetsUnloader.UnlockForUnloading(DbModelType.BodyAnimation, animsToUnlock);
            _characterBodyAnimations.Clear();
        }

        private void SetCharacterEditingFormation()
        {
            _originalSpawnFormation = TargetEvent.CharacterSpawnPositionFormationId.Value;
            _wasOriginalSpawnFormationApplied = TargetEvent.IsFormationApplied();
            
            var spawnFormation = _metadataStartPack.EditorMetadata.CharacterEditingSpawnFormations
                              .First(x => x.CharacterCount == TargetEvent.GetCharactersCount());
            ApplyFormationInternal(spawnFormation);
        }

        private void RestoreFormation()
        {
            var original = _metadataStartPack.CharacterSpawnPositionFormationTypes.SelectMany(x => x.CharacterSpawnPositionFormations)
                              .First(x => x.Id == _originalSpawnFormation);
            if (_wasOriginalSpawnFormationApplied)
            {
                ApplyFormationInternal(original);
                return;
            }
            
            TargetEvent.CharacterSpawnPositionFormationId = _originalSpawnFormation;
        }

        private void SetCharactersOnStandingSpawnPoints()
        {
            if (TargetEvent.GetTargetSpawnPosition().MovementTypeId == ServerConstants.MetaData.STANDING_MOVEMENT_TYPE_ID) {return;}
            var allSpawnPositions = TargetEvent.GetSetLocation().GetSpawnPositions();
            var spawnPos = allSpawnPositions.FirstOrDefault(x => x.MovementTypeId == ServerConstants.MetaData.STANDING_MOVEMENT_TYPE_ID);
            if (spawnPos == null) return;
            _wasOriginalSpawnPositionsChanged = true;
            _originalSpawnPosId = TargetEvent.CharacterSpawnPositionId;
            ChangeCharacterSpawnPosition(spawnPos, false);
        }

        private void RestoreCharacterSpawnPoints()
        {
            if (!_wasOriginalSpawnPositionsChanged) return;
            
            var setLocationAsset = GetTargetEventSetLocationAsset();
            var characterAssets = _assetManager.GetAllLoadedAssets<ICharacterAsset>();
            foreach (var controllerIdToSpawnPointId in _originalCharacterSpawnPointIds)
            {
                var spawnPosId = controllerIdToSpawnPointId.Value;
                var controller = TargetEvent.GetCharacterControllerByCharacterId(controllerIdToSpawnPointId.Key);
                controller.CharacterSpawnPositionId = spawnPosId;
                var characterAsset = characterAssets.First(x => x.Id == controller.CharacterId);
                setLocationAsset.Attach(spawnPosId, characterAsset);
                setLocationAsset.ResetPosition(characterAsset);
            }

            TargetEvent.CharacterSpawnPositionId = _originalSpawnPosId;
        }

        public async Task SwitchWardrobe(long characterId, WardrobeFullInfo wardrobeModel)
        {
            var characterAsset = EditingTargetCharacterAsset;
            if (characterAsset == null)
            {
                Debug.LogError($"Failed to change wardrobe on character {characterId}. Reason: character is not loaded");
                return;
            }

            IsChangingWardrobe = true;
            _undoOutfitsStack.Push(EditingCharacterController.Outfit);
            _characterEditor.SetGenderId(EditingCharacterController.Character.GenderId);
            if (characterAsset.View.IsEditable)
            {
                await EditExistedView(characterId, wardrobeModel, characterAsset);
                IsChangingWardrobe = false;
                return;
            }
            
            var cc = TargetEvent.GetCharacterControllerByCharacterId(characterId);
            var nextOutfit = await SetupNextOutfit(wardrobeModel, cc);
            cc.SetOutfit(nextOutfit);
            await BuildCharacterViaUmaBundles(characterAsset, nextOutfit);
            nextOutfit.UmaSharedColors = GetTargetCharacterAvatarColors();
            IsChangingWardrobe = false;
        }
        
        public async Task EditCharacterColor(string colorName, Color32 newColor)
        {
            var characterAsset = EditingTargetCharacterAsset;
            _undoOutfitsStack.Push(EditingCharacterController.Outfit);
            var outfit = await SetupNextOutfit(EditingCharacterController);
            if (!characterAsset.View.IsEditable)
            {
                await BuildCharacterViaUmaBundles(characterAsset, outfit);
            }
            
            _characterEditor.SetTargetAvatar(characterAsset.Avatar);
            _characterEditor.EditCharacterColor(colorName, newColor, EditingCharacterController.GetUsedWardrobes(), () =>
            {
                outfit.UmaSharedColors = GetTargetCharacterAvatarColors();
                EditingCharacterController.SetOutfit(outfit);
                characterAsset.View.ReplaceOutfit(outfit);
            });
        }

        public void UndoLastCharacterOutfitChange()
        {
            if (_characterEditor.IsUndoEmpty) return;
            
            _characterEditor.Undo();
            
            var currentOutfit = EditingCharacterController.Outfit;
            _redoOutfitsStack.Push(currentOutfit);

            var newOutfit = _undoOutfitsStack.Pop();
            EditingCharacterController.SetOutfit(newOutfit);
            EditingTargetCharacterAsset.View.ReplaceOutfit(newOutfit);
        }

        public void RedoLastCharacterOutfitChange()
        {
            if (_characterEditor.IsRedoEmpty) return;

            _characterEditor.Redo();

            var currentOutfit = EditingCharacterController.Outfit;
            _undoOutfitsStack.Push(currentOutfit);

            var newOutfit = _redoOutfitsStack.Pop();
            EditingCharacterController.SetOutfit(newOutfit);
            
            EditingTargetCharacterAsset.View.ReplaceOutfit(newOutfit);
        }

        public async void UndressCharacter()
        {
            if (!EditingTargetCharacterAsset.HasAnyWardrobe) return;
            
            var nextOutfit = new OutfitFullInfo
            {
                Id = LocalStorageManager.GetNextLocalId(nameof(OutfitFullInfo)),
                SaveMethod = SaveOutfitMethod.Automatic,
                GenderWardrobes = new Dictionary<long, List<long>>()
            };
            if (EditingTargetCharacterAsset.View.IsEditable)
            {
                _characterEditor.SetTargetAvatar(EditingTargetCharacterAsset.Avatar);
                _characterEditor.UndressCharacter();
                EditingTargetCharacterAsset.View.ReplaceOutfit(nextOutfit);
            }
            else
            {
                await BuildCharacterViaUmaBundles(EditingTargetCharacterAsset, nextOutfit);
            }
         
            EditingCharacterController.SetOutfit(nextOutfit);
            _undoOutfitsStack.Push(nextOutfit);
        }

        public async void ResetCharacterToInitialState()
        {
            if (EditingTargetCharacterAsset.OutfitId == null) return;
            _undoOutfitsStack.Push(null);
            if (EditingTargetCharacterAsset.View.IsEditable)
            {
                _characterEditor.SetTargetAvatar(EditingTargetCharacterAsset.Avatar);
                _characterEditor.SetEditingCharacterModel(EditingTargetCharacterAsset.RepresentedModel);
                _characterEditor.ResetCharacter();
                EditingTargetCharacterAsset.View.ReplaceOutfit(null);
            }
            else
            {
                await BuildCharacterViaUmaBundles(EditingTargetCharacterAsset, null);
            }
            EditingCharacterController.SetOutfit(null);
        }

        public void ClearCharacterModificationHistory()
        {
            _undoOutfitsStack.Clear();
            _redoOutfitsStack.Clear();
            _characterEditor.ClearModificationsHistory();
        }

        public void WarmupUmaBundlesForWardrobesModification()
        {
            _characterEditor.LoadUmaBundles(EditingCharacterController.Character, EditingCharacterController.Outfit);
        }

        public void UnloadUmaBundles()
        {
            _characterEditor.UnloadUmaBundles();
        }

        private async Task BuildCharacterViaUmaBundles(ICharacterAsset characterAsset, OutfitFullInfo nextOutfit)
        {
            _characterViewContainer.SetOptimizeMemory(CompressCharacterTextures);
            var nextView = await _characterViewContainer.GetView(characterAsset.RepresentedModel, nextOutfit);
            var previousView = characterAsset.View;
            
            var heelsHeight = _characterEditor.GetHeelsHeight(nextView.Avatar);
            var characterSize = nextView.Avatar.GetComponentInChildren<SkinnedMeshRenderer>().bounds.size;
            nextView.ChangeData(nextOutfit, heelsHeight, characterSize.y, characterSize.x);
            characterAsset.ChangeView(nextView);
            
            if (previousView.ViewType == ViewType.GeneratedRuntime)
            {
                _characterViewContainer.Unload(previousView);
            }

            RefreshAppliedWardrobesList(characterAsset.GenderId, nextOutfit);
            OnOutfitUpdated?.Invoke(EditingCharacterController.Outfit?.Wardrobes.ToArray());
        }

        private void RefreshAppliedWardrobesList(long gender, OutfitFullInfo nextOutfit)
        {
            _characterEditor.SetAppliedWardrobeItems(nextOutfit.GetWardrobesForGender(gender));
        }

        private async Task<OutfitFullInfo> SetupNextOutfit(WardrobeFullInfo wardrobeModel, CharacterController cc)
        {
            var nextOutfit = await SetupNextOutfit(cc);

            var alreadyAddedWardrobe = nextOutfit.Wardrobes.FirstOrDefault(x => x.Id == wardrobeModel.Id);
            if (alreadyAddedWardrobe != null)
            {
                nextOutfit.Wardrobes.Remove(alreadyAddedWardrobe);
                nextOutfit.GenderWardrobes[cc.Character.GenderId].Remove(alreadyAddedWardrobe.Id);
            }
            else
            {
                var slot = wardrobeModel.GetSlotName();
                RemovedClippedWardrobes(slot, nextOutfit);
                nextOutfit.Wardrobes.Add(wardrobeModel);
                if (!nextOutfit.GenderWardrobes.ContainsKey(cc.Character.GenderId))
                {
                    nextOutfit.GenderWardrobes.Add(cc.Character.GenderId, new List<long>());
                }
               
                nextOutfit.GenderWardrobes[cc.Character.GenderId].Add(wardrobeModel.Id);
            }
            
            return nextOutfit;
        }

        private UmaSharedColorInfo[] GetTargetCharacterAvatarColors()
        {
            _characterEditor.SetTargetAvatar(EditingTargetCharacterAsset.Avatar);
            return _characterEditor.GetCharacterColorsInt().ConvertToOutfitAndUmaSharedColor().ToArray();
        }

        private static async Task<OutfitFullInfo> SetupNextOutfit(CharacterController cc)
        {
            OutfitFullInfo nextOutfit;
            if (cc.Outfit != null)
            {
                nextOutfit = await cc.Outfit.CloneAsync();
            }
            else
            {
                nextOutfit = new OutfitFullInfo();
                nextOutfit.GenderWardrobes = new Dictionary<long, List<long>>();
                if (cc.Character.Wardrobes != null)
                {
                    nextOutfit.Wardrobes.AddRange(cc.Character.Wardrobes);
                    nextOutfit.GenderWardrobes[cc.Character.GenderId] = nextOutfit.Wardrobes.Select(x => x.Id).ToList();
                }
            }

            nextOutfit.SaveMethod = SaveOutfitMethod.Automatic;
            nextOutfit.Id = LocalStorageManager.GetNextLocalId(nameof(OutfitFullInfo));
            nextOutfit.Files = CHARACTER_OUTFIT_DEFAULT_DATA_PROVIDER.GetDefaultFiles();
            
            return nextOutfit;
        }

        private string[] GetClippingSlots(string slot)
        {
            var configuredClippingSlots = _characterManagerConfig.SlotsClippingMatrix.FirstOrDefault(x => x.Slot == slot);

            return configuredClippingSlots != null
                ? configuredClippingSlots.ClippingSlots.Append(slot).ToArray()
                : new[] { slot };
        }

        private async Task EditExistedView(long characterId, WardrobeFullInfo wardrobeModel, ICharacterAsset characterAsset)
        {
            var isCompleted = false;
            var wardrobesOnCharacter = EditingCharacterController.GetUsedWardrobes();
            _characterEditor.SetAppliedWardrobeItems(wardrobesOnCharacter);
            _characterEditor.CharacterChanged += OnCharacterChanged;
            _characterEditor.SetCompressionEnabled(CompressCharacterTextures);
            _characterEditor.ChangeWardrobeItem(characterAsset.Avatar, wardrobeModel);

            while (!isCompleted)
            {
                await Task.Delay(25);
            }
            
            return;

            async void OnCharacterChanged(IEntity[] appliedWardrobes)
            {
                _characterEditor.CharacterChanged -= OnCharacterChanged;
                var cc = TargetEvent.GetCharacterControllerByCharacterId(characterId);
                
                var nextOutfit = await SetupNextOutfit(wardrobeModel, cc);
                nextOutfit.UmaSharedColors = GetTargetCharacterAvatarColors();
                cc.SetOutfit(nextOutfit);
                
                var heelsHeight = _characterEditor.GetHeelsHeight(characterAsset.Avatar);
                var characterSize = characterAsset.Avatar.GetComponentInChildren<SkinnedMeshRenderer>().bounds.size;
                characterAsset.View.ChangeData(nextOutfit, heelsHeight, characterSize.y, characterSize.x);

                RefreshAppliedWardrobesList(characterAsset.GenderId, nextOutfit);
                _characterEditor.UnloadNotUsedUmaBundles(characterAsset.View.Avatar);
                isCompleted = true;
                OnOutfitUpdated?.Invoke(cc.Outfit?.Wardrobes.ToArray());
            }
        }

        private void RemovedClippedWardrobes(string clippingSlot, OutfitFullInfo nextOutfit)
        {
            var clippingSlots = GetClippingSlots(clippingSlot);
            nextOutfit.Wardrobes = nextOutfit.Wardrobes.Where(x=> clippingSlots == null || !clippingSlots.Contains(x.GetSlotName())).ToList();
            foreach (var genderWardrobes in nextOutfit.GenderWardrobes.Values)
            {
                for (var i = genderWardrobes.Count - 1; i >= 0; i--)
                {
                    var wardrobeId = genderWardrobes[i];
                    if (nextOutfit.Wardrobes.Any(x=>x.Id == wardrobeId)) continue;
                    genderWardrobes.RemoveAt(i);
                }
            }
        }
        
        public async Task<bool> SaveEditedOutfit(bool saveManual)
        {
            var cc = EditingCharacterController;
            var outfit = cc.Outfit;
            // we need to cache character view in order to prevent outfit replacement for the wrong character 
            // because method runs in background and editing character can be changed during this time
            var characterView = EditingTargetCharacterAsset.View;
            
            var saveToFavorite = saveManual && outfit?.SaveMethod == SaveOutfitMethod.Automatic;

            if (!cc.UnsavedOutfitEnabled && !saveToFavorite)
            {
                if (saveManual) OnTrySavingEmptyOutfit?.Invoke();
                return false;
            }
            
            var outfitSaveModel = new OutfitSaveModel
            {
                SaveMethod = saveManual ? SaveOutfitMethod.Manual : SaveOutfitMethod.Automatic,
                WardrobeIds = outfit.Wardrobes.Select(x => x.Id).ToList(),
                Files = outfit.Files,
                UmaSharedColors = GetTargetCharacterAvatarColors().ToList()
            };

            var resp = await _bridge.SaveOutfitAsync(outfitSaveModel);
            if (resp.IsError)
            {
                Debug.LogError($"Failed to save outfit\n{JsonUtility.ToJson(outfitSaveModel, true)}");
                return false;
            }
            
            cc.SetOutfit(resp.Model);
            characterView.ReplaceOutfit(resp.Model);

            return true;
        }
        
        private async Task ChangeBodyAnimationSilent(BodyAnimationInfo bodyAnimation, CharacterController cc)
        {
            var finished = false;
            _assetManager.Load(bodyAnimation, onCompleted: x => OnFinished(), onFailed: x=>
            {
                OnFinished();
                Debug.LogError(x);
            });
            while (!finished)
            {
                await Task.Delay(50);
            }

            void OnFinished()
            {
                finished = true;
            }
            cc.SetBodyAnimation(bodyAnimation);
            RefreshAssetsOnScene(DbModelType.BodyAnimation);
        }
        
        private void StoreOriginalSpawnPositions()
        {
            _originalCharacterSpawnPointIds.Clear();
            foreach (var cc in TargetEvent.CharacterController)
            {
                _originalCharacterSpawnPointIds[cc.CharacterId] = cc.CharacterSpawnPositionId;
            }
        }
    }
}