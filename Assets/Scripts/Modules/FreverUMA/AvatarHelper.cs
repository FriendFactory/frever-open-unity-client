using System;
using Modules.FreverSelfie;
using Modules.WardrobeManaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using UMA;
using UMA.AssetBundles;
using UMA.CharacterSystem;
using UnityEngine;
using Zenject;
using Bridge.Models.Common;
using Configs;
using Bridge.Services.SelfieAvatar;
using Bridge;
using System.Diagnostics;
using Bridge.Models.ClientServer.Assets;
using Common;
using Common.Exceptions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.AssetsStoraging.Core;
using Newtonsoft.Json;
using UIManaging.SnackBarSystem;
using Debug = UnityEngine.Debug;
using Utils;
using Object = UnityEngine.Object;
using TaskExtensions = Extensions.TaskExtensions;

namespace Modules.FreverUMA
{
    [UsedImplicitly]
    public sealed class AvatarHelper
    {
        private const string SHOES_SLOT_NAME = "Shoes";
        private const string OUTFIT_SLOT_NAME = "Outfit";
        private static readonly string[] TRUNK_BODY_PART_SLOTS = { "Dress", "Shirts" };
        private static readonly string[] PELVIC_BODY_PART_SLOTS = { "Pants", "Skirts", "Dress" };
        private const int HIGHLIGHT_DURATION = 1000;
        
        [Inject] private readonly UMACharacterSource _characterSource;
        [Inject] private readonly ClothesCabinet _clothesCabinet;
        [Inject] private readonly CharacterManagerConfig _characterManagerConfig;
        [Inject] private readonly IBridge _bridge;
        [Inject] private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        [Inject] private readonly AmplitudeAssetEventLogger _amplitudeAssetEventLogger;
        [Inject] private readonly StopWatchProvider _stopWatchProvider;
        [Inject] private readonly UmaBundleHelper _umaBundleHelper;
        [Inject] private readonly SnackBarHelper _snackBarHelper;
        [Inject] private readonly AmplitudeManager _amplitudeManager;
        [Inject] private readonly IExceptionCatcher _exceptionCatcher;
        [Inject] private readonly BoneColliderSettings[] _boneColliderSettings;
        [Inject] private readonly IMetadataProvider _metadataProvider;

        private readonly List<KeyValuePair<string, List<string>>> _activeBundlesInSlots = new();
        
        private static string[] _globalBundlesAssets;
        private Dictionary<long, UmaBundleFullInfo> _umaBundles;
        private readonly Dictionary<long, WardrobeFullInfo> _wardrobeFullInfoCache = new();

        private UmaRecipeBuilder _selfieBuilder;
        private Stopwatch _stopWatch;

        private bool _bundlesInRam;
        private bool _initialized;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private int AvatarModificationOperationCounter
        {
            get => _avatarModificationOperationCounter;
            set
            {
                if (_avatarModificationOperationCounter == 0 && value != 0)
                {
                    StartedProcessingAvatar?.Invoke();
                }else if (_avatarModificationOperationCounter != 0 && value == 0)
                {
                    FinishedProcessingAvatar?.Invoke();
                }
                _avatarModificationOperationCounter = value;
            } 
        }
        private int _avatarModificationOperationCounter;
        
        private IEnumerable<UmaBundleFullInfo> AllGlobalBundles { get; set; }

        /// <summary>
        /// UMA will generate lower resolution textures
        /// </summary>
        public bool ReduceTexturesQuality
        {
            get => UMAContext.Instance.umaGenerator.InitialScaleFactor > 1;
            set => UMAContext.Instance.umaGenerator.InitialScaleFactor = value ? 2 : 1;
        }
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action StartedProcessingAvatar;
        public event Action FinishedProcessingAvatar;
        
        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        public static async Task WaitWhileAvatarCreated(DynamicCharacterAvatar avatar, CancellationToken token = default)
        {
            var created = false;
            avatar.CharacterUpdated.AddListener(OnCharacterCreated);

            void OnCharacterCreated(UMAData data)
            {
                avatar.CharacterUpdated.RemoveListener(OnCharacterCreated);
                created = true;
            }

            while (!created && !token.IsCancellationRequested)
            {
                await Task.Delay(25, token);
            }
        }

        private static async Task WaitForBundlesLoaded(CancellationToken token = default)
        {
            bool Waiting()
            {
                return !token.IsCancellationRequested && AssetBundleManager.AreBundlesDownloading() || !DynamicAssetLoader.Instance.downloadingAssets.areDownloadedItemsReady;
            }
                
            while (Waiting())
            {
                await TaskExtensions.DelayWithoutThrowingCancellingException(25, token);
            }

            await TaskExtensions.DelayWithoutThrowingCancellingException(100, token);
                
            while (Waiting())
            {
                await TaskExtensions.DelayWithoutThrowingCancellingException(25, token);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<DynamicCharacterAvatar> PrepareAvatar(long genderId, CancellationToken token = default)
        {
            if (!_initialized) Init();

            var genderGlobalBundles = GenderGlobalBundles(genderId).ToArray();
            await DownloadBundles(genderGlobalBundles, token);
            if (token.IsCancellationRequested) return null;
            
            await StartToDecompressBundleForFasterAccess(genderGlobalBundles); // DWC in here we do encryption should we not await on this?
            DynamicAssetLoader.Instance.LoadAssetBundles(new List<string>(genderGlobalBundles.Select(x=> x.Name).ToArray()));
            await WaitForBundlesLoaded(token);

            var genderRaceName = _metadataProvider.MetadataStartPack.GetUmaRaceNameByGenderId(genderId);
            var avatar = _characterSource.SpawnCharacter(genderRaceName);
            avatar.CharacterCreated.AddAction((x) => AddBoneColliders(avatar, genderId));
            avatar.loadFileOnStart = false;
            _bundlesInRam = true;
#if UNITY_EDITOR_WIN
            avatar.CharacterUpdated.AddListener(FixShaders);
#endif

            return avatar;
        }

        private Task StartToDecompressBundleForFasterAccess(IEnumerable<UmaBundleFullInfo> umaBundles)
        {
            return Task.WhenAll(umaBundles.Select(b => _uncompressedBundlesManager.DecompressBundle(b)));
        }

        public async Task PrepareBundlesForCharacterList(ICollection<CharacterAndOutfit> characterAndOutfit, CancellationToken token = default)
        {
            if (!_initialized) Init();

            //todo: FREV-15046, reuse GetUmaBundleForCharacter for extracting character bundles
            var genders = characterAndOutfit.Select(x => x.Character.GenderId).Distinct();
            var bundlesToLoad = new List<UmaBundleFullInfo>();
            foreach (var genderId in genders)
            {
                bundlesToLoad.AddRange(GenderGlobalBundles(genderId));
            }
            foreach (var characterToOutfit in characterAndOutfit)
            {
                var character = characterToOutfit.Character;
                IEnumerable<long> wardrobeIds = null;
                if (characterToOutfit.Outfit != null)
                {
                    var fullOutfit = characterToOutfit.Outfit;
                    bundlesToLoad.AddRange(fullOutfit.Wardrobes.Select(x=>x.UmaBundle));
                }
                else
                {
                    wardrobeIds = character.Wardrobes?.Select(uw => uw.Id);
                }

                if (wardrobeIds == null) continue;

                foreach (var id in wardrobeIds)
                {
                    var umaBundle = await PrepareUmaBundleForWardrobe(id, token);
                    if(bundlesToLoad.Contains(umaBundle)) continue;
                    
                    bundlesToLoad.Add(umaBundle);
                    var bundles = umaBundle.GetBundleWithDependencies();
                    _activeBundlesInSlots.Add(new KeyValuePair<string, List<string>>("", bundles));
                }
            }
            await DownloadBundles(bundlesToLoad, token);
            _bundlesInRam = true;
            DynamicAssetLoader.Instance.LoadAssetBundles(bundlesToLoad.Select(x=>x.Name).ToArray());
            await WaitForBundlesLoaded(token);
        }

        public async Task LoadAvatar(DynamicCharacterAvatar avatar, string recipe, long characterGenderId,
            ICollection<WardrobeFullInfo> wardrobes = null, OutfitFullInfo outfit = null, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();
        
            _bundlesInRam = true;
            var bundlesToLoad = new List<UmaBundleFullInfo>();
            avatar.waitForBundles = true;
            var characterWardrobe = wardrobes;
            if (outfit != null)
            {
                characterWardrobe = SelectWardrobesForGender(outfit, characterGenderId);
            }
            ProcessWardrobesFullInfo(characterWardrobe);

            if (characterWardrobe != null)
            {
                var umaBundles = characterWardrobe.Select(x => x?.UmaBundle).ToArray();
                bundlesToLoad.AddRange(umaBundles);
            }
            bundlesToLoad.AddRange(GenderGlobalBundles(characterGenderId));

            await DownloadBundles(bundlesToLoad, token);

            DynamicAssetLoader.Instance.LoadAssetBundles(bundlesToLoad.Select(x=>x.Name).ToArray());

            await WaitForBundlesLoaded(token);

            avatar.ClearSlots();
            var unpackedRecipe = UMATextRecipe.PackedLoadDCS(avatar.context, recipe);

            try
            {
                await LoadWardrobesToRecipe(avatar, unpackedRecipe, characterWardrobe, token);
            }
            catch (Exception e)
            {
                _snackBarHelper.ShowCharacterLoadingFailedSnackBar();
                
                var metaData = new Dictionary<string, object>
                {
                    [AmplitudeEventConstants.EventProperties.ERROR_TYPE] = e.Message,
                    [AmplitudeEventConstants.EventProperties.ERROR_STACK_TRACE] = e.StackTrace,
                    [AmplitudeEventConstants.EventProperties.ERROR_HANDLED] = true,
                };
                _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ERROR_MESSAGE,
                                                              metaData);
                _exceptionCatcher.TryCatchBlockTriggered();
                
                RegisterCharacterModificationOperationEnd();
                return;
            }


            if (token.IsCancellationRequested)
            {
                RegisterCharacterModificationOperationEnd();
                return;
            }

            avatar.ImportSettings(unpackedRecipe);

            if (outfit != null)
            {
                avatar.RecipeUpdated.AddAction(UpdateOutfitColors);
            }
            
            avatar.RecipeUpdated.AddAction(OnRecipeUpdated);

            void OnRecipeUpdated(UMAData data)
            {
                if (token.IsCancellationRequested) return;
                avatar.RecipeUpdated.RemoveAction(OnRecipeUpdated);
                GetOverrideUnderwear(characterWardrobe, out var overrideTop, out var overrideBot);
                UpdateUnderwearStates(avatar, overrideTop, overrideBot);
            }

            void UpdateOutfitColors(UMAData data)
            {
                token.ThrowIfCancellationRequested();
                avatar.RecipeUpdated.RemoveAction(UpdateOutfitColors);
                SetOutfitColors(avatar, outfit);
            }
            
            RegisterCharacterModificationOperationEnd();
        }

        public async Task LoadCharacterToAvatar(DynamicCharacterAvatar avatar, CharacterFullInfo character, OutfitFullInfo outfit, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();

            var umaRecipe = character.UmaRecipe;
            if (umaRecipe is null)
            {
                RegisterCharacterModificationOperationEnd();
                throw new Exception($"Character {character.Id} is broken (has no recipe). Please contact support.");
            }
            var recipe = System.Text.Encoding.UTF8.GetString(umaRecipe.J);

            ICollection<WardrobeFullInfo> wardrobes = outfit != null ? outfit.Wardrobes : character.Wardrobes;

            await LoadAvatar(avatar, recipe, character.GenderId, wardrobes, outfit, token);
            await WaitWhileAvatarCreated(avatar, token);
            RegisterCharacterModificationOperationEnd();
        }

        public async Task LoadCharacterFromSelfie(DynamicCharacterAvatar avatar, JSONSelfie selfie, List<WardrobeFullInfo> wardrobes, long genderId)
        {
            RegisterCharacterModificationOperationStart();
            var recipe = _selfieBuilder.BuildRecipe(selfie);
            avatar.RecipeUpdated.AddListener(ApplyDNA);
            await LoadAvatar(avatar, recipe, genderId);

            var itemsToSet = _selfieBuilder.GetItemNames(avatar, selfie);
            var fullWardrobes = await PrepareWardrobesByName(genderId, itemsToSet);
            foreach (var item in fullWardrobes)
            {
                var umaBundle = await PrepareUmaBundleForWardrobe(item.Id);
                var accessory = _clothesCabinet.GetWardrobeItem(item.Name) as AccessoryItem;
                wardrobes.Add(item);
                await SetSlot(avatar, accessory);
            }
            avatar.BuildCharacter();
            avatar.ForceUpdate(false, true, true);
            GetOverrideUnderwear(wardrobes, out var overrideTop, out var overrideBot);
            UpdateUnderwearStatesDelayed(avatar, overrideTop, overrideBot);

            void ApplyDNA(UMAData data)
            {
                avatar.RecipeUpdated.RemoveListener(ApplyDNA);
                _selfieBuilder.ApplyDNA(avatar, selfie);
            }
            RegisterCharacterModificationOperationEnd();
        }

        public async Task ApplySlot(DynamicCharacterAvatar avatar, AccessoryItem itemToApply, IEnumerable<AccessoryItem> itemsToRemove, IEnumerable<AccessoryItem> initialItems, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();
            
            await SetSlot(avatar, itemToApply, token);
            foreach (var itemToClear in itemsToRemove)
            {
                avatar.ClearSlot(itemToClear.Slot);
                UnloadSlotBundle(itemToClear.Slot);
            }
            avatar.BuildCharacter();
            avatar.ForceUpdate(false, true, true);
            var wardrobes = initialItems.Where(x => !itemsToRemove.Contains(x)).Append(itemToApply).Select(x=>x.Wardrobe);
            GetOverrideUnderwear(wardrobes, out var overrideTop, out var overrideBot);
            UpdateUnderwearStates(avatar, overrideTop, overrideBot);
            await WaitWhileAvatarCreated(avatar, token);
            RegisterCharacterModificationOperationEnd();
        }
        
        public async Task SetSlot(DynamicCharacterAvatar avatar, AccessoryItem item, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();

            if (item?.Wardrobe != null)
            {
                await PrepareUmaBundleForWardrobe(item.Wardrobe.Id, token);
            }
            await DownloadBundle(item.Wardrobe.UmaBundle, token);
            DynamicAssetLoader.Instance.LoadAssetBundle(item.Wardrobe.Name);

            await WaitForBundlesLoaded(token);
            avatar.context.dynamicCharacterSystem.Refresh(false, item.Wardrobe.Name);

            if (!avatar.AvailableRecipes.TryGetValue(item.Slot, out var slot))
            {
                Debug.LogWarning($"Wardrobe {item.AssetName} not have associated slot # {item.Slot}");
                RegisterCharacterModificationOperationEnd();
                return;
            }

            var recipe = slot.Find(w => w.name == item.AssetName);
            if (recipe == null)
            {
                Debug.LogWarning($"Wardrobe {item.AssetName} not have associated recipe");
                RegisterCharacterModificationOperationEnd();
                return;
            }

            DestroyRecipeThumbnail(recipe);
            avatar.SetSlot(recipe);
            var bundles = item.Wardrobe.UmaBundle.GetBundleWithDependencies();
            _activeBundlesInSlots.Add(new KeyValuePair<string, List<string>>(item.Slot, bundles));
            RegisterCharacterModificationOperationEnd();
        }

        public async Task ApplyOutfit(DynamicCharacterAvatar avatar, OutfitFullInfo outfit, long genderId, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();
            
            _stopWatch = _stopWatchProvider.GetStopWatch();
            _stopWatch.Restart();
            
            _bundlesInRam = true;

            var wardrobesAndBundles = FilterWardrobeAndBundlesByGender(outfit, genderId);
            
            await LoadBundleList(wardrobesAndBundles.umaBundles, token);

            if (token.IsCancellationRequested)
            {
                RegisterCharacterModificationOperationEnd();
                return;
            }

            RestoreAvatarRace(avatar);

            SetOutfitColors(avatar, outfit);

            await SetOutfitWardrobeList(avatar, wardrobesAndBundles.wardrobes, token);

            GetOverrideUnderwear(wardrobesAndBundles.wardrobes, out var overrideTop, out var overrideBot);
            UpdateUnderwearStatesDelayed(avatar, overrideTop, overrideBot, token);

            avatar.ForceUpdate(true, true, true);
            avatar.BuildCharacter();
            
            _amplitudeAssetEventLogger.LogSelectedOutfitAmplitudeEvent(outfit.Id, _stopWatch.ElapsedMilliseconds.ToSecondsClamped());
            _stopWatch.Stop();
            _stopWatchProvider.Dispose(_stopWatch);
            UnloadNotUsedByAvatarBundles(avatar);
            RegisterCharacterModificationOperationEnd();
        }

        public async Task UnapplyOutfit(DynamicCharacterAvatar avatar, ICollection<WardrobeFullInfo> items,
            string recipe, long genderId, CancellationToken cancellationToken = default)
        {
            RegisterCharacterModificationOperationStart();
            _bundlesInRam = true;

            ProcessWardrobesFullInfo(items);
            var umaBundles = items?.Select(x => x.UmaBundle).ToList();

            await LoadBundleList(umaBundles, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            avatar.context.dynamicCharacterSystem.Refresh();
            RestoreAvatarRace(avatar);

            await LoadAvatar(avatar, recipe, genderId, items, null, cancellationToken);
            RegisterCharacterModificationOperationEnd();
        }

        public void UnloadAllUmaBundles()
        {
            if (!_bundlesInRam) return;
            if (!_initialized) Init();
            _bundlesInRam = false;
            AssetBundleManager.UnloadAllAssetBundles();
            UMAContext.Instance.overlayLibrary.ReleaseLibrary();
            UMAContext.Instance.slotLibrary.ReleaseLibrary();
            UMAContext.Instance.raceLibrary.ReleaseLibrary();
            RemoveAllActiveSlotBundlesFromIndex();
            _activeBundlesInSlots.Clear();
            OverlayDataAsset.DestroyAllTexturesNotYetUnloaded();
        }

        public void UnloadNonGlobalAssetBundles()
        {
            if (!_initialized) Init();
            
            var globalBundlesNames = new HashSet<string>(AllGlobalBundles.Select(x=>x.Name));
            AssetBundleManager.UnloadAllAssetBundles(globalBundlesNames);
            var globalBundlesAssets = GetGlobalBundlesAssetNames();//potential optimization: sort assets by types(overlay assets, textures etc) 
            var unloadingArgs = new PartialLibraryUnloadingArgs
            {
                AssetsToKeep = globalBundlesAssets,
                BundlesToKeep = globalBundlesNames.ToArray(),
                KeepGlobalUmaAssets = true,
                GlobalUmaAssets = globalBundlesAssets
            };
            UMAContext.Instance.overlayLibrary.ReleaseLibraryPartially(unloadingArgs);
            UMAContext.Instance.slotLibrary.ReleaseLibraryPartially(unloadingArgs);

            RemoveAllActiveSlotBundlesFromIndex();
            _activeBundlesInSlots.Clear();//it doesn't contains global bundles 
        }

        public void UnloadNotUsedByAvatarBundles(DynamicCharacterAvatar avatar)
        {
            var bundlesToKeep = AllGlobalBundles.Select(x=>x.Name).ToList();
            bundlesToKeep.AddRange(_activeBundlesInSlots.SelectMany(x=> x.Value));
            
            var dynamicSystem = (DynamicCharacterSystem) avatar.context.dynamicCharacterSystem;
            foreach (var notUsedAssetBundle in dynamicSystem.assetBundlesUsedDict.Where(x=> !bundlesToKeep.Contains(x.Key)).Select(x=>x.Key).ToArray())
            {
                dynamicSystem.assetBundlesUsedDict.Remove(notUsedAssetBundle);
            }

            var activeBundleNames = _activeBundlesInSlots.SelectMany(x => x.Value).ToArray();
            var activeBundleModels = _umaBundles.Where(x => activeBundleNames.Contains(x.Value.Name));
            var assetsUsedByAvatar = activeBundleModels.SelectMany(x => x.Value.UmaAssets.Select(_ => _.Name));
            var assetFilesUsedByAvatar = activeBundleModels.SelectMany(x => x.Value.UmaAssets)
                                                           .SelectMany(x => x.UmaAssetFiles).Select(x => x.Name);
            assetsUsedByAvatar = assetsUsedByAvatar.Concat(assetFilesUsedByAvatar);
            
            var globalBundlesAssets = GetGlobalBundlesAssetNames();
            var unloadingArgs = new PartialLibraryUnloadingArgs
            {
                AssetsToKeep = globalBundlesAssets.Concat(assetsUsedByAvatar).ToArray(),
                BundlesToKeep = bundlesToKeep.ToArray(),
                KeepGlobalUmaAssets = true,
                GlobalUmaAssets = globalBundlesAssets
            };
            
            UMAContext.Instance.overlayLibrary.ReleaseLibraryPartially(unloadingArgs);
            UMAContext.Instance.slotLibrary.ReleaseLibraryPartially(unloadingArgs);

            var allLoadedBundles = AssetBundleManager.GetLoadedAssetBundles().Keys;
            foreach (var bundleToUnload in allLoadedBundles.Where(x=> !bundlesToKeep.Contains(x)))
            {
                _umaBundleHelper.RemoveBundleFromIndex(bundleToUnload);
            }
            AssetBundleManager.UnloadAllAssetBundles(bundlesToKeep.ToHashSet());
        }

        private string[] GetGlobalBundlesAssetNames()
        {
            if (_globalBundlesAssets != null) return _globalBundlesAssets;
            _globalBundlesAssets = AllGlobalBundles.SelectMany(x => x.UmaAssets).Select(x => x.Name)
                                                 .Concat(AllGlobalBundles.SelectMany(x=> x.UmaAssets).SelectMany(x=> x.UmaAssetFiles).Select(x=> x.Name))
                                                 .Distinct().ToArray();
            return _globalBundlesAssets;
        }

        public async Task SetWardrobeList(DynamicCharacterAvatar avatar, IEnumerable<WardrobeFullInfo> wardrobes)
        {
            RegisterCharacterModificationOperationStart();
            
            var wardrobeNames = wardrobes.Select(x => x.Name);
            DynamicAssetLoader.Instance.LoadAssetBundles(wardrobeNames.ToArray());

            await WaitForBundlesLoaded();

            avatar.context.dynamicCharacterSystem.Refresh(false);

            avatar.ClearSlots();

            foreach (var wardrobe in wardrobes)
            {
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(wardrobe.Id) as AccessoryItem;
                if (wardrobeItem == null) continue;
                await SetSlot(avatar, wardrobeItem);
            }
            
            RegisterCharacterModificationOperationEnd();
        }

        public void UnloadSlotBundle(string slot, string[] bundlesToKeep = null)
        {
            var pair = _activeBundlesInSlots.Find(kv => kv.Key == slot);
            var bundleNames = pair.Value;

            if (bundleNames == null || bundleNames.Count == 0) return;

            _activeBundlesInSlots.Remove(pair);
            foreach (var bundleName in bundleNames)
            {
                var isSharedWithOther = _activeBundlesInSlots.Any(x => x.Value.Contains(bundleName));
                if (isSharedWithOther) continue;
                if (bundlesToKeep != null && bundlesToKeep.Contains(bundleName)) continue;
                UnloadBundle(bundleName);
                _umaBundleHelper.RemoveBundleFromIndex(bundleName);
            }
        }

        public void UpdateUnderwearStatesDelayed(DynamicCharacterAvatar avatar, bool overrideTop, bool overrideBot, CancellationToken token = default)
        {
            avatar.RecipeUpdated.AddAction(OnRecipeUpdated);

            void OnRecipeUpdated(UMAData data)
            {
                if(token.IsCancellationRequested) return;
                avatar.RecipeUpdated.RemoveAction(OnRecipeUpdated);
                UpdateUnderwearStates(avatar, overrideTop, overrideBot);
            }
        }

        public void UpdateUnderwearStates(DynamicCharacterAvatar avatar, bool overrideTop, bool overrideBottom)
        { 
            if (avatar.umaData.umaRecipe.slotDataList == null) return;
            
            var slotData = avatar.umaData.umaRecipe.slotDataList;
            var bodySlot = slotData.FirstOrDefault(x => x.slotName.Contains("body", StringComparison.InvariantCultureIgnoreCase));

            if (bodySlot == null) return;

            var overlaysList = bodySlot.GetOverlayList();
            if (overlaysList == null) return;

            OverlayData topUnderwearOverlay = null;
            var umaGenderRace = avatar.activeRace.name;
            var race = _metadataProvider.MetadataStartPack.GetGenderByUmaRaceName(umaGenderRace);
            var botUnderwearOverlay = overlaysList.Find(x => x.overlayName == race.LowerUnderwearOverlay);
            if (race.UpperUnderwearOverlay != null)
            {
                topUnderwearOverlay = overlaysList.Find(x => x.overlayName == race.UpperUnderwearOverlay);
            }
            
            if (topUnderwearOverlay != null)
            {
                if (overrideTop)
                {
                    overrideTop = AreTrunkWardrobesLoadedSuccessfully(avatar);
                }
                SetUnderwearState(topUnderwearOverlay, overrideTop);
            }

            if (botUnderwearOverlay != null)
            {
                if (overrideBottom)
                {
                    overrideBottom = ArePelvicWardrobesLoadedSuccessfully(avatar);
                }
                SetUnderwearState(botUnderwearOverlay, overrideBottom);
            }
            avatar.UpdateColors(true);
        }

        public async Task<UmaBundleFullInfo> PrepareUmaBundleForWardrobe(long wardrobeId, CancellationToken token = default)
        {
            var bundleData = await GetWardrobeFullInfo(wardrobeId, token);
            ProcessWardrobeFullInfo(bundleData);
            _clothesCabinet.AddUmaBundleToWardrobe(bundleData, wardrobeId);
            return bundleData.UmaBundle;
        }

        public void GetOverrideUnderwear(IEnumerable<WardrobeFullInfo> wardrobes, out bool overrideTop, out bool overrideBot)
        {           
            overrideTop = false;
            overrideBot = false;
            if (wardrobes == null) return;

            foreach (var wardrobe in wardrobes)
            {
                if (!overrideTop)
                {
                    overrideTop = wardrobe.OverridesUpperUnderwear;
                }

                if (!overrideBot)
                {
                    overrideBot = wardrobe.OverridesLowerUnderwear;
                }
            }
        }
        
        public List<WardrobeFullInfo> SelectWardrobesForGender(OutfitFullInfo outfit, long genderFilter)
        {
             return outfit.Wardrobes.Where(x => outfit.GenderWardrobes.TryGetValue(genderFilter, out var wardrobe) && wardrobe.Contains(x.Id)).ToList();
        }

        public async Task UnCompressBundlesForFasterAccess(CharacterAndOutfit characterData)
        {
            var targetBundles = await GetUmaBundleForCharacter(characterData);
            foreach (var bundle in targetBundles)
            {
                await _uncompressedBundlesManager.DecompressBundle(bundle);
            }
        }
        
        public async Task<ICollection<UmaBundleFullInfo>> GetUmaBundleForCharacter(CharacterAndOutfit characterToOutfit)
        {
            var output = new List<UmaBundleFullInfo>();
            
            var character = characterToOutfit.Character;
            IEnumerable<long> wardrobeIds = null;
            if (characterToOutfit.Outfit != null)
            {
                var fullOutfit = characterToOutfit.Outfit;
                output.AddRange(fullOutfit.Wardrobes.Select(x=>x.UmaBundle));
            }
            else
            {
                wardrobeIds = character.Wardrobes?.Select(uw => uw.Id);
            }

            if (wardrobeIds == null) return output;

            foreach (var id in wardrobeIds)
            {
                var wardrobeFullInfo = await GetWardrobeFullInfo(id);
                if (output.Contains(wardrobeFullInfo.UmaBundle)) continue;
                    
                output.Add(wardrobeFullInfo.UmaBundle);
            }
            return output;
        }

        public async void ShowHighlightingWardrobe(AccessoryItem wardrobe, DynamicCharacterAvatar avatar)
        {
            RegisterCharacterModificationOperationStart();
            await PrepareUmaBundleForWardrobe(wardrobe.Wardrobe.Id);
            await SetSlot(avatar, wardrobe);
            avatar.BuildCharacter();
            avatar.ForceUpdate(false, true, true);
            await Task.Delay(HIGHLIGHT_DURATION);
            await ClearSlot(avatar, wardrobe.Slot, Array.Empty<WardrobeFullInfo>());
            UnloadSlotBundle(wardrobe.Slot);
            avatar.BuildCharacter();
            avatar.ForceUpdate(false, true, true);
            RegisterCharacterModificationOperationEnd();
        }
        
        public async Task ClearSlot(DynamicCharacterAvatar avatar, string slot, ICollection<WardrobeFullInfo> wardrobes)
        {
            RegisterCharacterModificationOperationStart();
            avatar.ClearSlot(slot);
            avatar.BuildCharacter();
            avatar.ForceUpdate(false, true, true);
            GetOverrideUnderwear(wardrobes, out var overrideTop, out var overrideBot);
            UpdateUnderwearStates(avatar, overrideTop, overrideBot);
            await WaitWhileAvatarCreated(avatar);
            RegisterCharacterModificationOperationEnd();
        }

        public float GetHeelsHeight(DynamicCharacterAvatar avatar)
        {
            var meshVertices = avatar.WardrobeRecipes.Values.Where(x=>x.wardrobeSlot == SHOES_SLOT_NAME)
                                     .Select(x=>x.GetCachedRecipe(avatar.context))
                                     .Where(x=>x != null && !x.slotDataList.IsNullOrEmpty())
                                     .SelectMany(x=>x.slotDataList)
                                     .Where(x=>x != null && x.asset != null)
                                     .Select(x=>x.asset)
                                     .Where(x=>x.meshData != null)
                                     .Select(x=>x.meshData)
                                     .SelectMany(x=>x.vertices);
                                      
            return meshVertices.Any()? -meshVertices.Min(x => x.y) : 0;
        }
        
        public void ReleaseAvatarResources(DynamicCharacterAvatar avatar, bool releaseSharedAssets)
        {
            avatar.RecipeUpdated.RemoveAllListeners();
            avatar.CharacterUpdated.RemoveAllListeners();
            avatar.CharacterDnaUpdated.RemoveAllListeners();
            avatar.ClearCachedData(releaseSharedAssets);
            if (avatar.umaData != null)
            {
                avatar.umaData.umaRecipe = null;
                avatar.umaData.generatedMaterials = new UMAData.GeneratedMaterials();
                avatar.umaData = null;
            }
        }

        public void LoadRace(string raceName)
        {
            UMAContext.Instance.raceLibrary.GetRace(raceName);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Init()
        {
            if (_initialized) return;
            
            _umaBundleHelper.InitializeIndexer();
            _selfieBuilder = new UmaRecipeBuilder(_clothesCabinet);
            var globalBundles = _umaBundleHelper.GetGlobalBundles();
            _umaBundles = globalBundles.ToDictionary(x => x.Id);
            AllGlobalBundles = globalBundles;
            _umaBundleHelper.AddBundlesToIndex(AllGlobalBundles);

            DynamicAssetLoader.Instance.remoteServerURL = $"file://{Application.persistentDataPath}/Cache/";
            AssetBundleManager.overrideBaseDownloadingURL += (bundleName) =>
            {
                var bundleModel = _umaBundles.Values.First(x => x.Name == bundleName);
                var uncompressedBundlePath = _uncompressedBundlesManager.GetUnpackedBundlePath(bundleModel);
                if (uncompressedBundlePath != null)
                {
                    return "file://" + uncompressedBundlePath;
                }
                var path = $"file://{Application.persistentDataPath}/Cache/{_bridge.Environment.ToString()}/UmaBundle/{bundleModel.Id}/{Utility.GetPlatformName()}/AssetBundle";
                path = _bridge.EncryptionEnabled ? $"{path}{_bridge.TargetExtension}" : path;
                return path;
            };

            _initialized = true;
        }

        private static void UnloadBundle(string bundleName)
        {
            AssetBundleManager.UnloadAssetBundle(bundleName);
        }

        private static void SetUnderwearState(OverlayData underwearOverlay, bool hide)
        {
            var newColor = new Color32(255, 255, 255, (byte)(!hide ? 255 : 0));
            underwearOverlay.SetColor(0, newColor);
            underwearOverlay.SetColor(1, newColor);
            underwearOverlay.SetColor(2, newColor);
        } 

        private async Task LoadBundleList(IEnumerable<UmaBundleFullInfo> bundleList, CancellationToken token = default)
        {
            if (bundleList == null) return;

            await DownloadBundles(bundleList, token);

            if (token.IsCancellationRequested) return;

            DynamicAssetLoader.Instance.LoadAssetBundles(bundleList.Select(x=>x.Name).ToArray());

            await WaitForBundlesLoaded(token);
        }

        private void SetOutfitColors(DynamicCharacterAvatar avatar, OutfitFullInfo outfit)
        {
            if (outfit.UmaSharedColors == null) return;
            foreach (var item in outfit.UmaSharedColors)
            {
                var sharedColor = _clothesCabinet.GetUmaSharedColor(item.UmaSharedColorId);
                if (sharedColor == null || sharedColor.Name == "Skin") continue;
                avatar.SetColor(sharedColor.Name, new Color32().ConvertFromIntColor(item.Color));
            }
            avatar.UpdateColors();
        }

        private async Task SetOutfitWardrobeList(DynamicCharacterAvatar avatar, IEnumerable<WardrobeFullInfo> wardrobes, CancellationToken token = default)
        {
            RegisterCharacterModificationOperationStart();
            avatar.ClearSlots();
            _activeBundlesInSlots.Clear();
            UnloadNotUsedByAvatarBundles(avatar);
            
            foreach (var wardrobe in wardrobes)
            {
                ProcessWardrobeFullInfo(wardrobe);
                var accessoryItem = _clothesCabinet.GetWardrobeItem(wardrobe.Id) as AccessoryItem;
                if (accessoryItem == null) continue;
                await SetSlot(avatar, accessoryItem, token);
            }
            RegisterCharacterModificationOperationEnd();
        }

        private async Task LoadWardrobesToRecipe(DynamicCharacterAvatar avatar, UMATextRecipe.DCSUniversalPackRecipe recipe, IEnumerable<IEntity> wardrobes, CancellationToken token = default)
        {
            var accessories = GetAccessoryItems(wardrobes);
            if (!accessories.Any())
            {
                recipe.wardrobeSet.Clear();
                return;
            }

            RegisterCharacterModificationOperationStart();
            
            var umaBundles = ExtractUmaBundles(accessories);
            await DownloadBundles(umaBundles, token);

            if (token.IsCancellationRequested) return;
            
            var assetNames = SelectAssetNames(accessories);
            DynamicAssetLoader.Instance.LoadAssetBundles(assetNames);

            await WaitForBundlesLoaded(token);

            if (token.IsCancellationRequested)
            {
                RegisterCharacterModificationOperationEnd();
                return;
            }
            
            avatar.context.dynamicCharacterSystem.Refresh(false);

            recipe.wardrobeSet = new List<WardrobeSettings>();

            foreach (var item in accessories)
            {
                if (!avatar.AvailableRecipes.TryGetValue(item.Slot, out var slotRecipe))
                {
                    Debug.LogWarning($"No recipe for {item.Slot} available");
                    continue;
                }
                var wardrobeRecipe = slotRecipe.Find(w => w.name == item.AssetName);
                if (wardrobeRecipe == null) 
                {
                    Debug.LogWarning($"Not found {item.Wardrobe.Name} in slot {item.Slot}");
                    continue;
                }
                DestroyRecipeThumbnail(wardrobeRecipe);
                
                recipe.wardrobeSet.Add(new WardrobeSettings(wardrobeRecipe.wardrobeSlot, wardrobeRecipe.name));
                var bundles = item.Wardrobe.UmaBundle.GetBundleWithDependencies();
                _activeBundlesInSlots.Add(new KeyValuePair<string, List<string>>(item.Slot, bundles));
            }
            RegisterCharacterModificationOperationEnd();
        }

        private string[] SelectAssetNames(List<AccessoryItem> accessoryList)
        {
            return accessoryList.Select(x => x.AssetName).Distinct().ToArray();
        }

        private UmaBundleFullInfo[] ExtractUmaBundles(List<AccessoryItem> accessoryList)
        {
            return accessoryList.Select(x => x.Wardrobe.UmaBundle).DistinctBy(x=>x.Id).ToArray();
        }

        private List<AccessoryItem> GetAccessoryItems(IEnumerable<IEntity> wardrobes)
        {            
            var accessoryList = new List<AccessoryItem>();
            if (wardrobes == null)
            {
                return accessoryList;
            }
            
            foreach (var entity in wardrobes)
            {
                if (!(entity is WardrobeFullInfo wardrobe)) continue;
                var wardrobeItem = _clothesCabinet.GetWardrobeItem(wardrobe.Id);
                if (wardrobeItem == null)
                {
                    var availableWardrobes = _clothesCabinet.WardrobeItems.Select(x => x.Wardrobe.Id).Distinct();
                    DebugHelper.LogSilentError(7849,
                        $"Wardrobe is null on attempt get wardrobe {wardrobe.Id} from {nameof(_clothesCabinet)}. Available wardrobes in {_clothesCabinet} are: {JsonConvert.SerializeObject(availableWardrobes)}");
                    continue;
                }

                if (!(wardrobeItem is AccessoryItem accessoryItem))
                {
                    DebugHelper.LogSilentError(7849,
                                               $"AccessoryItem is null on attempt of casting {nameof(wardrobeItem)} to {nameof(AccessoryItem)}. Wardrobe item type: {wardrobeItem.GetType().Name}");
                    continue;
                }
                accessoryList.Add(accessoryItem);
            }

            return accessoryList;
        }

        private void RestoreAvatarRace(DynamicCharacterAvatar avatar)
        {
            if (avatar.activeRace.racedata == null)
            {
                avatar.activeRace.data = avatar.context.raceLibrary.GetRace(avatar.activeRace.name);
            }
            if (avatar.umaData.umaRecipe.raceData == null)
            {
                avatar.umaData.umaRecipe.raceData = avatar.activeRace.data;
            }
        }

        private async Task DownloadBundles(IEnumerable<UmaBundleFullInfo> bundles, CancellationToken token = default)
        {
            if (bundles == null || !bundles.Any())
            {
                return;
            }

            var tasks = new Task[bundles.Count()];
            for(var i = 0; i < bundles.Count(); i++)
            {
                tasks[i] = DownloadBundle(bundles.ElementAt(i), token);
            }
            await Task.WhenAll(tasks);
        }

        private async Task DownloadBundle(UmaBundleFullInfo bundleModel, CancellationToken token = default)
        {
            await _bridge.FetchMainAssetAsync(bundleModel, token);
            if (token.IsCancellationRequested) return;
            
            await DownloadBundles(bundleModel.DependentUmaBundles, token);
        }

        private void RegisterCharacterModificationOperationStart()
        {
            AvatarModificationOperationCounter++;
        }

        private void RegisterCharacterModificationOperationEnd()
        {
            AvatarModificationOperationCounter--;
        }

        private async Task<WardrobeFullInfo> GetWardrobeFullInfo(long id, CancellationToken token = default)
        {
            if (_wardrobeFullInfoCache.ContainsKey(id))
            {
                return _wardrobeFullInfoCache[id];
            }
            
            var result = await _bridge.GetWardrobe(id, token);

            if (!result.IsSuccess)
            {
                Debug.LogWarning($"!!!!! Something wrong with {id}");
                return null;
            }

            _wardrobeFullInfoCache[id] = result.Model;
            return result.Model;
        }

        private void ProcessWardrobesFullInfo(IEnumerable<WardrobeFullInfo> wardrobes)
        {
            if (wardrobes == null) return;
            foreach (var wardrobe in wardrobes)
            {
                ProcessWardrobeFullInfo(wardrobe);
            }
        }

        private void ProcessWardrobeFullInfo(WardrobeFullInfo wardrobeFullInfo)
        {
            var umaBundle = wardrobeFullInfo.UmaBundle;
            ProcessUmaBundleFullInfo(umaBundle);
            _clothesCabinet.AddUmaBundleToWardrobe(wardrobeFullInfo, wardrobeFullInfo.Id);
        }

        private void ProcessUmaBundleFullInfo(UmaBundleFullInfo umaBundle)
        {
            _umaBundleHelper.AddBundleToIndex(umaBundle);

            if (_umaBundles.ContainsKey(umaBundle.Id))
            {
                _umaBundles[umaBundle.Id] = umaBundle;
            }
            else
            {
                _umaBundles.Add(umaBundle.Id, umaBundle);
            }

            foreach (var dependentBundle in umaBundle.DependentUmaBundles)
            {
                ProcessUmaBundleFullInfo(dependentBundle);
            }
        }

        private async Task<WardrobeFullInfo[]> PrepareWardrobesByName(long genderId, IEnumerable<string> wardrobeNames)
        {
            var result = await _bridge.GetWardrobeList(genderId, wardrobeNames.ToArray());
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return Array.Empty<WardrobeFullInfo>();
            }
            return result.Models;
        }

        private (List<WardrobeFullInfo> wardrobes, List<UmaBundleFullInfo> umaBundles) FilterWardrobeAndBundlesByGender(
            OutfitFullInfo outfit, long genderId)
        {
            var genderFilter = genderId;
            if (genderFilter == _characterManagerConfig.NonBinaryId)
                genderFilter = _characterManagerConfig.FemaleId;

            var charactersWardrobes = SelectWardrobesForGender(outfit, genderFilter);

            var umaBundles = charactersWardrobes
                            .Where(w => w.UmaBundle != null)
                            .Select(w => w.UmaBundle)
                            .ToList();
            
            umaBundles.AddRange(GenderGlobalBundles(genderId));
            
            return (charactersWardrobes, umaBundles);
        }
        
        private IEnumerable<UmaBundleFullInfo> GenderGlobalBundles(long genderId)
        {
            return AllGlobalBundles.Where(x => x.GenderIds.Contains(genderId));
        }

        private void AddBoneColliders(DynamicCharacterAvatar avatar, long genderId)
        {
            var race = _metadataProvider.MetadataStartPack.GetRaceByGenderId(genderId);
            var settings = _boneColliderSettings.First(x => x.RaceId == race.Id);
            foreach (var item in settings.BoneColliders)
            {
                var boneTransform = avatar.transform.FindChildInHierarchy(item.BoneName);
                if(boneTransform == null) continue;

                var boneCollider = boneTransform.gameObject.AddComponent<DynamicBoneCollider>();
                boneCollider.m_Center = item.PossitionOffset;
                boneCollider.m_Radius = item.Radius;
                boneCollider.m_Height = item.Height;
            }
        }
#if UNITY_EDITOR_WIN
        private void FixShaders(UMAData data)
        {
            var renderer = data.GetRenderer(0);
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                var mat = materials[i];
                var newShader = Shader.Find(mat.shader.name);
                mat.shader = newShader;
            }

            renderer.sharedMaterials = materials;
        }
#endif
        private static void DestroyRecipeThumbnail(UMATextRecipe recipe)
        {
            if (recipe.wardrobeRecipeThumbs == null) return;
            foreach (var thumb in recipe.wardrobeRecipeThumbs)
            {
                if (thumb.thumb == null) continue;
                Object.DestroyImmediate(thumb.thumb.texture, true);
                Object.DestroyImmediate(thumb.thumb, true);
            }

            recipe.wardrobeRecipeThumbs = null;
        }

        private void RemoveAllActiveSlotBundlesFromIndex()
        {
            foreach (var bundleName in _activeBundlesInSlots.SelectMany(x => x.Value))
            {
                _umaBundleHelper.RemoveBundleFromIndex(bundleName);
            }
        }

        private static bool AreTrunkWardrobesLoadedSuccessfully(DynamicCharacterAvatar avatar)
        {
            if (avatar.WardrobeRecipes == null) return false;
            return TRUNK_BODY_PART_SLOTS.Append(OUTFIT_SLOT_NAME).Any(x => avatar.WardrobeRecipes.ContainsKey(x));
        }
        
        private static bool ArePelvicWardrobesLoadedSuccessfully(DynamicCharacterAvatar avatar)
        {
            if (avatar.WardrobeRecipes == null) return false;
            return PELVIC_BODY_PART_SLOTS.Append(OUTFIT_SLOT_NAME).Any(x => avatar.WardrobeRecipes.ContainsKey(x));
        }
    }
}