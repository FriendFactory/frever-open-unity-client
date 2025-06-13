using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

namespace UIManaging.Common
{
    public class CarouselView : ScrollRect
    {
        public float Offset;
        [NonSerialized]
        public float ScrollTime = 0.5f;

        public event Action<Transform> ObjectSelected;

        private Transform _targetTransform;
        private Coroutine moveCoroutine;

        private bool _draging = false;
        private Vector2 _dragStartPosition;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            _draging = true;
            _dragStartPosition = eventData.position;
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _draging = false;
            ScrollToElemetPosition(CalculateNearestTransform(eventData.position - _dragStartPosition));
        }

        public void ScrollToElemetPosition(Transform target, bool instantly = false)
        {
            moveCoroutine = StartCoroutine(MoveToTransform((RectTransform)target, instantly));
            ObjectSelected?.Invoke(target);
        }

        private IEnumerator MoveToTransform(RectTransform target, bool instantly = false)
        {
            var targetPosition = target.anchoredPosition;
            var lerpStartPosition = content.anchoredPosition;
            var correctedX = horizontal ? (targetPosition.x - Offset - target.rect.width / 2) : 0;
            var correctedY = vertical ? (targetPosition.y - Offset - target.rect.height / 2) : 0;
            var correctedTargetPosition = new Vector2(correctedX, correctedY);

            var t = instantly ? 1f : 0f;
            while (t < 1)
            {
                if (_draging) yield break;
                t += Time.deltaTime / ScrollTime;
                var lerped = Vector2.Lerp(lerpStartPosition, -correctedTargetPosition, t);
                content.anchoredPosition = lerped;
                yield return null;
            }

            content.anchoredPosition = -correctedTargetPosition;
        }

        private Transform CalculateNearestTransform(Vector2 delta)
        {
            Transform nearestElement = null;
            var childsCount = content.childCount;
            var multiplyedNormal = delta.normalized * 150;
            var currentPosition = content.anchoredPosition + multiplyedNormal;

            float nearestDistance = 0;

            for (int i = 0; i < childsCount; i++)
            {
                var child = content.GetChild(i) as RectTransform;
                var elementX = horizontal ? child.anchoredPosition.x - Offset - child.rect.width / 2 : 0;
                var elementY = vertical ? child.anchoredPosition.y - Offset - child.rect.height / 2 : 0;
                var elementPosition = -new Vector2(elementX, elementY);

                if (i > 0 && Vector2.Distance(elementPosition, currentPosition) > nearestDistance)
                {
                    continue;
                }

                nearestElement = child;
                nearestDistance = Vector2.Distance(elementPosition, currentPosition);
            }

            return nearestElement;
        }
    }
}
