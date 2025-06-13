using Common.UI;
using DG.Tweening;
using Modules.GalleryVideoManaging;
using Navigation.Args;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SwitchingEditors
{
    public abstract class SwitchEditorOptionsPanelBase : MonoBehaviour
    {
        [Range(0, 1f)]
        [SerializeField] private float _appearingTimeSec = 0.5f;
        [SerializeField] private CanvasGroup _transparencyControl;
        [SerializeField] protected Button UploadButton;
        [SerializeField] protected Image UploadButtonTip;
        [SerializeField] protected Color DisabledColor;
        [SerializeField] protected OutsideClickDetector OutsideClickDetector;

        [Inject] private IUploadGalleryVideoService _uploadGalleryVideoService;

        private Tween _appearingTween;
        private Tween _hidingTween;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool IsFeatureEnabled => _uploadGalleryVideoService.IsVideoToFeedAllowed;
        protected abstract Button[] Buttons { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void Init()
        {
            _appearingTween = _transparencyControl.DOFade(1, _appearingTimeSec).SetAutoKill(false).Pause();
            _hidingTween = _transparencyControl.DOFade(0, _appearingTimeSec).SetAutoKill(false).Pause().OnComplete(()=>gameObject.SetActive(false));
            SetupUploadButtonColor();
            
            foreach (var button in Buttons)
            {
                var colorBlock = button.colors;
                colorBlock.disabledColor = DisabledColor;
                button.colors = colorBlock;
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            _hidingTween.Pause();
            _transparencyControl.alpha = 0;
            _appearingTween.Restart();
            StartListeningToOutsideClickEvent();
            UploadButton.onClick.AddListener(OnUploadClicked);
        }

        public virtual void Hide()
        {
            _appearingTween.Pause();
            _hidingTween.Restart();
            StopListeningToOutsideClickEvent();
            UploadButton.onClick.RemoveListener(OnUploadClicked);
        }
        
        public void StartListeningToOutsideClickEvent()
        {
            OutsideClickDetector.OutsideClickDetected += Hide;
        }
        
        public void StopListeningToOutsideClickEvent()
        {
            OutsideClickDetector.OutsideClickDetected -= Hide;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void OnDestroy()
        {
            _appearingTween.Kill();
            _hidingTween.Kill();
        }
        
        protected abstract void OnVideoSelected(NonLeveVideoData nonLeveVideoData);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnUploadClicked()
        {
            UploadButton.interactable = false;
            SetUploadButtonColor(DisabledColor);
            OutsideClickDetector.enabled = false;
            _uploadGalleryVideoService.TryToOpenVideoGallery(OnVideoSelected, onFailed: ()=>
            {
                UploadButton.interactable = true;
                OutsideClickDetector.enabled = true;
                SetupUploadButtonColor();
            });
        }
        
        private void SetupUploadButtonColor()
        {
            var color = IsFeatureEnabled ? Color.white : DisabledColor;
            SetUploadButtonColor(color);
        }

        private void SetUploadButtonColor(Color color)
        {
            UploadButton.image.color = color;
            UploadButtonTip.color = color;
        }
    }
}