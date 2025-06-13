using System;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation.Core
{
    /// <summary>
    /// Responsible for triggering garbage collection on page switching
    /// </summary>
    internal sealed class GarbageManager
    {
        private static readonly HashSet<PageId> PAGES_REQUIRED_RAM_CLEANUP = new HashSet<PageId>
        {
            PageId.AvatarCreation,
            PageId.Feed,
            PageId.GamifiedFeed,
            PageId.LevelEditor,
            PageId.PublishPage,
            PageId.UmaEditorNew
        };

        private readonly PageManager _pageManager;

        public GarbageManager(PageManager pageManager)
        {
            _pageManager = pageManager;
        }
        
        public void Run()
        {
            _pageManager.PageSwitchingBegan += OnPageSwitching;
        }

        private static void OnPageSwitching(PageId? fromPage, PageData toPage)
        {
            var needMemoryCleanup = RequiresMemoryCleanupBeforeDisplay(toPage.PageId) ||
                                    fromPage != null && RequiresMemoryCleanupAfterClosing(fromPage.Value);
            if (needMemoryCleanup)
            {
                CollectGarbage();
            }
        }

        private static bool RequiresMemoryCleanupBeforeDisplay(PageId pageId)
        {
            return PAGES_REQUIRED_RAM_CLEANUP.Contains(pageId);
        }

        private static bool RequiresMemoryCleanupAfterClosing(PageId pageId)
        {
            return PAGES_REQUIRED_RAM_CLEANUP.Contains(pageId);
        }

        private static void CollectGarbage()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
}