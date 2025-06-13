using Bridge;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Home
{
    public class ChallengeSectionView : MonoBehaviour
    {
        [SerializeField] private Button _viewAllButton;
        [SerializeField] private TaskListView _taskList;

        [Inject] private IBridge _bridge;
        [Inject] private VideoManager _videoManager;
        [Inject] private PageManager _pageManager;


        private void OnEnable()
        {
            _viewAllButton.onClick.AddListener(OnViewAllButtonClicked);
            var listModel = new TaskListModel(_bridge, _videoManager, _pageManager);
            _taskList.Initialize(listModel);
        }

        private void OnDisable()
        {
            _viewAllButton.onClick.RemoveAllListeners();
            _viewAllButton.interactable = true;
        }

        private void OnViewAllButtonClicked()
        {
            _viewAllButton.interactable = false;
            _pageManager.MoveNext(new TasksPageArgs());
        }
    }
}