using System;
using System.Linq;
using ARKit;
using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Playing.Core;
using Modules.FreverUMA.ViewManagement;
using SharedAssetBundleScripts.Runtime.Character;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Object = UnityEngine.Object;

namespace Modules.LevelManaging.Assets
{
    public interface ICharacterAsset : IAsset<CharacterFullInfo>, IAttachableAsset
    {
        SkinnedMeshRenderer SkinnedMeshRenderer { get; }
        Animator Animator { get; } 
        GameObject LookAtBoneGameObject { get; } 
        GameObject HeadBoneGameObject { get; } 
        GameObject RightHandBoneGameObject { get; } 
        GameObject LeftHandBoneGameObject { get; } 
        GameObject SpineBoneGameObject { get; } 
        GameObject MouthBoneGameObject { get; } 
        FaceAnimPlayer FaceAnimPlayer { get; }
        float PlaybackTime { get; }
        CharacterView View { get; }
        DynamicCharacterAvatar Avatar { get; }
        bool HasView { get; }
        bool HasAnyWardrobe { get; }
        long? OutfitId { get; }
        long GenderId { get; }
        bool IgnoreHeightHeels { get; set; }
        float Height { get; }
        float Width { get; }
        Vector3 LowestMiddleWorldPoint { get; }
        Transform HipTransform { get; }

        event Action Updated;

        void ChangeView(CharacterView view);
        void ChangeLightSource(LightGuidOwnersParent lightSourceParent, LightSettingsInfo[] lightSettings);
        void UpdateSpawnPositionLightsIntensity(float timeOfDay);
        void SetLayer(LayerMask layer);
        void SetScale(float scale);
        void StartMirroringTrackedUserFace(ARFace face);
        void StopMirroringTrackedUserFace();
        void ResetHairPosition();
        event Action RequestPlayerCenterFaceStarted;
        event Action RequestPlayerCenterFaceFinished;
        event Action RequestPlayerNeedsBetterLightingStarted;
        event Action RequestPlayerNeedsBetterLightingFinished; 
    }
    
    internal sealed class CharacterAsset : RepresentationAsset<CharacterFullInfo>, ICharacterAsset
    {
        private const float INTENSITY_MULTIPLIER = 2f;
        private const float ADJUSTMENT_OF_CHARACTER_PIVOT_POINT_X = 0.06f;//the character pivot is on the left foot instead of being in the middle

        private LightSettingsInfo[] _currentLightSettings;

        private GameObject _hipBone;
        private LightGuidOwnersParent _lightSourceParent;
        private FaceBlendShapeMap _faceBlendShapeMap;
        private BlendShapeVisualizer _arFaceVisualizer;
        private HairControl _hairControl;
        private bool _ignoreHeightHeels;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public override DbModelType AssetType => DbModelType.Character;

        public GameObject GameObject { get; private set; }
        public SkinnedMeshRenderer SkinnedMeshRenderer { get; private set; }
        public Animator Animator { get; private set; }
        
        public GameObject LookAtBoneGameObject { get; private set; }
        public GameObject HeadBoneGameObject { get; private set; }
        public GameObject RightHandBoneGameObject { get; private set; }
        public GameObject LeftHandBoneGameObject { get; private set; }
        public GameObject SpineBoneGameObject { get; private set; }
        public GameObject MouthBoneGameObject { get; private set; }
        public FaceAnimPlayer FaceAnimPlayer { get; private set; }

        public float PlaybackTime
        {
            get
            {
                var normalizedTime = Animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
                var stateLength = Animator.GetCurrentAnimatorStateInfo(0).length;
                return normalizedTime * stateLength;
            }
        }

        public CharacterView View { get; private set; }
        public DynamicCharacterAvatar Avatar => View.Avatar;
        public bool HasView => View != null;

        public bool HasAnyWardrobe => OutfitId == null && !RepresentedModel.Wardrobes.IsNullOrEmpty() || !View.Outfit.Wardrobes.IsNullOrEmpty();
        public long? OutfitId => View?.OutfitId;
        public long GenderId => RepresentedModel.GenderId;
        public bool IgnoreHeightHeels
        {
            get => _ignoreHeightHeels;
            set
            {
                if(_ignoreHeightHeels == value) return;
                _ignoreHeightHeels = value;
                UpdateCharacterHeight();
            }
        }

        public float Height => View.Height;
        public float Width => View.Width;

        public Vector3 LowestMiddleWorldPoint
        {
            get
            {
                var characterTransform = GameObject.transform;
                return characterTransform.position - characterTransform.right * ADJUSTMENT_OF_CHARACTER_PIVOT_POINT_X;
            }
        }

        public Transform HipTransform => _hipBone?.transform;
        public event Action Updated;

        private UMAData UMAData { get; set; }
        private Transform Transform => GameObject.transform;
        private GameObject MeshViewRoot => View.GameObject;
        private PhysicsSettings HairPhysics => GetHairModel()?.PhysicsSettings;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action RequestPlayerCenterFaceStarted;
        public event Action RequestPlayerCenterFaceFinished;
        public event Action RequestPlayerNeedsBetterLightingStarted;
        public event Action RequestPlayerNeedsBetterLightingFinished;
        public event Action<ISceneObject,Scene> MovedToScene;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(CharacterFullInfo represent, CharacterView view, FaceBlendShapeMap faceBlendShapeMap)
        {
            BasicInit(represent);
            _hairControl = new HairControl();
            GameObject = new GameObject($"Character {represent.Id}");
            GameObject.AddListenerToOnGameObjectMovedToAnotherScene(OnMovedToAnotherScene);
            _faceBlendShapeMap = faceBlendShapeMap;
            SetupView(view);
            SetupFaceAnimComponents();
            SetSkinnedMeshRenderer();
            UpdateCharacterHeight();
            SetupUmaData();   
            ResolveMeshForDependentComponents();
        }

        public void ChangeView(CharacterView view)
        {
            SetupView(view);
            SetSkinnedMeshRenderer();
            UpdateCharacterHeight();
            SetupUmaData();
            ResolveMeshForDependentComponents();
            Updated?.Invoke();
        }

        public void ChangeLightSource(LightGuidOwnersParent lightSourceParent, LightSettingsInfo[] lightSettings)
        {
            if (lightSourceParent == null) return;

            if (_lightSourceParent != null)
            {
                if (AreSettingsEqual(_currentLightSettings ,lightSettings)) return;
                Object.Destroy(_lightSourceParent.gameObject);
            }

            _currentLightSettings = lightSettings;

            if (lightSourceParent == null || GameObject == null) return;
            var lightParent = Object.Instantiate(lightSourceParent.gameObject, _hipBone.transform);
            SetupLightSource(lightParent);
        }

        private static bool AreSettingsEqual(LightSettingsInfo[] settings1, LightSettingsInfo[] settings2)
        {
            return settings1.Length == settings2.Length
                && settings1.All(s1 => settings2.All(s2 => s2.UnityGuid.Equals(s1.UnityGuid)));
        }

        public void UpdateSpawnPositionLightsIntensity(float timeOfDay)
        {
            const float dayTime = 0.5f;
            var difference = Mathf.Abs(dayTime - timeOfDay);
            var intensityFactor = dayTime + difference * INTENSITY_MULTIPLIER;

            if (_lightSourceParent == null) return;
            var lights = _lightSourceParent.GetGuidOwners();

            for (var i = 0; i < lights.Length; i++)
            {
                var lightSettingIntensity = _currentLightSettings[i].Intensity / 100f;
                lights[i].LightComponent.intensity = lightSettingIntensity * intensityFactor;
            }
        }

        public void SetLayer(LayerMask layer)
        {
            GameObject.layer = layer;
            UpdateChildLayers(Transform, layer);
            SetLightsLayerMask(layer);
        }

        public void SetScale(float scale)
        {
            var parent = GameObject.transform.parent;
            GameObject.SetParent(null);
            GameObject.transform.localScale = Vector3.one * scale;
            GameObject.SetParent(parent);
        }

        public void StartMirroringTrackedUserFace(ARFace face)
        {
            InitializeFaceVisualizer(face);
            SwitchFaceTrackingComponentsState(true);
        }

        public void StopMirroringTrackedUserFace()
        {
            SwitchFaceTrackingComponentsState(false);
        }

        public void ResetHairPosition()
        {
            _hairControl.ResetHairPosition();
        }

        public override void CleanUp()
        {
            _arFaceVisualizer.RequestPlayerCenterFaceStarted -= PlayerNeedsToCenterFaceStart;
            _arFaceVisualizer.RequestPlayerCenterFaceFinished -= OnPlayerNeedsToCenterFaceFinished;
            _arFaceVisualizer.RequestPlayerNeedsBetterLightingStarted -= PlayerNeedsBetterLightingStart;
            _arFaceVisualizer.RequestPlayerNeedsBetterLightingFinished -= OnPlayerNeedsBetterLightingFinished;
            base.CleanUp();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
            if (GameObject == null) return;
            GameObject.SetActive(value);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void PlayerNeedsToCenterFaceStart()
        {
            RequestPlayerCenterFaceStarted?.Invoke();
        }

        private void OnPlayerNeedsToCenterFaceFinished()
        {
            RequestPlayerCenterFaceFinished?.Invoke();
        }
        
        private void PlayerNeedsBetterLightingStart()
        {
            RequestPlayerNeedsBetterLightingStarted?.Invoke();
        }

        private void OnPlayerNeedsBetterLightingFinished()
        {
            RequestPlayerNeedsBetterLightingFinished?.Invoke();
        }
        
        
        private void ResolveMeshForDependentComponents()
        {
            FaceAnimPlayer.Init(_faceBlendShapeMap, SkinnedMeshRenderer, UMAData);
            _arFaceVisualizer.Init(SkinnedMeshRenderer);
        }
        
        private void SetupBoneGameObjects()
        {
            if (View.ViewType == ViewType.Baked)
            {
                var bakedViewRoot = View.GameObject.GetComponent<BakedViewRoot>();
                LookAtBoneGameObject = bakedViewRoot.Nose;
                _hipBone = bakedViewRoot.Hips;
                HeadBoneGameObject = bakedViewRoot.Head;
                RightHandBoneGameObject = bakedViewRoot.RightHandPinky1;
                LeftHandBoneGameObject = bakedViewRoot.LeftHandPinky1;
                SpineBoneGameObject = bakedViewRoot.Spine;
                MouthBoneGameObject = bakedViewRoot.Mouth;
                _hairControl.SetupHairPhysicsBones(SpineBoneGameObject, HairPhysics);
                return;
            }

            var avatar = View.Avatar;
            var boneHash = UMAUtils.StringToHash("NoseAdjust");
            LookAtBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);

            boneHash = UMAUtils.StringToHash("Hips");
            _hipBone = avatar.umaData.skeleton.GetBoneGameObject(boneHash);
            
            boneHash = UMAUtils.StringToHash("HeadAdjust");
            HeadBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);
            
            boneHash = UMAUtils.StringToHash("RightHandPinky1");
            RightHandBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);

            boneHash = UMAUtils.StringToHash("LeftHandPinky1");
            LeftHandBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);
            
            boneHash = UMAUtils.StringToHash("Spine");
            SpineBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);

            boneHash = UMAUtils.StringToHash("MouthAdjust");
            MouthBoneGameObject = avatar.umaData.skeleton.GetBoneGameObject(boneHash);
            
            _hairControl.SetupHairPhysicsBones(SpineBoneGameObject, HairPhysics);
        }
        
        private void UpdateChildLayers(Transform transform, int layer)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = layer;
                UpdateChildLayers(transform.GetChild(i), layer);
            }
        }
        
        private void OnCharacterUpdated(UMAData umaData)
        {
            SetUmaData(umaData);
            SetSkinnedMeshRenderer();
            UpdateCharacterHeight();
            FaceAnimPlayer.Init(_faceBlendShapeMap, SkinnedMeshRenderer, UMAData);
            _hairControl.SetupHairPhysicsBones(SpineBoneGameObject, HairPhysics);
            Updated?.Invoke();
        }
        
        private void AttachView()
        {
            var viewTransform = View.GameObject.transform;
            
            viewTransform.SetParent(Transform);
            viewTransform.localPosition = Vector3.zero;
            viewTransform.localRotation = Quaternion.identity;
            viewTransform.localScale = Vector3.one;
        }

        private void InitAnimator()
        {
            var previousAnimator = Animator;
            Animator = MeshViewRoot.GetComponent<Animator>();
            if (Animator == previousAnimator) return;
            Animator.applyRootMotion = true;
            Animator.keepAnimatorStateOnDisable = true;
            Animator.AddListenerToOnAnimatorMove(ApplyPositionChangesFromAnimator);
            
            if (previousAnimator == null) return;
            previousAnimator.ApplyStateTo(Animator);
            Animator.speed = previousAnimator.speed;
            previousAnimator.RemoveListenerFromOnAnimatorMove(ApplyPositionChangesFromAnimator);
        }
        
        private void ApplyPositionChangesFromAnimator()
        {
            var deltaPosition = Animator.deltaPosition;
            var deltaRotation = Animator.deltaRotation;
            Transform.position += deltaPosition;
            Transform.rotation = deltaRotation * Transform.rotation;
        }

        private void SetupLightSource(GameObject lightParent)
        {
            lightParent.transform.SetParent(_hipBone.transform);
            lightParent.transform.position = _hipBone.transform.position;
            lightParent.transform.rotation = Transform.rotation;
            var rotationControl = lightParent.AddOrGetComponent<FollowRotation>();
            rotationControl.SetTarget(Transform);
            lightParent.SetActive(true);
            _lightSourceParent = lightParent.GetComponent<LightGuidOwnersParent>();
            SetLightsLayerMask(GameObject.layer);
        }
        
        private void SetLightsLayerMask(LayerMask layerMask)
        {
            if (_lightSourceParent == null) return;
            foreach (var light in _lightSourceParent.GetGuidOwners())
            {
                light.LightComponent.cullingMask = 1 << layerMask;
            }
        }

        private void SetupFaceAnimComponents()
        {
            _arFaceVisualizer = GameObject.AddOrGetBlendShapeVisualizer();
            _arFaceVisualizer.RequestPlayerCenterFaceStarted += PlayerNeedsToCenterFaceStart;
            _arFaceVisualizer.RequestPlayerCenterFaceFinished += OnPlayerNeedsToCenterFaceFinished;
            _arFaceVisualizer.RequestPlayerNeedsBetterLightingStarted += PlayerNeedsBetterLightingStart;
            _arFaceVisualizer.RequestPlayerNeedsBetterLightingFinished += OnPlayerNeedsBetterLightingFinished;
            
            FaceAnimPlayer = GameObject.AddOrGetComponent<FaceAnimPlayer>();
            SwitchFaceTrackingComponentsState(false);
        }
        
        private void SwitchFaceTrackingComponentsState(bool activate)
        {
            _arFaceVisualizer.enabled = activate;
        }

        private void InitializeFaceVisualizer(ARFace arFace)
        {
            if (arFace == null) return;
            _arFaceVisualizer.SetArFace(arFace);
        }
        
        private void SetupView(CharacterView view)
        {
            if (view.CharacterId != Id)
                throw new InvalidOperationException($"Attempt to set view from character {view.CharacterId} to {Id}");
            
            if (View != null)
            {
                RemoveView();
            }

            View = view;
            View.SetActive(true);
            AttachView();
            InitAnimator();
            _hairControl.SetupHairPhysics(MeshViewRoot, Animator);
            SetLayer(GameObject.layer);

            SetupBoneGameObjects();
            var hasAttachedLight = _lightSourceParent != null;
            if (hasAttachedLight)
            {
                SetupLightSource(_lightSourceParent.gameObject);
            }

            SetupUmaData();
            View.Updated += UpdateCharacterHeight;
            View.Destroyed += OnViewDestroyed;
        }

        private void SetSkinnedMeshRenderer()
        {
            SkinnedMeshRenderer = GameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer.localBounds = Constants.Character.BOUNDS;
            SkinnedMeshRenderer.updateWhenOffscreen = true;
        }

        private void SetupUmaData()
        {
            var umaData = GameObject.GetComponentInChildren<UMAData>();
            SetUmaData(umaData);
        }

        private void SetUmaData(UMAData umaData)
        {
            if (UMAData != null)
            {
                UMAData.OnCharacterCompleted -= OnCharacterUpdated;
            }
           
            UMAData = umaData;
            if (umaData != null)
            {
                UMAData.OnCharacterCompleted += OnCharacterUpdated;
            }
        }
        
        /// <summary>
        /// In case when current view(object with uma DynamicCharacterAvatar component and SkinnedMeshRenderer) is destroyed
        /// character will still have root game object and other components(such as face animation player, Animator etc).
        /// We might need Character without any view during loading processes(if we unload old view to clear memory, but haven't loaded the new one yet)
        /// </summary>
        private void OnViewDestroyed(CharacterView destroyedView)
        {
            RemoveView();
        }
        
        private void RemoveView()
        {
            if (View==null) return;
            
            DetachView();
            UnlinkViewComponents();
            View.Destroyed -= OnViewDestroyed;
            View.Updated -= UpdateCharacterHeight;
            View = null;
        }

        private void DetachView()
        {
            if (!View.IsDestroyed)
            {
                View.Release();
            }

            DetachLightFromView();
        }

        private void DetachLightFromView()
        {
            if(_lightSourceParent==null) return;
            _lightSourceParent.transform.SetParent(Transform);
        }

        private void UnlinkViewComponents()
        {
            SkinnedMeshRenderer = null;
            UMAData = null;
            LookAtBoneGameObject = null;
            HeadBoneGameObject = null;
            _hipBone = null;
            RightHandBoneGameObject = null;
            LeftHandBoneGameObject = null;
            SpineBoneGameObject = null;
        }

        private void UpdateCharacterHeight()
        {
            MeshViewRoot.transform.SetLocalPositionY(0);
            if (IgnoreHeightHeels) return;
            MeshViewRoot.transform.SetLocalPositionY(View.HeelsHeight);
        }

        private void OnMovedToAnotherScene(Scene scene)
        {
            MovedToScene?.Invoke(this, scene);
        }

        private WardrobeFullInfo GetHairModel()
        {
            var appliedWardrobes = View.Outfit != null ? View.Outfit.Wardrobes : RepresentedModel.Wardrobes;
            return appliedWardrobes?.FirstOrDefault(x => x.IsHair());
        }
    }
}