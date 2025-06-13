using System;
using Common.Pools;
using Modules.Gestures;
using TMPro;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal abstract class SnackBar : MonoBehaviour, IPoolable<SnackBar>
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private DragSwipeGestureComponent _dragSwipeGesture;
 
        private float? _time;
        private SnackBarState _state;
        private SnackBarToken _token;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _token = new SnackBarToken(RequestHide);

            if (!_dragSwipeGesture)
            {
                _dragSwipeGesture = GetComponentInChildren<DragSwipeGestureComponent>();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public event Action<SnackBar> HideRequested;
        
        public abstract SnackBarType Type { get; }
        public SnackBarState State => _state;
        public float? Time => _time;
        public SnackBarToken Token => _token;

        public virtual void Configure(SnackBarConfiguration configuration)
        {
            _time = configuration.Time;
            if (_title) _title.text = configuration.Title;

            _state = SnackBarState.Ready;
        }

        public virtual void OnAppearing()
        {
            _state = SnackBarState.Appearing;
        }

        public virtual void OnShown()
        {
            _state = SnackBarState.Shown;
            
            _dragSwipeGesture.Initialize();

            _dragSwipeGesture.SwipeUp += OnSwipeUp;
            _dragSwipeGesture.Tap += OnTap;
        }

        public virtual void OnDisappearing()
        {
            _state = SnackBarState.Disappearing;
        }

        public virtual void OnHidden()
        {
            _state = SnackBarState.Hidden;
            
            _dragSwipeGesture.CleanUp();

            _dragSwipeGesture.SwipeUp -= OnSwipeUp;
            _dragSwipeGesture.Tap -= OnTap;
        }

        //---------------------------------------------------------------------
        // IPoolable
        //---------------------------------------------------------------------
        public event Action<SnackBar> Used;

        public bool Visible
        {
            set => gameObject.SetActive(value);
        }

        public void MarkAsUsed()
        {
            Used?.Invoke(this);
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected void RequestHide()
        {
            HideRequested?.Invoke(this);
        }

        protected virtual void OnTap() { }
        protected virtual void OnSwipeUp() => RequestHide();
    }

    internal abstract class SnackBar<TConfiguration> : SnackBar where TConfiguration : SnackBarConfiguration
    {
        protected TConfiguration Configuration;
        
        public sealed override void Configure(SnackBarConfiguration configuration)
        {
            base.Configure(configuration);

            // Validate configuration
            if (configuration is TConfiguration validatedConfiguration)
            {
                Configuration = validatedConfiguration;
                OnConfigure(validatedConfiguration);
            }
            else
            {
                Debug.LogError($"Wrong configuration type {configuration.GetType()} for snack bar {GetType()}!");
            }
        }

        protected abstract void OnConfigure(TConfiguration configuration);
    }
}