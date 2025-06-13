using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.AssetsManaging.LoadArgs;
using Modules.FaceAndVoice.Face.Core;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using UnityEngine.SceneManagement;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class CharacterLoader : FileAssetLoader<CharacterFullInfo, CharacterLoadArgs>
    {
        private readonly CharacterViewContainer _characterViewContainer;
        private readonly FaceBlendShapeMap _faceBlendShapeMap;
        private CharacterView _characterView;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CharacterLoader(IBridge bridge, CharacterViewContainer characterViewContainer, FaceBlendShapeMap faceBlendShapeMap) 
            : base(bridge)
        {
            _characterViewContainer = characterViewContainer;
            _faceBlendShapeMap = faceBlendShapeMap;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override Task LoadAsset(CharacterLoadArgs args)
        {
            return LoadCharacterView();
        }

        protected override void InitAsset()
        {
            var asset = new CharacterAsset();
            asset.Init(Model, _characterView, _faceBlendShapeMap);

            if (LoadArgs.Layer.HasValue)
            {
                asset.SetLayer(LoadArgs.Layer.Value);
            }

            MoveCharacterToMainScene(asset);
            Asset = asset;
            OnCompleted(GetSuccessResult());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void MoveCharacterToMainScene(ICharacterAsset asset)
        {
            var mainScene = SceneManager.GetSceneByBuildIndex(Constants.PERSISTENT_SCENE_INDEX);
            SceneManager.MoveGameObjectToScene(asset.GameObject, mainScene);
        }

        private async Task LoadCharacterView()
        {
            _characterViewContainer.SetOptimizeMemory(LoadArgs.OptimizeMemory);
            var view = await _characterViewContainer.GetView(Model, LoadArgs.Outfit, LoadArgs.CancellationToken);
            if (LoadArgs.CancellationToken.IsCancellationRequested)
            {
                view?.Release();
                OnCancelled();
                return;
            }
            _characterView = view;
            InitAsset();
        }
    }
}