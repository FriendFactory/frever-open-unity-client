using Bridge.Results;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal class PageResult<T>: Result
    {
        public readonly T[] Models;

        private static PageResult<T> _cachedCancelledResult;

        internal PageResult(T[] models)
        {
            Models = models;
        }

        internal PageResult(string errorMessage) : base(errorMessage)
        {
        }

        internal PageResult(bool isCanceled) : base(isCanceled)
        {
        }

        internal static PageResult<T> Success(T[] models)
        {
            return new PageResult<T>(models);
        }

        internal static PageResult<T> Error(string error)
        {
            return new PageResult<T>(error);
        }

        internal static PageResult<T> Cancelled()
        {
            return _cachedCancelledResult ?? (_cachedCancelledResult = new PageResult<T>(true));
        }
    }
}