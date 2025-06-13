using System;
using Bridge;
using Common;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    public class TemplateUsedForVideoButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _templateNameText;
        
        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private Button _button;
        private RectTransform _rectTransform;
        private long _templateId;

        protected Button Button => _button;
        protected long TemplateID => _templateId;

        public event Action ButtonClicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
       
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void Initialize(long templateId, string templateTitle, UnityAction onClick)
        {
            if(_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            
            _templateId = templateId;
            SetTemplateName(templateTitle);

            if (onClick != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(onClick);
            }
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void RebuildLayout()
        {
            CoroutineSource.Instance.ExecuteWithFrameDelay(() => LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        protected virtual async void OnClick()
        {
            var response = await _bridge.GetEventTemplate(_templateId);

            if (response.IsError)
            {
                _snackBarHelper.ShowInformationSnackBar("Template is unavailable");
                return;
            }
            
            ButtonClicked?.Invoke();

            var pageArgs = new VideosBasedOnTemplatePageArgs
            {
                TemplateInfo = response.Model,
                TemplateName =  response.Model.Title,
                UsageCount =  response.Model.UsageCount
            };

            _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, pageArgs);
        }

        private void SetTemplateName(string templateName)
        {
            if (!_templateNameText) return;
            
            _templateNameText.text = templateName;
            RebuildLayout();
        }
    }
}