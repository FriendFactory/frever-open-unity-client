using Abstract;
using Bridge;
using Extensions;
using Modules.AssetsManaging;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.PhotoBooth.Character;
using UIManaging.Pages.UmaEditorPage.Ui;
using UMA;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Zenject;

namespace UIManaging.Pages.AvatarPreview.Ui {

    public class AvatarDisplayView : BaseContextDataView<IAvatarDisplayModel> 
    {
        private static readonly int WAVE_TRIGGER = Animator.StringToHash("Wave");
        
        [SerializeField] private RectTransform _characterViewRect;
        [SerializeField] private GameObject _loadingScreen;
        [SerializeField] private ParticleSystem _confetiParticles;
        
        [Inject] private UMACharacterEditorRoom _editorRoom;
        [Inject] private IAssetManager _assetManager;
        [Inject] private IMetadataProvider _metadataProvider;
        
        private AnimationClip _idleClip;
        private AnimationClip _waveClip;
        private AnimatorOverrideController _animatorOverrideController;
        private AnimationClipOverrides _animationClipOverrides;
        private UniversalAdditionalCameraData _additionalCameraData;
        private bool _renderPostProcessing;

        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            ContextData.CloseRoomRequested += CloseEditorRoom;
            
            _loadingScreen.SetActive(true);
            
            _animatorOverrideController = new AnimatorOverrideController(_editorRoom.roomCharacterController);
            _animationClipOverrides = new AnimationClipOverrides(_animatorOverrideController.overridesCount);

            _additionalCameraData = _editorRoom.roomCamera.GetComponent<UniversalAdditionalCameraData>();
            _renderPostProcessing = _additionalCameraData.renderPostProcessing;
            _additionalCameraData.renderPostProcessing = false;

            PrepareCharacter();
            GetAnimations();
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.CloseRoomRequested -= CloseEditorRoom;
            }
            _additionalCameraData.renderPostProcessing = _renderPostProcessing;
            base.BeforeCleanup();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void PrepareCharacter()
        {
            _editorRoom.BodyDisplayMode = BodyDisplayMode.FullBody;
            _editorRoom.SetRawImageRect(_characterViewRect);
            _editorRoom.Show(await ContextData.GetAvatar(), _metadataProvider.MetadataStartPack.GetRaceByGenderId(ContextData.GenderId));
            
            var animator = ContextData.Avatar.GetComponent<Animator>();
                                          
            animator.runtimeAnimatorController = _animatorOverrideController;
            animator.SetTrigger(WAVE_TRIGGER);

            _loadingScreen.SetActive(false);
            _confetiParticles.Play();
        }
        
        private void CloseEditorRoom()
        {
            _editorRoom.Hide();
            _editorRoom.DespawnAvatar();
        }
        
        private async void GetAnimations()
        {
            var idleAnimation = await ContextData.GetIdleBodyAnimation(); // id 171
            _assetManager.Load(idleAnimation, OnIdleAnimationAssetLoaded);

            var waveAnimation = await ContextData.GetWaveBodyAnimation(); // id 173
            _assetManager.Load(waveAnimation, OnWaveAnimationAssetLoaded);
        }
        
        private void OnIdleAnimationAssetLoaded(IAsset asset)
        {
            var bodyAnimationAsset = (IBodyAnimationAsset)asset;
            _idleClip = bodyAnimationAsset.BodyAnimation;
            OverrideAnimation(_idleClip, "Idle");
        }

        private void OnWaveAnimationAssetLoaded(IAsset asset)
        {
            var bodyAnimationAsset = (IBodyAnimationAsset)asset;
            _waveClip = bodyAnimationAsset.BodyAnimation;
            OverrideAnimation(_waveClip, "body_Std_Wave_v2");
        }

        private void OverrideAnimation(AnimationClip clip, string animationKey)
        {
            _animatorOverrideController.GetOverrides(_animationClipOverrides);

            _animationClipOverrides[animationKey] = clip;
            _animatorOverrideController.ApplyOverrides(_animationClipOverrides);
        }
    }
}