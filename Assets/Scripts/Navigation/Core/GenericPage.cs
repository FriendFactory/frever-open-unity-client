using System;

namespace Navigation.Core
{
    public abstract class GenericPage<T> : Page where T: PageArgs
    {
        public T OpenPageArgs { get; private set; }
        protected virtual T BackToPageArgs => OpenPageArgs;

        private Action _onOpened;
        private Action _onOpeningCanceled;

        protected override bool CheckArgs(PageArgs pageArgs)
        {
            var isValid = pageArgs.TargetPage == Id;
            if (isValid)
            {
                OpenPageArgs = pageArgs as T;
            }
            return isValid;
        }

        internal sealed override PageArgs GetBackToPageArgs()
        {
            return BackToPageArgs;
        }

        protected sealed override void OnDisplayStart(PageArgs args, Action onDisplayed, Action onLoadingCanceled)
        {
            _onOpened = onDisplayed;
            _onOpeningCanceled = onLoadingCanceled;
            gameObject.SetActive(true);
            OnDisplayStart(args as T);
        }

        protected virtual void OnDisplayStart(T args)
        {
            // you can override this method in derived classes
            OnPageOpened();
        }

        protected void OnPageOpened()
        {
            _onOpened?.Invoke();
        }

        protected void OnPageOpeningCanceled()
        {
            _onOpeningCanceled?.Invoke();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
