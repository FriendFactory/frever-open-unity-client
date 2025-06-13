using System;
using System.Collections;
using Bridge.Models.ClientServer.Level.Full;
using Common;
using Extensions;
using Modules.LevelManaging.Assets.Caption;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable CS0659

namespace Modules.LevelManaging.Assets
{
    public interface ICaptionAsset: IAsset<CaptionFullInfo>, IAttachableAsset
    {
        bool IsViewActive { get; }
        CaptionView CaptionView { get; }
        void SetCamera(Camera camera);
        void ForceRefresh();
        void RefreshModel(CaptionFullInfo model);
    }
    
    internal sealed class CaptionAsset : RepresentationAsset<CaptionFullInfo>, ICaptionAsset
    {
        private const float PLANE_DISTANCE_OFFSET = 0.25f;

        private Canvas _canvas;
        private CaptionCanvasPlaneDistanceControl _canvasPlaneDistanceControl;
        private Coroutine _runningCoroutine;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override DbModelType AssetType => DbModelType.Caption;
        public GameObject GameObject { get; private set; }
        public bool IsViewActive => CaptionView.gameObject.activeInHierarchy;
        public CaptionView CaptionView { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        public event Action<ISceneObject, Scene> MovedToScene;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(CaptionFullInfo representation, CaptionView captionView)
        {
            BasicInit(representation);

            _canvas = captionView.GetComponentInParent<Canvas>();
            GameObject = _canvas.gameObject;

            CaptionView = captionView;
            CaptionView.SetTargetCaptionId(representation.Id);
            CaptionView.Text = representation.Text;
            CaptionView.SetFontSize(representation.FontSize.ToKilo());
            CaptionView.SetRotation(representation.RotationDegrees.ToKilo());
            CaptionView.SetAlignment(representation.TextAlignment);
            ApplyColor();
            CaptionView.ForceRefresh();
            RefreshPosition();

            GameObject.AddListenerToOnGameObjectMovedToAnotherScene(OnMovedToAnotherScene);
            _canvasPlaneDistanceControl = GameObject.GetComponent<CaptionCanvasPlaneDistanceControl>();
            CaptionAssetsRegister.Register(this);
            //workaround for FREV-15801
            _runningCoroutine = CoroutineSource.Instance.StartCoroutine(ForceRefreshDelay());
        }

        public void SetCamera(Camera camera)
        {
            _canvas.worldCamera = camera;
            if (camera != null)
            {
                _canvas.planeDistance = camera.nearClipPlane + PLANE_DISTANCE_OFFSET;
            }
            _canvasPlaneDistanceControl.SetCamera(camera);
            RefreshPosition();
        }

        public void ForceRefresh()
        {
            CaptionView.Text = RepresentedModel.Text;
            CaptionView.SetRotation(RepresentedModel.RotationDegrees.ToKilo());
            CaptionView.SetFontSize(RepresentedModel.FontSize.ToKilo());
            ApplyColor();
            CaptionView.ForceRefresh();
            RefreshPosition();
        }

        public void RefreshModel(CaptionFullInfo model)
        {
            if (RepresentedModel.Id != model.Id) throw new InvalidOperationException("Failed to refresh caption model. Ids must be equal");
            BasicInit(model);
        }

        public override bool Equals(object obj)
        {
            var asset = obj as IAsset;

            if (!AssetType.Equals(asset?.AssetType))
            {
                return false;
            }

            if (Id == 0 && asset?.Id == 0)
            {
                return GetHashCode() == asset.GetHashCode();
            }

            return Id.Equals(asset?.Id);
        }
        
        public override void PrepareForUnloading()
        {
            CaptionAssetsRegister.Unregister(this);
            if (_runningCoroutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }
            base.PrepareForUnloading();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void SetModelActive(bool value)
        {
            if (GameObject == null) return;
            GameObject.SetActive(value);
        }

        private void OnMovedToAnotherScene(Scene scene)
        {
            MovedToScene?.Invoke(this, scene);
        }
        
        private void RefreshPosition()
        {
            _canvas.ForceRebuild();//to make sure Canvas got aspect ratio from camera
            CaptionView.SetNormalizedPosition(RepresentedModel.GetNormalizedPosition());
        }

        private void ApplyColor()
        {
            var color = string.IsNullOrEmpty(RepresentedModel.TextColorRgb)
                ? Color.white
                : ColorExtension.HexToColor(RepresentedModel.TextColorRgb);
            CaptionView.SetColor(color);
        }
        
        private IEnumerator ForceRefreshDelay()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            ForceRefresh();
            _runningCoroutine = null;
        }
    }
}