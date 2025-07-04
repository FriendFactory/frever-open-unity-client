﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Common
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextMeshProLinksOpener : MonoBehaviour, IPointerClickHandler
    {
        private TextMeshProUGUI _textMeshPro;
        [SerializeField] private Vector2 _linkHitboxIncrement = new Vector2(10,10);

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerClick(PointerEventData eventData) 
        {
            var linkIndex = FindIntersectingLink(_textMeshPro, Input.mousePosition, Camera.main, _linkHitboxIncrement);
            if( linkIndex != -1 ) 
            { 
                var linkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
                HandleLink(linkInfo);
            }
        }

        protected virtual void HandleLink(TMP_LinkInfo linkInfo)
        {
            Application.OpenURL(linkInfo.GetLinkID());
        }

        // Copied from TMPro.TMP_TextUtilities.cs
        // Added linkHitboxOffset parameter to be able to enlarge link hitbox
        private static int FindIntersectingLink(TMP_Text text, Vector3 position, Camera camera, Vector3 linkHitboxOffset = default)
        {
            Transform rectTransform = text.transform;

            // Convert position into Worldspace coordinates
            TMP_TextUtilities.ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    // Check if Link characters are on the current page
                    if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            //Add hitbox size offset
                            bl += new Vector3(-linkHitboxOffset.x, -linkHitboxOffset.y);
                            br += new Vector3(linkHitboxOffset.x, -linkHitboxOffset.y);
                            tl += new Vector3(-linkHitboxOffset.x, linkHitboxOffset.y);
                            tr += new Vector3(linkHitboxOffset.x, linkHitboxOffset.y);
                            
                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        //Add hitbox size offset
                        bl += new Vector3(-linkHitboxOffset.x, -linkHitboxOffset.y);
                        br += new Vector3(linkHitboxOffset.x, -linkHitboxOffset.y);
                        tl += new Vector3(-linkHitboxOffset.x, linkHitboxOffset.y);
                        tr += new Vector3(linkHitboxOffset.x, linkHitboxOffset.y);
                        
                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        //Add hitbox size offset
                        bl += new Vector3(-linkHitboxOffset.x, -linkHitboxOffset.y);
                        br += new Vector3(linkHitboxOffset.x, -linkHitboxOffset.y);
                        tl += new Vector3(-linkHitboxOffset.x, linkHitboxOffset.y);
                        tr += new Vector3(linkHitboxOffset.x, linkHitboxOffset.y);
                        
                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }
         
        // Copied from TMPro.TMP_TextUtilities.cs
         private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
         {
             Vector3 ab = b - a;
             Vector3 am = m - a;
             Vector3 bc = c - b;
             Vector3 bm = m - b;

             float abamDot = Vector3.Dot(ab, am);
             float bcbmDot = Vector3.Dot(bc, bm);

             return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
         }
    }
}
