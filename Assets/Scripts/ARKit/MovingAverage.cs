using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARKit
{
    public sealed class MovingAverage
    {
        private int _maxWindow=5;
        private List<float> _averageList = new List<float>();
        private float _average;
        private float _minLifetime = float.MaxValue;
        private float _maxLifetime = float.MinValue;
        private float _startingAverage = 0f;
        
        // Find min peak
        // the slope was negative and now we get a positive
        private float _lastSlope;
        private float _lastMagnitude;
        private float _minPeak;
        private float _maxPeak;
        
        private bool _isDebuggerEnabled = false;
        private string _debugContext;
        
        public bool EnableDebugText
        {
            get { return _isDebuggerEnabled; }
            set { _isDebuggerEnabled = value; }
        }        
        
        public string DebugContext
        {
            get { return _debugContext; }
            set { _debugContext = value; }
        }
        
        public float LastMagnitude
        {
            get { return _lastMagnitude; }
            set { _lastMagnitude =  value; }
        }   
        public int MaxWindow
        {
            get { return _maxWindow; }
            set
            {
                _maxWindow =  value; 
                _averageList.Capacity = _maxWindow; // Ok need to make sure we do the right thing here with memory
            }  
        }

        public int Count
        {
            get { return _averageList.Count; }
        }

        public float PeakToPeakMax
        {
            get { return Mathf.Abs(_maxPeak - _minPeak); }
        }

        public float Min
        {
            get { return _minPeak; }
        }
        

        public float Average
        {
            get { return _average; }
        }

        public MovingAverage()
        {
            _averageList.Capacity = _maxWindow;
        }
        
        public MovingAverage(int maxWindow)
        {
            _maxWindow = maxWindow;
            _averageList.Capacity = _maxWindow;
        }
        
        public MovingAverage(int maxWindow, float startingAverage)
        {
            _maxWindow = maxWindow;
            _averageList.Capacity = _maxWindow;
            _average = LastMagnitude = _startingAverage = startingAverage;
        }
        
        public void ResetAverage(float val)
        {
            ResetAverage();
            AddValue(val);
        }
        
        public void ResetAverage()
        {
            _average = LastMagnitude = _startingAverage;
            _lastSlope = 0f;  // important to zero this out or we may accidentally add a zero item to the array in our condition check
            _minLifetime = float.MaxValue;
            _maxLifetime = float.MinValue;
            _averageList.Clear();
        }
        
        public float AddIfMinPeakLastFramePercentMinLifeTime(float magnitude, float percent)
        {
            if (magnitude < _minLifetime)
            {
                _minLifetime = magnitude;
                
                if (_averageList.Count == 0)
                {
                    _average = _minLifetime;
                }
            }

            float threshold = _minLifetime + (_minLifetime * percent);
            return AddIfMinPeakLastFrame(magnitude, threshold);
        }
        
        public float AddIfMinPeakLastFrame(float magnitude, float threshold)
        {
            // Find min peak
            // the slope was negative and now we get a positive
            bool wasDownwardTrend = _lastSlope < 0f;
            bool isGrowing = _lastMagnitude < magnitude;
            bool addToMinAve = (wasDownwardTrend && isGrowing && (magnitude < threshold) &&
                                (magnitude > 0f));
            
            if (magnitude < _minLifetime)
            {
                _minLifetime = magnitude;
            }
            
            if (addToMinAve)
            {
                float previousAverage = _average;
                AddValue(_lastMagnitude); //Last value was a min peak
                if (_isDebuggerEnabled)
                {
                    Debug.Log(String.Format(
                                  "Min Peak {0}: average was {1:00.0000}, input {2:00.0000}, new {3:00.0000}, _lastSlope {4:00.0000}, _lastJawOpenMagnitude {5: 00.0000} ",
                                  _debugContext, previousAverage, magnitude, _average, _lastSlope, _lastMagnitude));
                }
            }
            
            //store off magnitude for next input
            _lastSlope = magnitude - _lastMagnitude;
            _lastMagnitude = magnitude;
            
            return _average;
        }
        
        public float AddIfMaxPeakLastFrame(float magnitude, float threshold)
        {
            // Find min peak
            // the slope was negative and now we get a positive
            bool wasUpwardTrend = _lastSlope > 0f;
            bool isShrinking = _lastMagnitude > magnitude;
            bool addToMinAve = (wasUpwardTrend && isShrinking && (magnitude > threshold) &&
                                (magnitude > 0f));
 
            if (magnitude > _maxLifetime)
            {
                _maxLifetime = magnitude;
            }

            if (addToMinAve)
            {
                float previousAverage = _average;
                AddValue(_lastMagnitude); //Last value was a min peak
                if (_isDebuggerEnabled)
                {
                    Debug.Log(String.Format(
                                  "MaxPeak {0}: average was {1:00.0000}, input {2:00.0000}, new {3:00.0000}, _lastSlope {4:00.0000}, _lastJawOpenMagnitude {5: 00.0000} ",
                                  _debugContext, previousAverage, magnitude, _average, _lastSlope, _lastMagnitude));
                }
            }

            //store off magnitude for next input
            _lastSlope = magnitude - _lastMagnitude;
            _lastMagnitude = magnitude;
            
            return _average;
        }

        public bool isWindowFull()
        {
            return _averageList.Count >= _maxWindow;
        }

        public float AddValue(float val)
        {
            if (val < _minLifetime)
            {
                _minLifetime = val;
            }
            
            if (val > _maxLifetime)
            {
                _maxLifetime = val;
            }
            
            _averageList.Add(val);
            while (_averageList.Count > _maxWindow)
            {
                _averageList.RemoveAt(0);
            }

            float sum = _averageList.Sum();
            _minPeak = _averageList.Min();
            _maxPeak = _averageList.Max();
            _average = sum / _averageList.Count;
            return _average;
        }

        public float GetTrendSlope()
        {
            float[] Xarray = Enumerable.Range(0, _maxWindow).ToArray().Select(x => (float)x).ToArray();
            float[] Yarray = _averageList.ToArray();
            float rsquared;
            float yintercept;
            float slope = 1;

            if (_averageList.Count > 1)
            {
                ARCoreCalibrationArray.LinearRegression(Xarray, Yarray, 0, _averageList.Count, out rsquared, out yintercept, out slope);
            }
            
            return slope;
        }
    }
}
