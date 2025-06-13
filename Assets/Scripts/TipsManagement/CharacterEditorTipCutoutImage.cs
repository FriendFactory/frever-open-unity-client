using Coffee.UIExtensions;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace TipsManagment
{
    [RequireComponent(typeof(Unmask))]
    [RequireComponent(typeof(Image))]
    internal sealed class CharacterEditorTipCutoutImage: MonoBehaviour
    {
        [TagField]
        [SerializeField] private string _targetTag;
        
        private void Start()
        {
            var sourceGameObject = GameObject.FindGameObjectWithTag(_targetTag);
            
            var sourceRect = sourceGameObject.GetComponent<RectTransform>();
            var targetRect = transform.GetComponent<RectTransform>();
            CopyRectTransformValues(targetRect, sourceRect);

            var sourceImage = sourceRect.GetComponent<Image>();
            var targetImage = targetRect.GetComponent<Image>();

            CopyImageComponentValues(targetImage, sourceImage);
        }

        private static void CopyImageComponentValues(Image destImage, Image sourceImage)
        {
            destImage.sprite = sourceImage.sprite;
            destImage.fillMethod = sourceImage.fillMethod;
            destImage.preserveAspect = sourceImage.preserveAspect;
            destImage.pixelsPerUnitMultiplier = sourceImage.pixelsPerUnitMultiplier;
        }

        private static void CopyRectTransformValues(RectTransform destRect, RectTransform sourceRect)
        {
            destRect.anchorMin = sourceRect.anchorMin;
            destRect.anchorMax = sourceRect.anchorMax;
            destRect.anchoredPosition = sourceRect.anchoredPosition;
            destRect.sizeDelta = sourceRect.sizeDelta;
            destRect.pivot = sourceRect.pivot;
        }
    }
}