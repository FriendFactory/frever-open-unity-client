//#define ENABLE_PREDICTION
using System;
using UnityEngine;

namespace ARKit
{
    public sealed class ARCoreCalibrationArray
    {
        private struct ValueRange
        {
            public float value;
            public float x;
            public float y;
            public bool measured;
        }

        private struct TrendInfo
        {
            public float rsquared;
            public float yintercept;
            public float slope;
        }
        private int _numArrayValues = 41;
        private ValueRange[,] _calibrationArray;
        private TrendInfo[] _trendArray;
        private float minFloat, maxFloat;

        private float _denominator;
        private bool _useInterpolation=true;
#if ENABLE_PREDICTION
        private float _trendslope = 0.0083f; // from my data analysis https://docs.google.com/spreadsheets/d/1gRJYEBlWgjv1EEFMaBqX3CZ6baokHM65f7Ws2dxfJ-s/edit#gid=921722046
#endif
        
        public ARCoreCalibrationArray(float min, float max)
        {
            _calibrationArray = new ValueRange[_numArrayValues,_numArrayValues];
            _trendArray = new TrendInfo[_numArrayValues];
            SetMappingRange(min, max);
        }
        
        public ARCoreCalibrationArray(float min, float max, int numValues)
        {
            _numArrayValues = numValues;
            _calibrationArray = new ValueRange[_numArrayValues,_numArrayValues];
            _trendArray = new TrendInfo[_numArrayValues];
            SetMappingRange(min, max);
        }      

        public int ArrayMaxValues { get { return _numArrayValues;}  }
        public void SetMappingRange(float min, float max)
        {
            minFloat = min;
            maxFloat = max;
            _denominator = maxFloat - minFloat;
        }

        public void UseIntepolation(bool use)
        {
            _useInterpolation = use;
        }

        public void PredictEmptyValues()
        {
            /*
             * Based on Offline data saw the Trend line slope was 0.0083 so this is a very naive attempt
             * but what could work is do a calibration run, then do a trend line for each y in a x bin and determine the slope
             * then fill in the data as below.  This seemed to be superior to interpolation between 2 measured values at times
             */
#if ENABLE_PREDICTION
            for (int x = 0; x < _numArrayValues; x++)
            {
                int ysearch = 0;
                //find a valid data point
                for (; ysearch < _numArrayValues; ysearch++)
                {
                    if (_calibrationArray[x, ysearch].measured)
                    {
                        break;
                    }
                }

                if (ysearch >= _numArrayValues)
                    continue;

                //from ysearch go bigger
                for (int y = ysearch + 1; y < _numArrayValues; y++)
                {
                    if (!_calibrationArray[x, y].measured) // we might have a new trend line so redo this
                    {
                        int prevy = y - 1;
                        float currentY = GetRangeFromIndex(y);
                        float newValue = _calibrationArray[x, prevy].value + (currentY - _calibrationArray[x, prevy].y) * _trendslope;
                        _calibrationArray[x, y].value = newValue;
                        _calibrationArray[x, y].y = currentY;
                        _calibrationArray[x, y].x = GetRangeFromIndex(x);
                    }
                }
                
                for (int y = ysearch - 1; y >= 0; y--)
                {
                    if (!_calibrationArray[x, y].measured)
                    {
                        int prevy = y + 1;
                        float currentY = GetRangeFromIndex(y);
                        float newValue = _calibrationArray[x, prevy].value + (currentY - _calibrationArray[x, prevy].y) * _trendslope;
                        _calibrationArray[x, y].value = newValue;
                        _calibrationArray[x, y].y = currentY;
                        _calibrationArray[x, y].x = GetRangeFromIndex(x);
                    }
                }
            } 
#endif
        }

        public void PrintAllDataToLog(string context)
        {
            Debug.Log(String.Format("public static float[] {0} = {{",context));
            for (int x = 0; x < _numArrayValues; x++)
            {
                for (int y = 0; y < _numArrayValues; y++)
                {
                    if(_calibrationArray[x, y].value != 0f && _calibrationArray[x, y].measured)
                        Debug.Log(String.Format("{0}f, {1}f, {2}f, // {3} float inputX, float inputY, float value",_calibrationArray[x, y].x, _calibrationArray[x, y].y, _calibrationArray[x, y].value, context));
                }
            }
            Debug.Log(String.Format("}}; // {0}",context));
        }

        public string GetDebugArrayHoles()
        {
            string debugText="";
            bool foundvalue;
            int numbermissed = 0;

                debugText += "holes: ";
                for (int x = 0; x < _numArrayValues; x++)
                {
                    foundvalue = false;
                    for (int y = 0; y < _numArrayValues; y++)
                    {
                        if (_calibrationArray[x, y].measured)
                        {
                            foundvalue = true;
                            numbermissed = 0;
                        }
                        else if (foundvalue)
                        {
                            debugText += String.Format("({0} {1}),", x,y);
                            numbermissed++;
                            if (numbermissed > 2)
                            {
                                foundvalue = false;
                            }
                        }
                    }
                }

            return debugText;
        }
        
        public string GetLocalDebugArrayHoles(float inputX,float inputY, int squareRadius = 5)
        {
            
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return "";
            
            int minX = indexX - squareRadius;
            if (minX < 0)
                minX = 0;
            int minY = indexY - squareRadius;
            if (minY < 0)
                minY = 0;
            int maxX = indexX + squareRadius;
            if (maxX > _calibrationArray.Length)
                maxX = _calibrationArray.Length;
            int maxY = indexY + squareRadius;
            if (maxY > _calibrationArray.Length)
                maxY = _calibrationArray.Length;
                
            string debugText="";

            debugText += "holes: ";
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (!_calibrationArray[x, y].measured)
                    {
                        debugText += String.Format("({0} {1}),", x, y);
                    }
                }
            }

            return debugText;
        }

        public void SetValue(float inputX, float inputY, float value, bool isMeasured=true)
        {
            if (inputX > maxFloat || inputX < minFloat || inputY > maxFloat || inputX < minFloat)
                return;
            
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            //  Debug.Log(String.Format("Cham2 inputX: {0}  inputY: {1} indexX: {2} indexY: {3} value: {4}", inputX, inputY, indexX, indexY, value));
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
              return;

            _calibrationArray[indexX, indexY].value = value;
            _calibrationArray[indexX, indexY].x = inputX;
            _calibrationArray[indexX, indexY].y = inputY;
            _calibrationArray[indexX, indexY].measured = isMeasured;
        }

        public float GetValue(float inputX, float inputY)
        {
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return 0f;
            float value = _calibrationArray[indexX, indexY].value;
            return value;
        }
        
        public float GetYthisIndex(float inputX, float inputY)
        {
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return 0f;
            
            return _calibrationArray[indexX, indexY].y;
        }
        
        public bool IsMeasured(float inputX, float inputY)
        {
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return false;
            
            return _calibrationArray[indexX, indexY].measured;
        }
        public float GetValueInterpolation(float inputX, float inputY, float inputValue, bool writeToCalibration, bool isMax=false)
        {
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return 0f;

            float value = _calibrationArray[indexX, indexY].value;
            float origArrayValue = value;
            float x2 = _calibrationArray[indexX, indexY].y;
            bool isMeasured = _calibrationArray[indexX, indexY].measured;
            float prevValue = 0.0f;
            float prevY = 0.0f;
            
            //TODO what should we do if we get a 0 value from _calibrationArray?
            // what happens if we store this value and let the values fill in?  the problem is
            // if the eyes were open then it is likely that
            // our PredictEmptyValues trend line is useless
            // so do we need a stored data trend line that we reference rather than the on the fly PredictEmptyValues?
            //

            if (_useInterpolation)
            {
                // the data indicated that y is the more important input
                if (inputY < x2)
                {
                    int prevYIndex = indexY - 1;
                    if (prevYIndex >= 0)
                    {
                        prevValue = _calibrationArray[indexX, prevYIndex].value;
                        prevY = _calibrationArray[indexX, prevYIndex].y;
                    }
                }
                else if (inputY > x2)
                {
                    int nextYIndex = indexY + 1;
                    if (nextYIndex < _calibrationArray.Length)
                    {
                        prevValue = _calibrationArray[indexX, nextYIndex].value;
                        prevY = _calibrationArray[indexX, nextYIndex].y;
                    }
                }

                if (prevY != 0.0f) //specifically not using measured because if we did populate values then we want to use that value
                    value = prevValue + (inputY - prevY) * ((value - prevValue) / (x2 - prevY));
            }

            if (writeToCalibration)
            {
                if (isMax)
                {
                    
                    //todo where do we fix bad prediction?
                    if (inputValue > origArrayValue ||
                        (!isMeasured && origArrayValue == 0.0f)) //(!isMeasured && withInRange) ||
                    {
                        _calibrationArray[indexX, indexY].value = inputValue;
                        _calibrationArray[indexX, indexY].x = inputX;
                        _calibrationArray[indexX, indexY].y = inputY;
                        _calibrationArray[indexX, indexY].measured = true;
                        value = inputValue;
                        PredictEmptyValues();
                    }
                }
                else
                {
                    if (inputValue < origArrayValue ||
                        (!isMeasured && origArrayValue == 0.0f)) //|| (!isMeasured && withInRange)
                    {
                        _calibrationArray[indexX, indexY].value = inputValue;
                        _calibrationArray[indexX, indexY].x = inputX;
                        _calibrationArray[indexX, indexY].y = inputY;
                        _calibrationArray[indexX, indexY].measured = true;
                        value = inputValue;
                        PredictEmptyValues();
                    }
                }
            }

            return value;
        }
        
        public string GetDebugText(string context="")
        {
            string text = context;
            for (int y = 0; y < _numArrayValues; y++)
            {
                float iY = GetRangeFromIndex(y);
                for (int x = 0; x < _numArrayValues; x++)
                {
                    float value = _calibrationArray[x, y].value;
                    if (_calibrationArray[x, y].measured)
                    {
                        float iX = GetRangeFromIndex(x);
                        text = text + String.Format("[({0},{1}){2:0.0},{3:0.0}]: {4:0.00000} | ", x, y, iX, iY, value);
                    }
                }
                text = text + "\n";
            }
            return text;
        }

        public void PrintDebug(string context="")
        {
            context = GetDebugText(context);
            Debug.Log(context);
        }
        
        public int GetIndex(float input)
        {
            float index = ((input - minFloat) / _denominator) * (_numArrayValues-1);
            return Mathf.FloorToInt(index);
        }
        
        private float GetRangeFromIndex(int index)
        {
            float i = index;
            float v = _numArrayValues-1;
            return minFloat + (_denominator * (i/v));
        }
        
        public float GetLinearRegressionValue(float inputX, float inputY)
        {
            int indexX = GetIndex(inputX);
            int indexY = GetIndex(inputY);
            if (indexX > _calibrationArray.Length || indexX < 0 || indexY > _calibrationArray.Length || indexY < 0)
                return 0f;

            float value = _trendArray[indexX].yintercept + (_trendArray[indexX].slope * inputY);
            return value;
        }

        public float[] CalculateTrendData(bool printLog=false, string context="")
        {
            float[] Xarray = new float[_numArrayValues];
            float[] Yarray = new float[_numArrayValues];
            float[] slopes = new float[_numArrayValues];          
            int indexReal = 0;
            
            for (int x = 0; x < _numArrayValues; x++)
            {
                indexReal = 0;
                for (int y = 0; y < _numArrayValues; y++)
                {
                    if (_calibrationArray[x, y].value > 0.0)
                    {
                        Xarray[indexReal] = _calibrationArray[x, y].y;
                        Yarray[indexReal] = _calibrationArray[x, y].value;
                        indexReal++;
                    }
                }

                if (indexReal > 0)
                {
                    // find out the slope of this madness
                    LinearRegression(Xarray, Yarray, 0, indexReal, out _trendArray[x].rsquared,
                                     out _trendArray[x].yintercept,
                                     out _trendArray[x].slope);

                    slopes[x] = _trendArray[x].slope;
                }
            }

            if (printLog)
            {
                Debug.Log("TrendData " + context);
                for (int x = 0; x < _numArrayValues; x++)
                {
                    Debug.Log(String.Format("_trendArray[{0}].slope = {1}; ", x, _trendArray[x].slope));
                    Debug.Log(String.Format("_trendArray[{0}].yintercept = {1}; ", x, _trendArray[x].yintercept));
                }
            }

            return slopes;
        }
        
        /*
         *
        double rsquared;
        double yintercept;
        double slope;
        LinearRegression(xVal, yVal,0,9, out rsquared, out yintercept, out slope);
        Console.WriteLine( yintercept + (slope*15));//15 is xvalue of future(no of day from 1)
         */
        public static void LinearRegression(float[] xVals, float[] yVals,
                                            int inclusiveStart, int exclusiveEnd,
                                            out float rsquared, out float yintercept,
                                            out float slope)
        {
            Debug.Assert(xVals.Length == yVals.Length);
            float sumOfX = 0;
            float sumOfY = 0;
            float sumOfXSq = 0;
            float sumOfYSq = 0;
            float ssX = 0;
            float ssY = 0;
            float sumCodeviates = 0;
            float sCo = 0;
            float count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                float x = xVals[ctr];
                float y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            float RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            float RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
                         * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            float meanX = sumOfX / count;
            float meanY = sumOfY / count;
            float dblR = RNumerator / Mathf.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

        public void ResetCalibration()
        {
            for (int x = 0; x < _numArrayValues; x++)
            {
                for (int y = 0; y < _numArrayValues; y++)
                {
                    _calibrationArray[x, y].value = 0f;
                    _calibrationArray[x, y].x = 0f;
                    _calibrationArray[x, y].y = 0f;
                    _calibrationArray[x, y].measured = false;
                  
                }
            }
        }
    }
    
    
}
