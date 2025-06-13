using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Navigation.Args.SeasonPageArgs.Tab;

namespace UIManaging.Pages.Common.Seasons
{
    public class SeasonLikesMilestoneView : MonoBehaviour
    {
        [SerializeField] private Button[] _readButtons;

        [Inject] private ISeasonLikesNotificationHelper _seasonLikesNotificationHelper;
        [Inject] private PageManager _pageManager;

        private INotificationSource _source;
        
        private void Awake()
        {
            _source = GetComponent<INotificationSource>();
        }

        private void OnEnable()
        {
            foreach (var button in _readButtons)
            {
                button.onClick.AddListener(OnClick);
            }
            
            TryMarkNotificationAsRead();
            if (_source != null)
            {
                _source.NotificationReceived += TryMarkNotificationAsRead;
            }
        }

        private void OnDisable()
        {
            foreach (var button in _readButtons)
            {
                button.onClick.RemoveListener(OnClick);
            }
            if (_source != null)
            {
                _source.NotificationReceived -= TryMarkNotificationAsRead;
            }
        }

        private void OnClick()
        {
            _pageManager.MoveNext(new SeasonPageArgs(Quests, _source?.QuestId));
            TryMarkNotificationAsRead();
        }

        private void TryMarkNotificationAsRead()
        {
            if (_source?.NotificationId != null && _source.ShouldRead)
            {
                _seasonLikesNotificationHelper.MarkNotificationAsDisplayed(_source.NotificationId.Value);
            }
        }
    }
}