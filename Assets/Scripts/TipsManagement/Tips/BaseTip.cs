using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Extensions;
using TipsManagment.Args;

namespace TipsManagment
{
    public abstract class BaseTip : MonoBehaviour
    {
        public event Action<BaseTip> TipDone;
        public event Action<BaseTip> TipIgnored;

        public TipId Id => Args.Id;
        public TipType CurrentType => Args.Type;
        public int PromptAgain => Args.PromptAgain;

        protected TipArgs Args;
        protected RectTransform TargetTransform;
        protected RectTransform RTransform;
        protected TaskCompletionSource<bool> TaskSource;
        protected bool IsShown;
        protected string TargetName;
        protected bool IsDestroyed;

        private Rect _screenBounds;
        private bool _listeningPageDestroyEvent;

        protected virtual void Update()
        {
            CheckClickPosition();
            if (IsShown)
            {
                PositionateTip();
            }
        }

        private void OnDestroy()
        {
            Stop();

            if (_listeningPageDestroyEvent)
            {
                UnsubscribeFromPageDestroyingDestroy();
            }
        }

        public virtual void Init(TipArgs args)
        {
            Args = args;
            RTransform = transform as RectTransform;
            TargetTransform = args.Target?.TargetTransform;
            SubscribeToPageDestroyingEvent();
        }

        public virtual async Task Activate()
        {
            if (TargetTransform) return;
            
             await FindTargetTransform();
        }

        public virtual async void Show()
        {
            if (IsDestroyed) return;

            if ((TargetTransform != null && !TargetTransform.gameObject.activeInHierarchy))
            {
                StartTip();
                return;
            }

            IsShown = true;
            gameObject.SetActive(true);
            PositionateTip();

            var taskList = new List<Task>();
            TaskSource = new TaskCompletionSource<bool>();
            taskList.Add(TaskSource.Task);
            if (Args.Duration > 0)
            {
                taskList.Add(Task.Delay(Args.Duration));
            }

            await Task.WhenAny(taskList.ToArray());
            Hide();
        }

        public virtual void Hide()
        {
            if (IsDestroyed) return;
            IsShown = false;
            if (CheckTipDone())
            {
                RaiseTipDone();
            }
            else
            {
                RaiseTipIgnored();
            }
            gameObject.SetActive(false);
        }

        protected virtual async Task FindTargetTransform()
        {
            var lastSplitterIndex = Args.TargetElementName.LastIndexOf('/');
            Transform parentTransform;

            if (lastSplitterIndex > 0)
            {
                var parentName = Args.TargetElementName.Substring(0, lastSplitterIndex);
                parentTransform = await AwaitParentTransform(parentName);
                if (IsDestroyed) return;
            }
            else
            {
                parentTransform = Args.PageTransform;
            }

            TargetName = Args.TargetElementName.Substring(lastSplitterIndex + 1);
            TargetTransform = await AwaitTargetTransform(TargetName, parentTransform) as RectTransform;
        }

        protected virtual bool CheckTipDone()
        {
            if (TaskSource == null) return true;
            if (TaskSource.Task.IsCompleted && TaskSource.Task.Status == TaskStatus.RanToCompletion)
                return TaskSource.Task.Result;
            return false;
        }

        protected async void StartTip()
        {
            await Task.Delay(Args.TriggerWaitTime);
            await AwaitTargetTransformVisible();

            if (IsDestroyed) return;
            Show();
        }
        
        protected abstract void PositionateTip();

        protected void RaiseTipDone()
        {
            TipDone?.Invoke(this);
        }
        
        protected void RaiseTipIgnored()
        {
            TipIgnored?.Invoke(this);
        }

        protected virtual void CheckClickPosition()
        {
#if UNITY_EDITOR
            var pos = Input.mousePosition;
            if (!Input.GetMouseButton(0)) return;
#else
            if (Input.touchCount == 0) return;
            var pos = Input.GetTouch(0).position;
#endif

            if (!ClickOnTarget(pos))
            {
                TaskSource.SetCanceled();
            }
        }

        protected async Task AwaitTargetTransformVisible()
        {
            while (!TargetOnScreen())
            {
                if (IsDestroyed) break;
                await Task.Delay(5);
            }
        }

        protected async Task<Transform> AwaitParentTransform(string parentName)
        {
            Transform parentTransform = null;
            if (Args.PageTransform != null)
            {
                parentTransform = Args.PageTransform.Find(parentName);
            }
            while (parentTransform is null)
            {
                if (IsDestroyed) break;
                await Task.Delay(1000);
                if (Args.PageTransform == null) return null;
                parentTransform = Args.PageTransform.Find(parentName);
            }

            return parentTransform;
        }

        protected async Task<Transform> AwaitTargetTransform(string targetName, Transform parentTransform)
        {
            if (parentTransform is null)
            {
                throw new Exception($"{Args.TargetElementName} can't be found! Parent is NULL");
            }

            var targetElements = FindTargetsInParent(targetName, parentTransform);

            while (targetElements.Count == 0 || targetElements.Count < Args.ItemIndex)
            {
                await Task.Delay(1000);
                if (parentTransform == null) return null;
                targetElements = FindTargetsInParent(targetName, parentTransform);
            }
            var elementAtIndex = targetElements[Args.ItemIndex];
            TargetTransform = elementAtIndex as RectTransform;

            return TargetTransform;
        }

        protected List<Transform> FindTargetsInParent(string targetName, Transform parrentTransform)
        {
            var targetElements = new List<Transform>();

            if (parrentTransform == null) return targetElements;

            foreach (Transform childTransform in parrentTransform)
            {
                if (childTransform.name == targetName)
                {
                    targetElements.Add(childTransform);
                }
            }
            return targetElements;
        }

        protected virtual bool TargetOnScreen()
        {
            if (IsDestroyed || TargetTransform == null || !TargetTransform.gameObject.activeInHierarchy) return false;
            _screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
            var objectCorners = new Vector3[4];
            TargetTransform.GetWorldCorners(objectCorners);

            var visibleCorners = 0;
            for (var i = 0; i < objectCorners.Length; i++)
            {
                if (BoundsContains(objectCorners[i]))
                {
                    visibleCorners++;
                }
            }
            return visibleCorners > 0;
        }

        protected virtual bool ClickOnTarget(Vector2 clickPosition)
        {
            Vector3[] corners = new Vector3[4];
            TargetTransform.GetWorldCorners(corners);
            var topLeft = corners[0];
            var scaledSize = new Vector2(TargetTransform.rect.size.x, TargetTransform.rect.size.y);

            var transformRect = new Rect(topLeft, scaledSize);
            return transformRect.Contains(clickPosition);
        }

        private void OnPageUnloaded()
        {
            _listeningPageDestroyEvent = false;
            Stop();
        }
        
        private void Stop()
        {
            TaskSource?.TrySetCanceled();
            IsDestroyed = true;
        }

        private void SubscribeToPageDestroyingEvent()
        {
            _listeningPageDestroyEvent = true;
            Args.PageTransform.AddListenerToDestroyEvent(OnPageUnloaded);
        }

        private void UnsubscribeFromPageDestroyingDestroy()
        {
            _listeningPageDestroyEvent = false;
            Args.PageTransform.RemoveListenerFromDestroyEvent(OnPageUnloaded);
        }

        private bool BoundsContains(Vector3 point)
        {
            // Hot fix for full screen hints not shown on some resolution
            if (_screenBounds.Contains(point)) return true;

            if ((point.x - _screenBounds.x) < 0) return false;
            if ((point.y - _screenBounds.y) < 0) return false;
            if ((point.x - _screenBounds.xMax) > 0) return false;
            if ((point.y - _screenBounds.yMax) > 0) return false;

            return true;
        }

#if UNITY_EDITOR
        [ContextMenu("Check path")]
        protected void CheckPath()
        {
            var path = Args.TargetElementName;
            var lastFoundPath = path;
            var done = false;
            var pageTransform = Args.PageTransform;
            while (!done)
            {
                var foundTransform = pageTransform.Find(lastFoundPath);
                if (foundTransform != null)
                {
                    done = true;
                }
                else
                {
                    var lastDelimiterIndex = lastFoundPath.LastIndexOf("/");
                    lastFoundPath = lastFoundPath.Substring(0, lastDelimiterIndex);
                }
            }
        }
#endif
    }
}
