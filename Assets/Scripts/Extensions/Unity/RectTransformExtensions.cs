using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions
{
    public static class RectTransformExtensions
    {
        private static readonly Vector3[] _worldCorners = new Vector3[4];

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public static void FillParent(this RectTransform rectTransform)
        {
            rectTransform.pivot = Vector2.one * 0.5f;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector3.zero;
            rectTransform.transform.localScale = Vector3.one;
        }

        public static void SetSizeToFitParent(this RectTransform rectTransform)
        {
            rectTransform.SetSize(rectTransform.parent.GetComponent<RectTransform>().GetSize());
        }

        public static void AdjustSizeDeltaToParentSize(this RectTransform rectTransform)
        {
            rectTransform.SetSizeDelta(rectTransform.parent.GetComponent<RectTransform>().GetSize());
        }

        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 vec)
        {
            trans.pivot = vec;
            trans.anchorMin = vec;
            trans.anchorMax = vec;
        }

        public static void SetPivotAndAnchors(this RectTransform trans, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax)
        {
            trans.pivot = pivot;
            trans.anchorMin = anchorMin;
            trans.anchorMax = anchorMax;
        }

        public static void ChangePivotWithKeepingPosition(this RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;
            deltaPosition.Scale(rectTransform.rect.size);
            deltaPosition.Scale(rectTransform.localScale);
            deltaPosition = rectTransform.rotation * deltaPosition;
    
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition; // reverse the position change
        }
        
        public static float GetAspectRatio(this RectTransform trans)
        {
            if (trans == null) return 0f;

            var rect = trans.rect;
            return rect.width / rect.height;
        }

        //---------------------------------------------------------------------
        // Width and Height
        //---------------------------------------------------------------------

        public static float GetWidth(this RectTransform trans)
        {
            return trans.rect.width;
        }

        public static void SetWidth(this RectTransform trans, float width)
        {
            var sizeDelta = trans.sizeDelta;
            sizeDelta.x = width;
            trans.sizeDelta = sizeDelta;
        }

        public static float GetHeight(this RectTransform trans)
        {
            return trans.rect.height;
        }

        public static void SetHeight(this RectTransform trans, float height)
        {
            var sizeDelta = trans.sizeDelta;
            sizeDelta.y = height;
            trans.sizeDelta = sizeDelta;
        }

        public static Vector2 GetSize(this RectTransform trans)
        {
            return trans.rect.size;
        }

        //---------------------------------------------------------------------
        // Size
        //---------------------------------------------------------------------

        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            if (trans == null) return;
            
            var oldSize = trans.rect.size;
            var deltaSize = newSize - oldSize;
            var pivot = trans.pivot;
            trans.offsetMin -= new Vector2(deltaSize.x * pivot.x, deltaSize.y * pivot.y);
            trans.offsetMax += new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - pivot.y));
        }

        public static void SetSizeX(this RectTransform trans, float sizeX)
        {
            if (trans == null) return;

            var size = trans.GetSize();
            size.x = sizeX;
            trans.SetSize(size);
        }

        public static void SetSizeY(this RectTransform trans, float sizeY)
        {
            if (trans == null) return;

            var size = trans.GetSize();
            size.y = sizeY;
            trans.SetSize(size);
        }

        private static void SetSizeDelta(this RectTransform trans, Vector2 newSize)
        {
            trans.sizeDelta = newSize;
        }

        //---------------------------------------------------------------------
        // Anchored Position
        //---------------------------------------------------------------------

        public static void SetAnchoredX(this RectTransform trans, float x)
        {
            trans.anchoredPosition = new Vector2(x, trans.anchoredPosition.y);
        }

        public static void SetAnchoredY(this RectTransform trans, float y)
        {
            trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, y);
        }
        
        //---------------------------------------------------------------------
        // Offsets
        //---------------------------------------------------------------------

        public static float GetLeft(this RectTransform rt)
        {
            return rt.offsetMin.x;
        }
        
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static float GetRight(this RectTransform rt)
        {
            return -rt.offsetMax.x;
        }
 
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static float GetTop(this RectTransform rt)
        {
            return -rt.offsetMax.y;
        }
 
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static float GetBottom(this RectTransform rt)
        {
            return rt.offsetMin.y;
        }
 
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
        
        public static Vector3 GetWorldPositionFromNormalized(this RectTransform rt, Vector2 normalizedPos)
        {
            var localPosition = new Vector2(rt.rect.width * (normalizedPos.x - rt.pivot.x), rt.rect.height * (normalizedPos.y - rt.pivot.y));
            return rt.TransformPoint(localPosition );
        }
        
        public static Vector2 GetNormalizedLocalPosition(this RectTransform target)
        {
            if (target == null || target.parent == null)
            {
                Debug.LogError("Invalid parameters. Make sure both transforms are not null, and the second transform is a child of the first.");
                return Vector3.zero;
            }
        
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
            var parentRect = target.parent.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out var localPoint);
            return Rect.PointToNormalized(parentRect.rect, localPoint);
        }
    
        public static Vector2 GetLocalPositionFromNormalized(this RectTransform target, Vector2 normalized)
        {
            if (target == null || target.parent == null)
            {
                Debug.LogError("Invalid parameters. Make sure both transforms are not null, and the second transform is a child of the first.");
                return Vector3.zero;
            }
        
            var parentRect = target.parent.GetComponent<RectTransform>();
            var point = Rect.NormalizedToPoint(parentRect.rect, normalized);
            var worldPos = parentRect.TransformPoint(point);
            var screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out var localPosition);
            return localPosition;
        }
        
        public static void CopyPosition(this RectTransform destination, RectTransform source)
        {
            var sourceAnchoredPosition = source.anchoredPosition;
            
            var pivotOffset = new Vector2(
                (0.5f - source.pivot.x) * source.rect.width,
                (0.5f - source.pivot.y) * source.rect.height
            );
            
            var destinationAnchoredPosition = sourceAnchoredPosition + pivotOffset;
            destination.anchoredPosition = destinationAnchoredPosition;
        }

        public static void CopyProperties(this RectTransform source, RectTransform dest)
        {
            dest.anchorMin = source.anchorMin;
            dest.anchorMax = source.anchorMax;
            dest.anchoredPosition = source.anchoredPosition;
            dest.sizeDelta = source.sizeDelta;
        }
        
        public static bool Overlaps(this RectTransform a, RectTransform b) 
        {
            var bounds1 = GetBounds(a);
            var bounds2 = GetBounds(b);

            return bounds1.Intersects(bounds2);
        }

        private static Bounds GetBounds(RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(_worldCorners);

            var localToWorldMatrix = rectTransform.localToWorldMatrix;
            var center = localToWorldMatrix.MultiplyPoint3x4(rectTransform.rect.center);
            var size = rectTransform.rect.size;
            var boundsSize = localToWorldMatrix.MultiplyVector(size);

            var bounds = new Bounds(center, boundsSize);

            foreach (var corner in _worldCorners)
            {
                bounds.Encapsulate(corner);
            }

            return bounds;
        }

        public static Vector3 GetWorldCenterPosition(this RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(_worldCorners);
            return _worldCorners.Aggregate(Vector3.zero, (current, next) => current + next) / _worldCorners.Length;
        }
        
        public static float GetSizeWithCurrentAnchors(this RectTransform rectTransform, RectTransform.Axis axis)
        {
            var index = (int)axis;
            var sizeDelta = rectTransform.sizeDelta;
            var parentSize = GetParentSize();
            var anchorMin = rectTransform.anchorMin[index];
            var anchorMax = rectTransform.anchorMax[index];
            var size = sizeDelta[index] + parentSize[index] * (anchorMax - anchorMin);
            
            return size;

            Vector2 GetParentSize()
            {
                var parent = rectTransform.parent as RectTransform;
                return !(bool)(Object)parent ? Vector2.zero : parent.rect.size;
            }
        }
    }
}