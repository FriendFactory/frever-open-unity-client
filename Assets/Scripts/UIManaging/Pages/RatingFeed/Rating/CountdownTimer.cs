using System;
using System.Collections;
using Common;
using UnityEngine;

namespace UIManaging.Pages.RatingFeed.Rating
{
    public class CountdownTimer
    {
        private readonly float _durationInSeconds;
        
        private Coroutine _countdownCoroutine;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action TimeExpired;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public float CurrentTime { get; private set; }

        public bool IsRunning => _countdownCoroutine != null;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CountdownTimer(float durationInSeconds)
        {
            _durationInSeconds = durationInSeconds;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Start()
        {
            if (_countdownCoroutine != null)
            {
                CoroutineSource.Instance.StopCoroutine(_countdownCoroutine);
            }
            _countdownCoroutine = CoroutineSource.Instance.StartCoroutine(RunTimer());
        }

        public void Stop()
        {
            if (_countdownCoroutine == null) return;
            
            CoroutineSource.Instance.StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IEnumerator RunTimer()
        {
            CurrentTime = _durationInSeconds;
            while (CurrentTime > 0)
            {
                yield return null;
                CurrentTime -= Time.deltaTime;
            }
            TimeExpired?.Invoke();
        }
    }
}