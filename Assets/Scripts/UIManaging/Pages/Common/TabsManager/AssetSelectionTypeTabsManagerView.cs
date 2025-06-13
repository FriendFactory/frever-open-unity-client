using Common;
using DG.Tweening;
using UnityEngine;

namespace UIManaging.Pages.Common.TabsManager
{
    public class AssetSelectionTypeTabsManagerView : TabsManagerView
    {
        [SerializeField] private float _moveSpeed = 2500f;
        [SerializeField] private RectTransform _marker;

        private Tween _selectionRectMoveTween;

        protected override void OnToggleSetOn(int index, bool setByUser)
        {
            base.OnToggleSetOn(index, setByUser);
            RefreshMarkerPosition();
        }

        private void OnEnable()
        {
            if (TabViews != null)
            {
                RefreshMarkerPosition(false);
            }
        }
        
        private void RefreshMarkerPosition(bool overtime = true)
        {
            _selectionRectMoveTween?.Kill();
            _marker.gameObject.SetActive(false);
            CoroutineSource.Instance.ExecuteWithFramesDelay(2, Callback);

            void Callback()
            {
                if(overtime)
                {
                    var targetPosition = GetMarkerPositionForTabAtIndex(TabsManagerArgs.SelectedTabIndex);
                    _selectionRectMoveTween = _marker.DOAnchorPos(targetPosition, _moveSpeed).SetSpeedBased().SetUpdate(true).SetEase(Ease.Linear);
                }
                else
                {
                    _marker.anchoredPosition = GetMarkerPositionForTabAtIndex(TabsManagerArgs.SelectedTabIndex);
                }
                
                _marker.gameObject.SetActive(true);
            }
        }

        private Vector2 GetMarkerPositionForTabAtIndex(int index)
        {
            var targetXPosition = TabViews[index].RectTransform.anchoredPosition.x;
            return new Vector2(targetXPosition, _marker.anchoredPosition.y);
        }

        private void OnDisable()
        {
            _selectionRectMoveTween?.Kill();
        }
    }
}