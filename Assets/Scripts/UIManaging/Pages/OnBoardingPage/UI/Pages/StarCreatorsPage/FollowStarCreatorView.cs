using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal sealed class FollowStarCreatorView : MonoBehaviour
    {
        private const string FOLLOWERS_FORMAT = "{0} followers";
        
        [SerializeField] private UserPortraitView _userPortrait;
        [SerializeField] private TMP_Text _nickname;
        [SerializeField] private TMP_Text _followers;

        [Space] 
        [SerializeField] private GameObject  _selectedIndicator;
        [SerializeField] private GameObject _notSelectedIndicator;
        [SerializeField] private Button _button;

        [Inject] private OnBoardingLocalization _localization;
        
        private FollowStarCreatorModel _model;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void Initialize(FollowStarCreatorModel model)
        {
            gameObject.SetActive(true);
            _model = model;
            
            _nickname.text = model.Nickname;
            _followers.text = string.Format(_localization.FollowersCounterFormat, model.Followers);
            _userPortrait.Initialize(model.PortraitModel);
            Refresh();
        }

        private void OnButtonClick()
        {
            _model.MarkToFollow(!_model.IsMarkedToFollow);
            
            Refresh();
        }

        private void Refresh()
        {
            _selectedIndicator.SetActive(_model.IsMarkedToFollow);
            _notSelectedIndicator.SetActive(!_model.IsMarkedToFollow);
        }
    }
    
    
}