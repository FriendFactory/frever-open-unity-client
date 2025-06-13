using System;
using System.Collections.Generic;
using Extensions;
using Laphed.Rx;
using UnityEngine;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking
{
    [RequireComponent(typeof(Animator))]
    public sealed partial class AnimatorMonitor : MonoBehaviour
    {
        private readonly HashSet<AnimatorTimeTrigger> _timeTriggers = new();
        
        private Animator _animator;
        private long _currentMonitoringAnimationId;
        private int _currentCycle;

        public long? AnimationId => _currentMonitoringAnimationId;
        public bool IsTracking { get; private set; }
        public float CurrentLength { get; private set; }
        public float CurrentTime { get; private set; }
        
        private ReactiveProperty<int> CurrentCycle { get; } = new();

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void StartMonitoring(long id, float startTime, float animationLength)
        {
            if (IsTracking)
            {
                StopMonitoring();
            }

            if (_currentMonitoringAnimationId != id)
            {
                RemoveTimeTriggers(_currentMonitoringAnimationId);
            }
            
            _currentMonitoringAnimationId = id;
            CurrentCycle.Value = 0;
            CurrentLength = animationLength;
            CurrentTime = startTime % CurrentLength;

            IsTracking = true;
            
            CurrentCycle.OnChanged += OnNewCycle;
        }

        public void StopMonitoring()
        {
            if (!IsTracking) return;

            IsTracking = false;
            
            CurrentCycle.OnChanged -= OnNewCycle;
        }

        public void AddTimeTrigger(long animationId, float triggerTime, Action onTrigger)
        {
            var trigger = new AnimatorTimeTrigger(animationId, triggerTime, onTrigger);
            
            trigger.IsTriggered = CurrentTime % CurrentLength >= trigger.TriggerTime;

            if (trigger.IsTriggered)
            {
                trigger.OnTrigger?.Invoke();
            }

            _timeTriggers.Add(trigger);
        }
        
        public void RemoveTimeTriggers(long animationId)
        {
            _timeTriggers.RemoveWhere(trigger => trigger.AnimationId == animationId);
        }

        private void LateUpdate()
        {
            if (!_animator || !IsTracking) return;

            UpdateTimeInfo();

            // Check triggers
            foreach (var trigger in _timeTriggers)
            {
                if (!trigger.IsTriggered && trigger.AnimationId == _currentMonitoringAnimationId && CurrentTime >= trigger.TriggerTime)
                {
                    trigger.IsTriggered = true;
                    trigger.OnTrigger?.Invoke();
                }
            }
        }

        private void OnNewCycle()
        {
            _timeTriggers.ForEach(trigger => trigger.IsTriggered = false);
        }

        private void UpdateTimeInfo()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // Assuming layer 0
            
            CurrentTime = stateInfo.length * (stateInfo.normalizedTime % 1f);
            CurrentLength = stateInfo.length;
            CurrentCycle.Value = (int)stateInfo.normalizedTime;
        }
    }
}