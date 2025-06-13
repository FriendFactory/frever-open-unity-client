using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using SharedAssetBundleScripts.Runtime.Character;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using Object = UnityEngine.Object;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Modules.FreverUMA.ViewManagement
{
   /// <summary>
   /// Store prebaked meshes for different character + outfit combos
   /// </summary>
   public sealed class CharacterViewContainer
   {
      private static GameObject VIEWS_HOLDER
      {
         get
         {
            if (_holder != null) return _holder;
            
            _holder = new GameObject("CharacterViewHolder");
            Object.DontDestroyOnLoad(_holder);
            _holder.SetActive(false);
            return _holder;
         }
      }

      private const string ANIMATOR_CONTROLLER_PATH = "Animator/CharacterController";

      private static GameObject _holder;
      
      private readonly AvatarHelper _avatarHelper;
      private readonly ICharacterEditor _umaCharacterEditor;
      private readonly IBridge _bridge;
      private readonly List<CharacterView> _loadedViews = new(); 
      
      private int _charactersLoadingCount;
      private bool _optimizeMemory;

      public CharacterViewContainer(AvatarHelper avatarHelper, IBridge bridge, ICharacterEditor umaCharacterEditor)
      {
         _avatarHelper = avatarHelper;
         _bridge = bridge;
         _umaCharacterEditor = umaCharacterEditor;
      }

      public IReadOnlyCollection<CharacterView> GetLoadedViewsList()
      {
         return _loadedViews;
      }
      
      public bool HasPreparedView(long characterId, long? outfitId)
      {
         return _loadedViews.Any(x => x.CharacterId == characterId && outfitId.Compare(x.OutfitId));
      }
      
      public async Task<CharacterView> GetView(CharacterFullInfo character, OutfitFullInfo outfit = null, CancellationToken token = default)
      {
         var cachedView = GetViewFromCache(character.Id, outfit?.Id);

         if (cachedView != null)
         {
            return cachedView;
         }

         var spawnResult = await SpawnView(character, outfit, token);
         spawnResult.GameObject.name = $"CharacterView {character.Id}/{outfit?.Id}";
         var view = new CharacterView(character.Id, outfit, spawnResult);
         _loadedViews.Add(view);
         view.Released += TakeView;
         view.Destroyed += OnViewDestroyed;

         return view;
      }

      public void UnloadAllCharacterViews(long characterId)
      {
          var targets = _loadedViews.Where(x => x.CharacterId == characterId).ToArray();
          Unload(targets);
      }

      public void Unload(params CharacterView[] views)
      {
          foreach (var view in views)
          {
              if (view.ViewType == ViewType.GeneratedRuntime)
              {
                  _avatarHelper.ReleaseAvatarResources(view.Avatar, false);
              }
              view.Destroy();
          }
          Resources.UnloadUnusedAssets();
      }

      public void SetOptimizeMemory(bool enable)
      {
          if(enable == _optimizeMemory) return;
          
         if (_charactersLoadingCount > 0)
         {
            Debug.LogWarning("There are characters loading while trying to change memory optimization setting");
         }
         _optimizeMemory = enable;
      }

      private void TakeView(CharacterView view)
      {
         view.GameObject.SetParent(VIEWS_HOLDER.transform);
      }
      
      private CharacterView GetViewFromCache(long characterId, long? outfitId)
      {
         return _loadedViews.FirstOrDefault(x => x.CharacterId == characterId && x.OutfitId.Compare(outfitId));
      }
      
      private async Task<SpawnedViewData> SpawnView(CharacterFullInfo character, OutfitFullInfo outfit, CancellationToken token = default)
      {
          var data = SelectSupportedViewModel(character, outfit?.Id);
          if (data.bakedView != null)
          {
              var spawnedBakedView = await SpawnBakedView(data.bakedView, data.fileInfo, token);
              if (spawnedBakedView.GameObject != null)
              {
                  return spawnedBakedView;
              }
          }

          return await SpawnViewBasedOnUmaData(character, outfit, token);
      }

      private static (BakedView bakedView, FileInfo fileInfo) SelectSupportedViewModel(CharacterFullInfo characterModel, long? outfitId)
      {
          if (characterModel.BakedViews.IsNullOrEmpty()) return (null, null);
          var matchOutfits = characterModel.BakedViews.Where(x => x.OutfitId == outfitId);
          var bakedViewModel = matchOutfits.FirstOrDefault(x => x.GetCompatibleBundle() != null);
          return bakedViewModel == null 
              ? (null, null) 
              : (bakedViewModel, bakedViewModel.GetCompatibleBundle());
      }
      
      private async Task<SpawnedViewData> SpawnBakedView(BakedView bakedView, FileInfo fileInfo, CancellationToken token)
      {
          var assetResult = await _bridge.GetCharacterBakedView(bakedView, fileInfo, cancellationToken: token);
          if (!assetResult.IsSuccess)
          {
              return default;
          }

          var assetBundle = assetResult.Object as AssetBundle;
          if (assetBundle == null) return default;

          var filePath = assetBundle.GetAllAssetNames().First();
          var assetName = Path.GetFileName(filePath);
          //todo: use CharacterView <ID> on prod to better safety; for now I just load whatever the AB have, because I want to test Prod characters on Stage
          var viewPrefab = assetBundle.LoadAsset<GameObject>(assetName);
          var view = Object.Instantiate(viewPrefab);
          var hair = view.GetComponent<BakedViewRoot>().HairRoot;
          if (hair != null)
          {
              var dynamicBone = hair.AddComponent<DynamicBone>();
              dynamicBone.m_Root = hair.transform;
          }
          var characterSize = view.GetComponentInChildren<SkinnedMeshRenderer>().bounds.size;
          var animatorTemplate =  Resources.Load<RuntimeAnimatorController>(ANIMATOR_CONTROLLER_PATH);
          var animatorController = Object.Instantiate(animatorTemplate);
          var animator = view.GetComponent<Animator>();
          animator.runtimeAnimatorController = animatorController;
          animator.Update(1f);
          assetBundle.Unload(false);
          return new SpawnedViewData
          {
              GameObject = view,
              HeelsHeight = bakedView.HeelsHeight,
              CharacterHeight = characterSize.y,
              CharacterWidth = characterSize.x,
              ViewType = ViewType.Baked
          };
      }
      
      private async Task<SpawnedViewData> SpawnViewBasedOnUmaData(CharacterFullInfo character, OutfitFullInfo outfit, CancellationToken token)
      {
          Application.backgroundLoadingPriority = ThreadPriority.High; 

          UMAGenerator.compressRenderTexture = _optimizeMemory;
          
          var avatar = await _umaCharacterEditor.CreateNewAvatar(character.GenderId, token);
          _umaCharacterEditor.SetTargetAvatar(avatar);
        
          // We need to do this before locking multiple characters, so the asset bundles are only loaded once
          await RequestStartLoadingCharacter(token);
          
          await _umaCharacterEditor.LoadCharacter(character, outfit, token);
          var heelsHeight = _umaCharacterEditor.GetHeelsHeight(avatar);
          if (_optimizeMemory)
          {
              _umaCharacterEditor.UnloadNotUsedUmaBundles(avatar);
              //not sure why we need it, see the commit comment for details. Commit: 9472abbfc30d1515506a633da09cb5411e1a9aa9
              _avatarHelper.LoadRace(avatar.activeRace.name);
          }
          NotifyEndLoadingCharacter();

          //avatar.BuildCharacterEnabled = false;
          var characterSize = avatar.GetComponentInChildren<SkinnedMeshRenderer>().bounds.size;

          return new SpawnedViewData
          {
              Avatar = avatar,
              GameObject = avatar.gameObject,
              HeelsHeight = heelsHeight,
              CharacterHeight = characterSize.y,
              CharacterWidth = characterSize.x,
              ViewType = ViewType.GeneratedRuntime
          };
      }

      private async Task RequestStartLoadingCharacter(CancellationToken token = default)
      {
         if (_optimizeMemory)
         {
            while (_charactersLoadingCount >= 1)
            {
               await Task.Delay(25, token);
            }
         }
         _charactersLoadingCount++;
      }

      private void NotifyEndLoadingCharacter()
      {
          --_charactersLoadingCount;
      }

      private void OnViewDestroyed(CharacterView view)
      {
         _loadedViews.Remove(view);
      }
   }

   public struct SpawnedViewData
   {
       public DynamicCharacterAvatar Avatar;
       public GameObject GameObject;
       public float HeelsHeight;
       public float CharacterHeight;
       public float CharacterWidth;
       public ViewType ViewType;
   }

   public enum ViewType
   {
       GeneratedRuntime,
       Baked
   }
}