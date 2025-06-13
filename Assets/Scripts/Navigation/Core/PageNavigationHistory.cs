using System;
using System.Collections.Generic;
using System.Linq;

namespace Navigation.Core
{
    internal class PageNavigationHistory
    {
        private readonly Stack<PageData> _pageHistory = new Stack<PageData>(5);
        public int Length => _pageHistory.Count;
        
        public void Add(PageData pageData)
        {
            _pageHistory.Push(pageData);
        }

        public PageData Pop()
        {
            return _pageHistory.Pop();
        }

        public PageData Peek()
        {
            return _pageHistory.Peek();
        }
        
        public bool IsEmpty()
        {
            return _pageHistory.Count == 0;
        }

        public bool Contains(PageId pageId)
        {
            return _pageHistory.Any(page => page.PageId == pageId);
        }
        
        public PageArgs GetLastPageArgs(PageId pageId)
        {
            if (!Contains(pageId))
            {
                throw new InvalidOperationException($"Navigation history does not contain {pageId}");
            }
            return _pageHistory.First(x => x.PageId == pageId).PageArgs;
        }
    }
}