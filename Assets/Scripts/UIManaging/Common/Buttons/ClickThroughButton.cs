using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons
{
    public class ClickThroughButton: Button
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            RaycastBelow<IPointerClickHandler>(eventData)?.OnPointerClick(eventData);
            
            base.OnPointerClick(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            
            RaycastBelow<IPointerDownHandler>(eventData)?.OnPointerDown(eventData);
            
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            
            RaycastBelow<IPointerUpHandler>(eventData)?.OnPointerUp(eventData);
            
            base.OnPointerDown(eventData);
        }

        private T RaycastBelow<T>(PointerEventData eventData) where T: class
        {
            var results = new List<RaycastResult>();
            var newEventData = new PointerEventData(EventSystem.current)
            {
                position = eventData.position
            };
            EventSystem.current.RaycastAll(newEventData, results);

            var targetHitObj = results.Count(result => !result.gameObject.transform.IsChildOf(transform)) > 0
                ? results.Where(result => !result.gameObject.transform.IsChildOf(transform))
                         .Aggregate((result1, result2) => result1.sortingOrder == result2.sortingOrder ?  
                                        result1.depth > result2.depth ? result1 : result2 
                                        : result1.sortingOrder > result2.sortingOrder ? result1 : result2)
                         .gameObject
                : null;

            return targetHitObj == null ? null : targetHitObj.GetComponentInParent<T>();
        }
    }
}