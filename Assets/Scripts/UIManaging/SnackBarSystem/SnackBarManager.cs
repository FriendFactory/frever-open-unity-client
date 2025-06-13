using System.Collections.Generic;
using Common;
using Common.Pools;
using UI.UIAnimators;
using UIManaging.SnackBarSystem.Configurations;
using UIManaging.SnackBarSystem.SnackBars;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem
{
    public sealed class SnackBarManager : MonoBehaviour
    {
        [SerializeField] private SnackBarFactory _factory;
        [SerializeField] private Transform _parent;
        [SerializeField] private SequentialUiAnimationPlayer _animationPlayer;
        [SerializeField] private ColorUiAnimator _colorAnimationPlayer;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _showTimeInSeconds = 3f;
        [SerializeField] private int _queueLimit = 3;

        private MoveUiAnimator _moveUiAnimator;
        
        private SnackBar _currentSnackBar;
        private readonly IList<SnackBar> _queue = new List<SnackBar>();
        private IPool<SnackBar> _pool;

        private Coroutine _showingRoutine;  // Routine is active when SnackBar is in 'Shown' state

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _pool = new ManualPool<SnackBar>(DestroySnackBar);
            _moveUiAnimator = _animationPlayer.GetComponentInChildren<MoveUiAnimator>();
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = Constants.SNACKBAR_SORTING_LAYER;
        }

        private void OnDestroy()
        {
            _queue.Clear();
            _pool.Clear();  // SnackBarks are destroyed here
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public SnackBarToken Show(SnackBarConfiguration configuration)
        {
            SnackBarToken token = null;
            
            var snackBar = TryGetFromPool(configuration.Type) ?? CreateSnackBar(configuration.Type);
            if (snackBar != null)
            {
                snackBar.Configure(configuration);
                Enqueue(snackBar);
                ShowNextIfReady();

                token = snackBar.Token;
            }

            return token;
        }

        public void PlayBlinking()
        {
            _colorAnimationPlayer.PlayBlinkAnimation();
        }

        public bool IsShowing(SnackBarType type)
        {
            if (_currentSnackBar == null || _currentSnackBar.Type != type) return false;
            return _currentSnackBar?.State != SnackBarState.Hidden;
        }

        public void CancelAutoHiding()
        {
            if (_showingRoutine == null) return;
            CoroutineSource.Instance.StopCoroutine(_showingRoutine);
            _showingRoutine = null;
        }
        
        //---------------------------------------------------------------------
        // Show\Hide
        //---------------------------------------------------------------------

        private void ShowNextIfReady()
        {
            if (_currentSnackBar != null) return;   // Some SnackBar is shown right now
            if (_queue.Count <= 0) return;  // No more SnackBars to show

            _currentSnackBar = Dequeue();
            AppearCurrentSnackBar();
        }

        private void AppearCurrentSnackBar()
        {
            // Animation configuration
            _moveUiAnimator.animationTarget = _currentSnackBar.GetComponent<RectTransform>();
            _colorAnimationPlayer.animationTarget = _currentSnackBar.GetComponentInChildren<Image>();
            _animationPlayer.PlayShowAnimation(OnCurrentSnackBarShown);
            
            _currentSnackBar.OnAppearing();
        }

        private void DisappearCurrentSnackBar()
        {
            _animationPlayer.PlayHideAnimation(OnCurrentSnackBarHidden);
            _currentSnackBar.OnDisappearing();
        }

        private void DeactivateCurrentSnackBar()
        {
            DeactivateSnackBar(_currentSnackBar);
            _currentSnackBar = null;
        }
        
        private static void DeactivateSnackBar(SnackBar snackBar)
        {
            snackBar.OnHidden();
            snackBar.MarkAsUsed();
        }
        
        private void ForceHideCurrentSnackBar()
        {
            if (_currentSnackBar == null) return;   // No SnackBar to hide

            switch (_currentSnackBar.State)
            {
                case SnackBarState.Appearing:
                    _animationPlayer.CancelCurrentAnimation();
                    DisappearCurrentSnackBar();
                    break;

                case SnackBarState.Shown:
                    if (_showingRoutine != null)
                    {
                        StopCoroutine(_showingRoutine);
                        _showingRoutine = null;
                    }
                    
                    DisappearCurrentSnackBar();
                    break;
            }
        }

        //---------------------------------------------------------------------
        // Animation Events
        //---------------------------------------------------------------------

        private void OnCurrentSnackBarShown()
        {
            _currentSnackBar.OnShown();
            _showingRoutine = CoroutineSource.Instance.ExecuteWithRealtimeDelay(_currentSnackBar.Time ?? _showTimeInSeconds, DisappearCurrentSnackBar);
        }
        
        private void OnCurrentSnackBarHidden()
        {
            DeactivateCurrentSnackBar();
            ShowNextIfReady();
        }

        //---------------------------------------------------------------------
        // SnackBar Events
        //---------------------------------------------------------------------

        private void SubscribeToSnackBar(SnackBar snackBar)
        {
            snackBar.HideRequested += OnHideSnackbarRequested;
        }
        
        private void UnsubscribeFromSnackBar(SnackBar snackBar)
        {
            snackBar.HideRequested -= OnHideSnackbarRequested;
        }
        
        private void OnHideSnackbarRequested(SnackBar snackBarToHide)
        {
            if (_currentSnackBar != null && _currentSnackBar == snackBarToHide)
            {
                // Current SnackBar has requested to get hidden
                ForceHideCurrentSnackBar();
            }
            else if(_queue.Count > 0)
            {
                if (_queue.Remove(snackBarToHide))
                {
                    // SnackBar to hide isn't shown yet
                    DeactivateSnackBar(snackBarToHide);
                    snackBarToHide.MarkAsUsed();
                }
            }
        }
        
        //---------------------------------------------------------------------
        // Other
        //---------------------------------------------------------------------

        private SnackBar TryGetFromPool(SnackBarType type)
        {
            return _pool.Get(snackBar => snackBar.Type == type);
        }

        private SnackBar CreateSnackBar(SnackBarType type)
        {
            var snackBar = _factory.Create(type, _parent);
            if (snackBar != null)
            {
                SubscribeToSnackBar(snackBar);
                _pool.Add(snackBar, false);
            }
            return snackBar;
        }

        private void DestroySnackBar(SnackBar snackBar)
        {
            UnsubscribeFromSnackBar(snackBar);
            Destroy(snackBar);
        }

        private void Enqueue(SnackBar snackBar)
        {
            if (_queue.Count >= _queueLimit)
            {
                // Limit is reached. The oldest item should be removed from queue
                Dequeue().MarkAsUsed();
            }

            _queue.Add(snackBar);
        }

        private SnackBar Dequeue()
        {
            SnackBar item = null;
            if (_queue.Count > 0)
            {
                item = _queue[0];
                _queue.RemoveAt(0);
            }
            return item;
        }
    }
}