using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Common.UI
{
    public abstract class UIElementWithPlaceholder<TModel> : MonoBehaviour
    {
        // Allows to have exclusive access to initialization process
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _isInitializing;
        private bool _isContentShown;
        private TaskCompletionSource<object> _initializationCompletionSource;

        protected bool IsInitialized;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void Awake() { }

        protected virtual void OnDestroy()
        {
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual async Task InitializeAsync(TModel model, CancellationToken token)
        {
            if (IsInitialized)
            {
                Debug.LogError("Object is already initialized. Clean-up it before the re-initialization.");
            }
            else if (_isInitializing)
            {
                Debug.LogError("Object initialization is in progress right now. Cancel previous initialization first.");
            }
            else
            {
                // Wait for completion of cancellation of the previous initialization if it's running
                await _semaphore.WaitAsync(token);

                await PerformInitializationAsync(model, token);
            }
        }

        public void ShowContent()
        {
            if (IsInitialized) OnShowContent();
            _isContentShown = true;
        }

        public void HideContent()
        {
            if (IsInitialized) OnHideContent();
            _isContentShown = false;
        }

        public void CleanUp()
        {
            if (_isInitializing)
            {
                _initializationCompletionSource?.TrySetCanceled();
                Debug.Log("Failed to clean-up object while initialization isn't completed. Cancel initialization first.");
            }
            else if (IsInitialized)
            {
                OnDeactivated();
                OnCleanUp();
                _isContentShown = false;
                IsInitialized = false;
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void CompleteInitialization()
        {
            if (_isInitializing)
            {
                _initializationCompletionSource?.SetResult(null);
            }
            else
            {
                Debug.Log($"Attempt was made to complete the initialization which is {(IsInitialized ? "already completed" : "cancelled")}");
            }
        }

        protected abstract InitializationResult OnInitialize(TModel model, CancellationToken token);
        protected abstract void OnInitializationCancelled();
        protected abstract void OnShowContent();
        protected abstract void OnCleanUp();

        protected virtual void OnHideContent() { }
        protected virtual void OnActivated() { } // Raised once before the initialization. Good case for subscriptions.

        protected virtual void OnDeactivated() { } // Raised once before 'OnCleanUp' or 'OnInitializationCancelled'. Good case for unsubscribing actions.

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async Task PerformInitializationAsync(TModel model, CancellationToken token)
        {
            try
            {
                _isInitializing = true;

                OnActivated();
                var result = OnInitialize(model, token);

                // Ensure that the initialization is not cancelled
                token.ThrowIfCancellationRequested();

                if (result == InitializationResult.Wait)
                {
                    await WaitInitializationCompletionAsync(token);
                }

                token.ThrowIfCancellationRequested();

                IsInitialized = true;

                if (_isContentShown) OnShowContent();
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"{GetType()} initialization was canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isInitializing = false;
                _initializationCompletionSource = null;

                if (!IsInitialized)
                {
                    OnDeactivated();
                    OnInitializationCancelled();
                }

                _semaphore.Release();
            }
        }

        private async Task WaitInitializationCompletionAsync(CancellationToken token)
        {
            _initializationCompletionSource = new TaskCompletionSource<object>();
            using (token.Register(CancelInitialization))
            {
                await _initializationCompletionSource.Task;
            }

            void CancelInitialization()
            {
                _isInitializing = false;
                _initializationCompletionSource.TrySetCanceled();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        protected enum InitializationResult
        {
            /// <summary>
            /// Instant completion of the initialization
            /// </summary>
            Done,

            /// <summary>
            /// The initialization is supposed to be completed manually by calling completion method
            /// </summary>
            Wait
        }
    }
}