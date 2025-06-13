using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class CaptionProjectionToScreenHelper : MonoBehaviour
    {
        [SerializeField] private RectTransform _viewPort;
        [SerializeField] private TMP_Text _captionScreenSpace;
        [Inject] private IAssetManager _assetManager;
        
        private RectTransform _captionCanvasCopy;//helping in calculation via using Unity API
        private readonly Vector2[] _localCornerPointsOnCameraSpace = new Vector2[4];

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public float ScreenSpaceFontSizeMultiplier { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task Init(Camera cam)
        {
            if (_captionCanvasCopy != null) return;
            
            await SpawnHelpingRectTransform(cam);
            CalculateCameraSpaceFontSize();
        }

        public Vector2 GetCameraSpaceLocalPositionNormalized()
        {
            var localPos = GetCameraSpaceLocalPosition();
            return Rect.PointToNormalized(_captionCanvasCopy.rect, localPos);
        }

        public Vector2 ConvertLocalNormalizedToScreenSpacePositionsNormalized(Vector2 localNormalized)
        {
            var localPoint = Rect.NormalizedToPoint(_viewPort.rect, localNormalized);
            var worldPoint = _viewPort.TransformPoint(localPoint);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_captionScreenSpace.canvas.GetComponent<RectTransform>(), screenPoint, null, out var localPointOnScreenSpace);
            return Rect.PointToNormalized(_captionScreenSpace.canvas.GetComponent<RectTransform>().rect, localPointOnScreenSpace);
        }

        public void Cleanup()
        {
            if (_captionCanvasCopy == null) return;
            Destroy(_captionCanvasCopy.gameObject);
            _captionCanvasCopy = null;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task SpawnHelpingRectTransform(Camera cam)
        {
            ICaptionAsset captionAsset = null;

            _assetManager.Load(new CaptionFullInfo
            {
                Id = long.MinValue, Text = string.Empty
            }, asset => { captionAsset = asset as ICaptionAsset; });

            while (captionAsset == null)
            {
                await Task.Delay(30);
            }

            captionAsset.SetCamera(cam);
            var captionCanvas = captionAsset.CaptionView.Canvas;
            _captionCanvasCopy = Instantiate(captionCanvas.gameObject, captionCanvas.transform.parent).GetComponent<RectTransform>();
            CleanupCanvasCopy();
            _assetManager.Unload(captionAsset);
        }

        private void CleanupCanvasCopy()
        {
            Destroy(_captionCanvasCopy.GetComponent<GraphicRaycaster>()); //prevent any interaction
            foreach (Transform child in _captionCanvasCopy)
            {
                Destroy(child.gameObject);
            }
        }

        private void CalculateCameraSpaceFontSize()
        {
            GetLocalCornersOnCameraSpace(_localCornerPointsOnCameraSpace);

            var widthOnCameraSpace = _localCornerPointsOnCameraSpace[3].x - _localCornerPointsOnCameraSpace[0].x;
            var screenSpaceRectTransform = _captionScreenSpace.GetComponent<RectTransform>();
            var widthOnScreenSpace = screenSpaceRectTransform.rect.width;
            ScreenSpaceFontSizeMultiplier = widthOnCameraSpace / widthOnScreenSpace;
        }

        private Vector2 GetCameraSpaceLocalPosition()
        {
            GetLocalCornersOnCameraSpace(_localCornerPointsOnCameraSpace);
            return _localCornerPointsOnCameraSpace.Aggregate(Vector2.zero, (current, corner) => current + corner)/4f;
        }
        
        private void GetLocalCornersOnCameraSpace(Vector2[] localCornerPointsOnCameraSpace)
        {
            var worldCorners = new Vector3[4];
            var screenSpaceRectTransform = _captionScreenSpace.GetComponent<RectTransform>();
            screenSpaceRectTransform.GetWorldCorners(worldCorners);
            
            for (var i = 0; i < worldCorners.Length; i++)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewPort, screenPoint, null,
                                                                        out var localPoint);
                var normalizedInCameraViewPoint = Rect.PointToNormalized(_viewPort.rect, localPoint);
                var worldPosOnCameraSpaceCanvas = _captionCanvasCopy.GetWorldPositionFromNormalized(normalizedInCameraViewPoint);
                localCornerPointsOnCameraSpace[i] = _captionCanvasCopy.InverseTransformPoint(worldPosOnCameraSpaceCanvas);
            }
        }
    }
}
