using DG.Tweening;
using UnityEngine;

namespace Modules.VideoStreaming.Feed
{
    public class FeedRefreshIndicator : MonoBehaviour
    {
        [SerializeField] private float resetPositionSpeed = 5000f;
        [SerializeField] private float arrowTopPosition = 30f;
        [SerializeField] private float arrowBottomPosition = -30f;
        [SerializeField] private RectTransform arrowRectTransform;
        [SerializeField] private GameObject refreshTextGameObject;

        public RectTransform RectTransform { get; private set; }
        private Vector2 _initialPosition;
        
        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            _initialPosition = RectTransform.anchoredPosition;
        }
        
        public void ResetToInitialPosition()
        {
            RectTransform.DOAnchorPos(_initialPosition, resetPositionSpeed).SetSpeedBased().SetEase(Ease.Linear);
            arrowRectTransform.DOAnchorPos(new Vector2(arrowRectTransform.anchoredPosition.x, arrowTopPosition), resetPositionSpeed).SetSpeedBased().SetEase(Ease.Linear);
            SetRefreshTextActive(false);
        }

        public void SetNormalizedArrowPosition(float normalizedPosition)
        {
            var yPosition = Mathf.Lerp(arrowTopPosition, arrowBottomPosition, normalizedPosition);
            arrowRectTransform.anchoredPosition = new Vector2(arrowRectTransform.anchoredPosition.x, yPosition);
            refreshTextGameObject.SetActive(normalizedPosition >= 1f);
        }
        
        private void SetRefreshTextActive(bool value)
        {
            refreshTextGameObject.SetActive(value);
        }
    }
}