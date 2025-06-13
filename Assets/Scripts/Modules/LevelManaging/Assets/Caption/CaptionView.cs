using System;
using Bridge.Models.ClientServer.Level.Full;
using Common.UI;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.LevelManaging.Assets.Caption
{
    public class CaptionView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] protected TMP_Text TextView;
        
        private TextSizer _textSizer;
        private Canvas _canvas;
        private RectTransform _rectTransform;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public long CaptionId { get; private set; }
        public float FontSize => TextView.fontSize;
        public Color Color => TextView.color;
        public CaptionTextAlignment Alignment => TextView.alignment.ToCaptionTextAlignment();
        
        public string Text
        {
            get => TextView.text;
            set
            {
                TextView.text = value;
                TextSizer.RefreshImmediate();
            }
        }
        
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        public TMP_Text TextComponent => TextView;

        private TextSizer TextSizer
        {
            get
            {
                if (_textSizer == null)
                {
                    _textSizer = GetComponentInChildren<TextSizer>(true);
                }

                return _textSizer;
            }
        }

        private bool IsActive => gameObject.activeSelf;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<long> Clicked; 

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake()
        {
            //Workaround for FREV-11284. Not able to set SortingLayer while mode is set to ScreenSpaceCamera.
            //Start mode is WorldSpace with SortingLayer set to Layer1
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        private void OnDestroy()
        {
            Clicked = null;
        }

        //---------------------------------------------------------------------
        // IPointerClickHandler
        //---------------------------------------------------------------------

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnClicked();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        public void SetTargetCaptionId(long id)
        {
            CaptionId = id;
        }
        
        public void SetNormalizedPosition(Vector2 normalized)
        {
            RectTransform.localPosition = RectTransform.GetLocalPositionFromNormalized(normalized);
        }

        public void ForceRefresh()
        {
            var isCaptionActive = IsActive;
            if (!isCaptionActive)
            {
                SetActive(true);
            }
            
            var textComponentEnabled = TextComponent.enabled;
            if (!TextComponent.enabled)
            {
                TextComponent.enabled = true;
            }

            TextComponent.ForceMeshUpdate(ignoreActiveState:true, forceTextReparsing:true);
            TextComponent.enabled = textComponentEnabled;
            
            SetActive(isCaptionActive);
        }

        public void SetFontSize(float fontSize)
        {
            TextView.fontSize = fontSize;
            TextSizer.RefreshImmediate();
        }  
        
        public void SetRotation(float eulerAngle)
        {
            RectTransform.SetLocalEulerAngleZ(eulerAngle);
        }
        
        public void SetColor(Color color)
        {
            TextView.color = color;
        }
        
        public void SetAlignment(CaptionTextAlignment alignment)
        {
            TextView.alignment = alignment.ToTMPTextAlignment();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        private void OnClicked()
        {
            Clicked?.Invoke(CaptionId);
        }
    }
}