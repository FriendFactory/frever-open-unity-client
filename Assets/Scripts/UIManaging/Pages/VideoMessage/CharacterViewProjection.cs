using Modules.LevelManaging.Assets;
using SharedAssetBundleScripts.Runtime.SetLocationScripts;
using UnityEngine;

namespace UIManaging.Pages.VideoMessage
{
    /// <summary>
    /// It's a rect transform which represents the character rectangle on the camera space canvas
    /// Useful for projection on screen space or detecting whether the touch is near character
    /// </summary>
    internal sealed class CharacterViewProjection: MonoBehaviour
    {
        private ISetLocationAsset _setLocationAsset;
        private ICharacterAsset _characterAsset;

        public RectTransform RectTransform { get; private set; }

        private PictureInPictureController PictureInPictureController => _setLocationAsset.PictureInPictureController;
        
        public void Init(ISetLocationAsset setLocationAsset, ICharacterAsset characterAsset)
        {
            _setLocationAsset = setLocationAsset;
            _characterAsset = characterAsset;
            RectTransform = GetComponent<RectTransform>();

            RectTransform.parent = PictureInPictureController.PictureRectTransform;
            RectTransform.localPosition = Vector3.zero;
            RectTransform.localRotation = Quaternion.identity;
            RectTransform.localScale = Vector3.one;
            RectTransform.pivot = new Vector2(0.5f, 0); //character position starts from bottom
            
            Refresh();
        }

        public void Refresh()
        {
            var characterLowestMiddlePoint = _characterAsset.LowestMiddleWorldPoint;
            var characterHighestPoint = characterLowestMiddlePoint + _characterAsset.Height * Vector3.up;
            var cam = _setLocationAsset.Camera;
            var viewportLowestPosition = cam.WorldToViewportPoint(characterLowestMiddlePoint);
            var viewportHighestPosition = cam.WorldToViewportPoint(characterHighestPoint);
            
            RectTransform.anchorMin = viewportLowestPosition;
            RectTransform.anchorMax = viewportLowestPosition;
            
            var characterScreenHeight = (viewportHighestPosition.y - viewportLowestPosition.y) * cam.pixelHeight * PictureInPictureController.PictureSize;
            var characterScreenWidth = characterScreenHeight / _characterAsset.Height * _characterAsset.Width;
            RectTransform.sizeDelta = new Vector2(characterScreenWidth, characterScreenHeight) / PictureInPictureController.PictureSize;
        }
    }
}