using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.Tasks.TaskVideosGridPage;
using UnityEngine;

namespace UIManaging.Pages.Tasks
{
    public class OnboardingTaskListModel: ITaskListModel
    {
        public IReadOnlyList<TaskModel> Tasks => _tasks;
        public TimeSpan? TimeToNewTasks => null;

        private List<OnboardingTaskModel> _tasks;

        #pragma warning disable CS0067
        public event Action TasksChanged;
        #pragma warning restore CS0067
        public event Action TaskVideoClicked;

        private readonly VideoManager _videoManager;
        private readonly PageManager _pageManager;
        private readonly IBridge _bridge;
        private readonly CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // ctors
        //---------------------------------------------------------------------
        
        public OnboardingTaskListModel(VideoManager videoManager, PageManager pageManager, IBridge bridge)
        {
            _bridge = bridge;
            _pageManager = pageManager;
            _videoManager = videoManager;
            _tokenSource = new CancellationTokenSource();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void CleanUp()
        {
            TasksCleanUp();
            
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
        
        public void RequestPage()
        {
            TasksCleanUp();
            //todo: drop onboarding tasks logic since GetOnBoardingTasksAsync does not exist anymore on API 1.8, FREV-15880
            /*
            var result = await _bridge.GetOnBoardingTasksAsync(_tokenSource.Token); 

            if (result.IsError)
            {
                Debug.LogError("Error loading task videos: " + result.ErrorMessage);
                return;
            }

            if (result.IsSuccess)
            {
                var currentTask = result.Models.Last();
                var existingModel = _tasks?.FirstOrDefault(taskModel => taskModel.Task.Id == currentTask.Id);
                
                _tasks = new List<OnboardingTaskModel> { existingModel ?? new OnboardingTaskModel(_videoManager, _pageManager, _bridge, currentTask) };
            
                foreach (var task in _tasks)
                {
                    task.VideoClicked += OnTaskVideoClicked;
                }
                
                TasksChanged?.Invoke();
            }
            */
        }

        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------
        
        private void TasksCleanUp()
        {
            if (_tasks != null)
            {
                foreach (var task in _tasks)
                {
                    task.VideoClicked -= OnTaskVideoClicked;
                }
            }
        }

        private void OnTaskVideoClicked()
        {
            TaskVideoClicked?.Invoke();
        }
    }
}