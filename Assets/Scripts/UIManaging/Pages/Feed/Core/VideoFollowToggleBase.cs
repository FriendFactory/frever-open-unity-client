using System.Threading;
using Bridge;
using DG.Tweening;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    public abstract class VideoFollowToggleBase : MonoBehaviour
    {
        [SerializeField] protected Toggle _followToggle;
        [SerializeField] protected CanvasGroup _notFollowingCanvasGroup;

        protected long VideoGroupId;
        protected Sequence ToggleSequence;
        protected readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            _followToggle.onValueChanged.AddListener(OnFollowToggleValueChanged);
        }

        protected virtual void OnDisable()
        {
            _followToggle.onValueChanged.RemoveListener(OnFollowToggleValueChanged);
        }

        protected virtual void OnDestroy()
        {
            ToggleSequence.Kill();
            TokenSource.Cancel();
            TokenSource.Dispose();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public virtual void Init(long videoGroupId)
        {
            VideoGroupId = videoGroupId;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected abstract void OnFollowToggleValueChanged(bool value);

        protected void ToggleTween()
        {
            ToggleSequence?.Kill();
            ToggleSequence = DOTween.Sequence();
            _followToggle.interactable = false;
            
            ToggleSequence.Append(_notFollowingCanvasGroup.DOFade(0f, 0.1f)).OnComplete(_notFollowingCanvasGroup.Disable);
        }
    }
}