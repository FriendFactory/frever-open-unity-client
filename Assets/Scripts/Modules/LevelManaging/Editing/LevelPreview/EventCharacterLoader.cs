using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.FreverUMA;
using UnityEngine;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    [UsedImplicitly]
    internal sealed class EventCharacterLoader : LoaderBase<CharacterFullInfo, CharacterLoadArgs>
    {
        private readonly AvatarHelper _avatarHelper;
        private readonly List<CharacterAndOutfit> _charactersRequiresAsyncLoading = new List<CharacterAndOutfit>();//which requires loading bundles into RAM
        private readonly List<CharacterAndOutfit> _charactersReadyToLoadInOneFrame = new List<CharacterAndOutfit>();//which have prebaked meshes in cache and can be loaded immediate
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override DbModelType Type => DbModelType.Character;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public EventCharacterLoader(IAssetManager assetManager, AvatarHelper avatarHelper):base(assetManager)
        {
            _avatarHelper = avatarHelper;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Prepare(ICollection<Event> events)
        {
            LoadedAssets.Clear();
            CollectCharactersToLoad(events);
            AssetsToLoadRemaining = _charactersRequiresAsyncLoading.Count + _charactersReadyToLoadInOneFrame.Count;
        }

        public override void Run()
        {
            #pragma warning disable 4014
            RunAsync();
            #pragma warning restore 4014
        }
        
        public override async Task RunAsync()
        {
            IsRunning = true;
            CheckIfHaveAssetsToLoad();
            CancellationToken = new CancellationTokenSource();
            
            LoadCharactersWhichCanBeLoadedImmediate();
            
            if (_charactersRequiresAsyncLoading.Count > 0)
            {
                await LoadCharactersFromBundles();
            }

            await WaitForAllAssetsLoaded(CancellationToken.Token);
        }

        private async Task LoadCharactersFromBundles()
        {
            var charactersToLoad = _charactersRequiresAsyncLoading.ToArray();//preventing exception if _charactersRequiresAsyncLoading is modified during foreach loop
            foreach (var loadingPiece in charactersToLoad)
            {
                await LoadCharacterFromBundles(loadingPiece, CancellationToken.Token);
                //await UnloadNonGlobalUmaBundles();
                if (CancellationToken.IsCancellationRequested) return;
            }
            //ReleaseAllUmaResources();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void LoadCharactersWhichCanBeLoadedImmediate()
        {
            if (_charactersReadyToLoadInOneFrame.Count <= 0) return;
            foreach (var characterAndOutfit in _charactersReadyToLoadInOneFrame)
            {
                var loadArgs = CreateArgs(characterAndOutfit.Outfit);
                loadArgs.CancellationToken = CancellationToken.Token;
                LoadAsset(characterAndOutfit.Character, loadArgs);
            }
        }

        private async Task LoadCharacterFromBundles(CharacterAndOutfit target, CancellationToken token = default)
        {
            // We need to save memory, so we will load the asset bundles when needed and unload them after,
            // instead of loading them ahead of time. Loading ahead of time is better time wise but not memory wise.
            if (!OptimizeMemory)
            {
                await _avatarHelper.PrepareBundlesForCharacterList(new []{ target }, token);
            }

            await LoadCharacterAsync(target);
        }

        private async Task LoadCharacterAsync(CharacterAndOutfit characterAndOutfit)
        {
            var loadArgs = CreateArgs(characterAndOutfit.Outfit);
            var character = characterAndOutfit.Character;
            await LoadAssetAsync(character, loadArgs);
        }

        private void CollectCharactersToLoad(ICollection<Event> events)
        {
            _charactersRequiresAsyncLoading.Clear();
            _charactersReadyToLoadInOneFrame.Clear();

            var characterControllers = events.SelectMany(x => x.CharacterController);

            foreach (var controller in characterControllers)
            {
                var data = controller.GetCharacterAndOutfitData();
                if(_charactersRequiresAsyncLoading.Any(x=>x.Equals(data))) continue;
                if(AssetManager.IsCharacterLoaded(data.Character.Id, data.Outfit?.Id)) continue;

                if (CanLoadImmediate(data))
                {
                    _charactersReadyToLoadInOneFrame.Add(data);
                }
                else
                {
                    _charactersRequiresAsyncLoading.Add(data);
                }
            }
        }

        private bool CanLoadImmediate(CharacterAndOutfit data)
        {
            return AssetManager.IsCharacterMeshReady(data.Character.Id, data.Outfit?.Id);
        }

        private CharacterLoadArgs CreateArgs(OutfitFullInfo outfit)
        {
            return new CharacterLoadArgs
            {
                OptimizeMemory = OptimizeMemory,
                DeactivateOnLoad = true,
                Outfit = outfit
            };
        }
        
        private void ReleaseAllUmaResources()
        {
            _avatarHelper.UnloadAllUmaBundles();
        }

        private async Task UnloadNonGlobalUmaBundles()
        {
            _avatarHelper.UnloadNonGlobalAssetBundles();
            await Resources.UnloadUnusedAssets();
        }
    }
}
