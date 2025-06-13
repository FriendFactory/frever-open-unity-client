using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Amplitude;
using UnityEngine;

namespace Navigation.Core
{
    public sealed class PageManager : MonoBehaviour
    {
        [SerializeField] private PageInfo[] _pageInfo;

        private readonly List<Page> _loadedPages = new();
        private readonly PageNavigationHistory _pageHistory = new();
        
        private GarbageManager _garbageManager;

        private PageData? _currentPageData;
        private Page _nextPage;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<PageId?, PageData> PageSwitchingBegan;
        public event Action<PageData> PageDisplayed;
        public event Action<bool> ApplicationFocus;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public Page CurrentPage { get; private set; }
        public bool IsChangingPage { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnApplicationFocus(bool hasFocus)
        {
            ApplicationFocus?.Invoke(hasFocus);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init()
        {
            _garbageManager = new GarbageManager(this);
            _garbageManager.Run();
        }

        public void MoveNext(PageArgs pageArgs, bool savePrevPageToHistory = true)
        {
            MoveNext(pageArgs.TargetPage, pageArgs, savePrevPageToHistory);
        }
        
        public void MoveNext(PageId pageId, PageArgs pageArgs, bool savePrevPageToHistory = true)
        {
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = savePrevPageToHistory
            };

            OverrideFeedPageIdIfNeeded(ref pageId);

            MoveNext(pageId, pageArgs, transitionArgs);
        }
        
        public void MoveNext(PageArgs pageArgs, PageTransitionArgs transitionArgs)
        {
            MoveNext(pageArgs.TargetPage, pageArgs, transitionArgs);
        }
        
        public void MoveNext(PageId pageId, PageArgs pageArgs, PageTransitionArgs transitionArgs)
        {
            OverrideFeedPageIdIfNeeded(ref pageId);
            
            if (ShouldSaveCurrentPageToHistory(transitionArgs))
            {
                var backArgs = GetPageDataForMovingBack(transitionArgs);
                _pageHistory.Add(backArgs);
            }

            MoveToPage(pageId, pageArgs, transitionArgs);
        }
        
        public void MoveBack()
        {
            var pageData = _pageHistory.Pop();
            pageData.PageArgs.Backed = true;
            MoveToPage(pageData.PageId, pageData.PageArgs);
        }
        
        public void MoveBack(PageTransitionArgs transitionArgs)
        {
            var pageData = _pageHistory.Pop();
            pageData.PageArgs.Backed = true;
            MoveToPage(pageData.PageId, pageData.PageArgs, transitionArgs);
        }

        public void MoveBackTo(PageId pageId, PageArgs pageArgs = null)
        {
            OverrideFeedPageIdIfNeeded(ref pageId);
            
            var pageData = RollbackHistoryTo(pageId);

            var overrideArgs = pageArgs != null;
            if (overrideArgs)
            {
                pageData = new PageData {PageArgs = pageArgs, PageId = pageId};
            }

            pageData.PageArgs.Backed = true;
            MoveToPage(pageData.PageId, pageData.PageArgs);
        }
        
        public void TryMoveBackTo(PageId pageId, PageArgs pageArgs)
        {
            OverrideFeedPageIdIfNeeded(ref pageId);
            
            if (HistoryContains(pageId))
            {
                MoveBackTo(pageId, pageArgs);
            }
            else
            {
                MoveNext(pageId, pageArgs, false);
            }
        }

        public bool IsCurrentPage(PageId id)
        {
            OverrideFeedPageIdIfNeeded(ref id);
            
            return CurrentPage != null && CurrentPage.Id == id;
        }

        public bool IsPreviousPage(PageId id)
        {
            return _pageHistory.Peek().PageId == id;
        }

        public bool HistoryContains(PageId pageId)
        {
            OverrideFeedPageIdIfNeeded(ref pageId);
            
            return _pageHistory.Contains(pageId);
        }

        public PageArgs GetLastArgsForPage(PageId targetPage)
        {
            OverrideFeedPageIdIfNeeded(ref targetPage);
            
            return _pageHistory.GetLastPageArgs(targetPage);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SwitchPage(PageId nextPageId, PageArgs pageArgs, PageTransitionArgs transitionArgs)
        {
            var prevPageId = CurrentPage?.Id;
            var nextPageData = new PageData
            {
                PageArgs = pageArgs, PageId = nextPageId
            };

            OnPageSwitchingBegan(nextPageData, prevPageId);
            transitionArgs.TransitionStartedCallback?.Invoke();

            // We need to keep the right order of execution here
            Action onTransitionFinished = OnTransitionFinished;
            onTransitionFinished += transitionArgs.TransitionFinishedCallback;

            if (CurrentPage != null && CurrentPage.Id != nextPageId && !transitionArgs.HidePreviousPageOnOpen)
            {
                CurrentPage.Hide();
            }
            
            Action onLoadingCanceled = OnLoadingCanceled;
            _nextPage = _loadedPages.FirstOrDefault(x => x.Id.Equals(nextPageData.PageId));
            _nextPage.Display(nextPageData.PageArgs, onTransitionFinished, onLoadingCanceled);
            
            void OnTransitionFinished()
            {
                if (transitionArgs.HidePreviousPageOnOpen && !transitionArgs.LeavePreviousPageOpenOnTransitionFinished)
                {
                    CurrentPage.Hide();
                }
                
                CurrentPage = _nextPage;
                _currentPageData = nextPageData;
                OnPageSwitchingEnded(nextPageData, prevPageId);
                PageDisplayed?.Invoke(nextPageData);
            }

            void OnLoadingCanceled()
            {
                IsChangingPage = false;
            }
        }

        private void OnPageSwitchingEnded(PageData nextPage, PageId? prevPageId)
        {
            IsChangingPage = false;
        }

        private void OnPageSwitchingBegan(PageData nextPageData, PageId? prevPageId)
        {
            IsChangingPage = true;
            PageSwitchingBegan?.Invoke(prevPageId, nextPageData);
        }

        private bool ShouldSaveCurrentPageToHistory(PageTransitionArgs transitionArgs)
        {
            return CurrentPage != null && _currentPageData.HasValue && transitionArgs.SaveCurrentPageToHistory;
        }

        private PageData GetPageDataForMovingBack(PageTransitionArgs transitionArgs)
        {
            if (!transitionArgs.OverrideCurrentPageArgs)
            {
                return new PageData
                {
                    PageId =  CurrentPage.Id,
                    PageArgs = CurrentPage.GetBackToPageArgs()
                };
            }

            return new PageData
            {
                PageId = CurrentPage.Id,
                PageArgs = transitionArgs.CurrentPageArgs
            };
        }

        private PageData RollbackHistoryTo(PageId pageId) 
        {
            PageData? lastSavedArgs = null;

            while (!_pageHistory.IsEmpty() && (lastSavedArgs == null || lastSavedArgs.Value.PageId != pageId))
            {
                lastSavedArgs = _pageHistory.Pop();
            }

            if (lastSavedArgs == null)
            {
                throw new InvalidOperationException($"Page navigation does not have {pageId} in navigation history");
            }
            
            return lastSavedArgs.Value;
        }

        private void MoveToPage(PageId pageId, PageArgs pageArgs)
        {
            MoveToPage(pageId, pageArgs, PageTransitionArgs.Default());
        }

        private void MoveToPage(PageId nextPageId, PageArgs pageArgs, PageTransitionArgs transitionArgs)
        {
            if (_loadedPages.Exists(page => page.Id == nextPageId))
            {
                SwitchPage(nextPageId, pageArgs, transitionArgs);
            }
            else
            {
                _loadedPages.ForEach(page => page.CleanUp());
                
                var nextScene = _pageInfo.FirstOrDefault(info => info.PageIds.Contains(nextPageId));
                var nextScenePath = nextScene.SceneAsset.ScenePath;

                SceneManager.LoadSceneIfNeeded(nextScenePath, OnLoaded, !nextScene.LoadSceneSynchronously);

                void OnLoaded()
                {
                    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(nextScenePath);
                    var pagesRegister = scene.GetRootGameObjects().First(x=>x.GetComponent<ScenePagesRegister>() != null).GetComponent<ScenePagesRegister>();

                    _loadedPages.Clear();
                    _loadedPages.AddRange(pagesRegister.Pages);

                    foreach (var page in _loadedPages)
                    {
                        page.Init(this);
                        page.gameObject.SetActive(false);
                    }

                    SwitchPage(nextPageId, pageArgs, transitionArgs);
                }
            }
        }

        private void OverrideFeedPageIdIfNeeded(ref PageId pageId)
        {
            if (pageId != PageId.Feed) return;

            pageId = AmplitudeManager.IsGamifiedFeedEnabled() ? PageId.GamifiedFeed : PageId.Feed;
        }
    }
}