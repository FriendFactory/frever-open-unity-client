using System.Linq;
using Bridge.Models.ClientServer.Chat;
using Common.Publishers;
using DG.Tweening;
using Extensions;
using Modules.GalleryVideoManaging;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.Panels
{
    public sealed class PostTypeSelectionPanel : MonoBehaviour
    {
        [SerializeField] private float _appearingTime = 0.2f;
        [SerializeField] private Button _videoPostButton;
        [SerializeField] private Button _videoMessageButton;
        [SerializeField] private Button _uploadFromGalleryButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _header;

        [Inject] private IScenarioManager _scenarioManager;
        [Inject] private IUploadGalleryVideoService _uploadGalleryVideoService;
        [Inject] private IUniverseManager _universeManager;

        private ChatInfo _chatInfo;
        
        private float _height;
        private readonly Vector3[] _containerWorldCorners = new Vector3[4];
        private Sequence _currentTween;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsShown { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private float ShownPositionY
        {
            get
            {
                _header.GetWorldCorners(_containerWorldCorners);
                var maxPosY = _containerWorldCorners.Max(x => x.y);
                return maxPosY + _height;
            }
        }

        private void Awake()
        {
            _videoPostButton.onClick.AddListener(OnVideoPostButtonClicked);
            _videoMessageButton.onClick.AddListener(OnVideoMessageButtonClicked);
            _uploadFromGalleryButton.onClick.AddListener(OnUploadFromGalleryButtonClicked);
            _height = transform.GetComponent<RectTransform>().GetHeight();

            gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(ChatInfo chatInfo)
        {
            _chatInfo = chatInfo;
        }
        
        public void Show()
        {
            if (IsShown) return;
            gameObject.SetActive(true);
            PlayAppearingAnimation();
            IsShown = true;
        }

        public void Hide(bool immediate = false)
        {
            if (!IsShown) return;
            IsShown = false;
            if (immediate)
            {
                gameObject.SetActive(false);
            }
            else
            {
                PlayHideAnimation();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnVideoPostButtonClicked()
        {
            _canvasGroup.interactable = false;
            _universeManager.SelectUniverse(universe => _scenarioManager.ExecuteNewLevelCreation(universe: universe, chatInfo:_chatInfo));
        }

        private void OnVideoMessageButtonClicked()
        {
            _universeManager.SelectUniverse(universe => 
            {
                _canvasGroup.interactable = false;
                _scenarioManager.ExecuteVideoMessageCreation(universe, _chatInfo, PublishingType.VideoMessage);
            });
        }

        private void OnUploadFromGalleryButtonClicked()
        {
            if (_uploadGalleryVideoService.IsVideoToFeedAllowed)
            {
                _canvasGroup.interactable = false;
            }
            
            _uploadGalleryVideoService.TryToOpenVideoGallery(OpenPublishNonLevelVideo, () => _canvasGroup.interactable = true, ()=> _canvasGroup.interactable = true);
            
            void OpenPublishNonLevelVideo(NonLeveVideoData videoData)
            {
                _scenarioManager.ExecuteNonLevelVideoCreationScenario(videoData, _chatInfo);
            }
        }
        
        private void PlayAppearingAnimation()
        {
            transform.SetPositionY(ShownPositionY - _height);
            _canvasGroup.alpha = 0;
            _currentTween?.Kill();
            _currentTween = PlayAppearingTween();
        }

        private void PlayHideAnimation()
        {
            _currentTween?.Kill();
            _currentTween = CreateHidingTween().Play();
        }
        
        private Sequence CreateHidingTween()
        {
            return DOTween.Sequence()
                          .Append(transform.DOMoveY(ShownPositionY - _height, _appearingTime))
                          .Join(_canvasGroup.DOFade(0, _appearingTime))
                          .SetAutoKill(true).OnComplete(() => gameObject.SetActive(false));
        }

        private Sequence PlayAppearingTween()
        {
            return DOTween.Sequence()
                          .Append(transform.DOMoveY(ShownPositionY, _appearingTime))
                          .Join(_canvasGroup.DOFade(1, _appearingTime))
                          .SetAutoKill(true);
        }
    }
}
