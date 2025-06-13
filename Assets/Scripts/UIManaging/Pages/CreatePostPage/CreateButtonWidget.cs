using DG.Tweening;
using Extensions;
using Modules.GalleryVideoManaging;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Sirenix.OdinInspector;
using TMPro;
using TweenExtensions;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.CreatePostPage
{
    public sealed class CreateButtonWidget : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        [SerializeField] private CreatePostPageButtonAnimator[] _buttonAnimators;
        [SerializeField] private UploadGalleryButton _uploadGalleryVideoButton;
        [SerializeField] private float _animationDuration = 0.3f;
    
        [SerializeField] private Image _uploadIcon;
        [SerializeField] private TMP_Text _uploadText;
    
        [SerializeField] private Color _uploadEnabledButtonColor;
        [SerializeField] private Color _uploadDisabledButtonColor;
    
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IScenarioManager _scenarioManager;

        private Sequence _expandSequence;
        private bool _isExpanded;
    
        private void Awake()
        {
            InitAnimations();
            InitUploadButton();
        }

        private void OnEnable()
        {
            _uploadGalleryVideoButton.VideoSelected += OnGalleryVideoSelected;
        }

        private void OnDisable()
        {
            _uploadGalleryVideoButton.VideoSelected -= OnGalleryVideoSelected;
        }

        public void PlayAnimation(bool state, bool instant = false)
        {
            if (_isExpanded == state) return;
            _isExpanded = state;
            _expandSequence.PlayByState(state, instant);
            _buttonAnimators.ForEach(a => a.PlayAnimation(state, instant));
        }

        private void OnGalleryVideoSelected(NonLeveVideoData videoData)
        {
            _scenarioManager.ExecuteNonLevelVideoCreationScenario(videoData);
        }

        private void InitAnimations()
        {
            _expandSequence = DOTween.Sequence()
                                     .Join(_rectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, 440f), 0.3f))
                                     .Join(_image.DOPixelsPerUnitMultiplier(2.5f, 0.3f))
                                     .SetEase(Ease.OutQuad)
                                     .Pause()
                                     .SetAutoKill(false);
        
            _buttonAnimators.ForEach(a => a.InitAnimation(_animationDuration));
        }
    
        private void InitUploadButton()
        {
            if (_localUser.LevelingProgress.AllowVideoToFeed)
            {
                _uploadIcon.color = _uploadEnabledButtonColor;
                _uploadText.color = Color.black;
                return;
            }
    
            _uploadIcon.color = _uploadDisabledButtonColor;
            _uploadText.color = new Color(0,0,0,0.5f);
        }

        [Button]
        private void TestAnim()
        {
            animState = !animState;
            PlayAnimation(animState);
        }

        private bool animState = false;
    }
}
