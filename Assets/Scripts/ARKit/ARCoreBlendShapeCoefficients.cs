using System;
using System.Collections.Generic;
#if UNITY_IOS
using SwiftPlugin.Source;
#endif
using UnityEngine;

namespace ARKit
{
    public sealed class ARCoreBlendShapeCoefficients
    {
        private const int NUM_COEFFICIENTS = 39; // from the swift file
        public const float
            MIN_PLAYER_TO_CAMERA_COSINE =
                0.94551857f; // 19 degrees 0.94551857f // 17 degrees 0.95630476f   // 18 degrees 0.95105652f // 20 degrees 0.93969262f // 15 degrees 0.978148f; 
        
        private float[] _blendshapes = new float[NUM_COEFFICIENTS];
        private bool canTrackFace = false;
        private float _centerTransformZAxisCosine = 1.0f;
        public ARCoreBlendShapeMouth ArCoreBlendShapeMouth = new ARCoreBlendShapeMouth();
        public ARCoreBlendShapeEyes ArCoreBlendShapeEyes = new ARCoreBlendShapeEyes();
        private MovingAverage _playerfacingYvalues = new MovingAverage(60);
        
        //DEBUGGING
        private bool _isDebuggerEnabled = false;
        private bool _isDebuggerEnabledMouth = false;
        private bool _isDebuggerEnabledEyes = false;
        private string _debugText = "";
        private bool _doDataCollection = false;
        private Dictionary<int, float> _debugDict = new Dictionary<int, float>();
        private bool _zeroOutAllExceptOverrides = false;
        public string DebugText
        {
            get { return _debugText; }
        }
        
        public bool EnableDebugTextMouth
        {
            get { return _isDebuggerEnabledMouth; }
            set
            {
                _isDebuggerEnabledMouth = value;
                ArCoreBlendShapeMouth.EnableDebugText = _isDebuggerEnabledMouth;
                _isDebuggerEnabled = _isDebuggerEnabledMouth || _isDebuggerEnabledEyes;
            }
        }
        
        public bool EnableDebugTextEyes
        {
            get { return _isDebuggerEnabledEyes; }
            set
            {
                _isDebuggerEnabledEyes = value;
                ArCoreBlendShapeEyes.EnableDebugText = _isDebuggerEnabledEyes;
                _isDebuggerEnabled = _isDebuggerEnabledMouth || _isDebuggerEnabledEyes;
            }
        }
        
        //Events
        public event Action RequestPlayerCenterFaceStarted;
        public event Action RequestPlayerCenterFaceFinished;
        
        public float[] CalculateBlendShapeCoefficients(Vector3[] vertices, Vector3 faceForward,Vector3 facePos, Vector3  cameraForward, Vector3  cameraPos, bool lastFrameCapturedVerts)
        {
            if (_isDebuggerEnabled)
                _debugText = "";


            _centerTransformZAxisCosine = faceForward.z; 
#if UNITY_IOS
            // NOTE: There is some Gyroscope stuff in the Swift code that might be fighting us giving us weird swings in data
            // we mostly care about enterTransform4X4[2].z because enterTransform4X4[2] is the z axis which is our player nose pointing
            // at our camera and the z component (enterTransform4X4[2].z) is the cosine of the angle off from center facing we are
            // so 1.0 is full confidence and then less so from there. so we can use it to dial in the gain that we use
            if (_isDebuggerEnabled)
            {
                Vector4[] centerTransform4X4 = SwiftForUnity.centerTransform4X4;
                _debugText = _debugText +
                             String.Format("lastGyroCalulatedRotation : {0}\n", SwiftForUnity.lastGyroCalulatedRotation);
                _debugText = _debugText +
                            String.Format("centerTransform[0] X: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}\n",
                                          centerTransform4X4[0].x, centerTransform4X4[0].y, centerTransform4X4[0].z,
                                          centerTransform4X4[0].w);
                _debugText = _debugText +
                            String.Format("centerTransform[1] Y: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}\n",
                                          centerTransform4X4[1].x, centerTransform4X4[1].y, centerTransform4X4[1].z,
                                          centerTransform4X4[1].w);
                _debugText = _debugText +
                            String.Format("centerTransform[2] Z: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}\n",
                                          centerTransform4X4[2].x, centerTransform4X4[2].y, centerTransform4X4[2].z,
                                          centerTransform4X4[2].w);
                _debugText = _debugText +
                            String.Format("centerTransform[3] P: {0:0.0000}, {1:0.0000}, {2:0.0000}, {3:0.0000}\n",
                                          centerTransform4X4[3].x, centerTransform4X4[3].y, centerTransform4X4[3].z,
                                          centerTransform4X4[3].w);
            }
#endif

            if (lastFrameCapturedVerts)
            {
                if (_centerTransformZAxisCosine > MIN_PLAYER_TO_CAMERA_COSINE)
                {
                    if (canTrackFace == false)
                    {
                        canTrackFace = true;
                        RequestPlayerCenterFaceFinished?.Invoke();
                    }
                    
                    // clear the blendshapes value from previous frame
                    for (int i = 0; i < NUM_COEFFICIENTS; i++)
                    {
                        _blendshapes[i] = 0.0f;
                    }
                    
                    // what is our forwardVector.y component doing.  I have seen that I will get different results
                    // if the forwardVector.y has an amplitude peak to peak of 0.01 or 0.04
                    // trying to determine if this is a hardware Android limitation or a ARCore lighting difficiency 
                    // This may show when not plugged into the computer/power and/or the phone is left on its back for a while
                    //  Maybe Gyro is doing something wierd in power save mode or something
                    // but want to detect the situation so that we could compensate when we see it.
                    _playerfacingYvalues.AddValue(faceForward.y);
                   // float peakToPeakY = _playerfacingYvalues.PeakToPeakMax;
                    
                    // order of calls matters.  Call Mouth then Eyes
                    _blendshapes = ArCoreBlendShapeMouth.CalculateMouthBlendShapeCoefficients(vertices, _blendshapes, faceForward, facePos, cameraForward, cameraPos);
                    if (_isDebuggerEnabledMouth)
                        _debugText = _debugText + ArCoreBlendShapeMouth.DebugText;
                    
                    _blendshapes = ArCoreBlendShapeEyes.CalculateEyeBlendShapeCoefficients(vertices, _blendshapes, faceForward, facePos, cameraForward, cameraPos);
                    if (_isDebuggerEnabledEyes)
                        _debugText = _debugText + ArCoreBlendShapeEyes.DebugText;
                    
                    // try rounding and see if it helps with noise
                    for (int i = 0; i < NUM_COEFFICIENTS; i++)
                    {
                        float rounded =
                            Mathf.Round(_blendshapes[i] * 100f) / 100f; 
                        _blendshapes[i] = Mathf.Clamp(rounded, 0, 100);
                    }
                }
                else
                {
                    if (canTrackFace)
                    {
                        canTrackFace = false;
                        RequestPlayerCenterFaceStarted?.Invoke();
                    }

                    _blendshapes = GetOffScreenFace(_blendshapes);
                    if (_isDebuggerEnabled)
                    {
                        _debugText = _debugText +
                                    String.Format(
                                        "\n PLEASE CENTER THE CAMERA ON YOUR FACE, AND HOLD THE PHONE STEADY\n");
                    }
                }
            }
            else
            {
                ResetToStartingValues(); // maybe this is because we have a new user so we need to reset
                _blendshapes = GetOffScreenFace(_blendshapes);
                if (canTrackFace)
                {
                    canTrackFace = false;
                    RequestPlayerCenterFaceStarted?.Invoke();
                }

                if (_isDebuggerEnabled)
                {
                    _debugText = _debugText +
                                String.Format(
                                    "\n ARCORE did not capture a face for us. Will see about error codes in future\n");
                }
            }

            if (_isDebuggerEnabled)
            {
                if (_debugDict.Count > 0)
                {
                    _debugText = _debugText + "OVERRIDES:";

                    if (_zeroOutAllExceptOverrides)
                    {
                        for (int i = 0; i < NUM_COEFFICIENTS; i++)
                        {
                            _blendshapes[i] = 0.0f;
                        }
                        _debugText = _debugText + "(ZERO) ";
                    }

                    foreach (KeyValuePair<int, float> entry in _debugDict)
                    {
                        if (entry.Key >= 0 && entry.Key < NUM_COEFFICIENTS)
                        {
                            _blendshapes[entry.Key] = entry.Value;
                            _debugText = _debugText + String.Format(" {0}: {1:000.000} ,", entry.Key, entry.Value);
                        }
                    }
                    if(_debugDict.Count>0)
                        _debugText = _debugText + "\n";
                    
                }
            }

            return _blendshapes;
        }

        public void OnEnable()
        {
            ResetToStartingValues();
        }
        public void OnDisable()
        {
            canTrackFace = false;
            RequestPlayerCenterFaceFinished?.Invoke();
        }
        
        public float[] GetNeutralScreenFace()
        {
            for (int i = 0; i < NUM_COEFFICIENTS; i++)
            {
                _blendshapes[i] = 0.0f;
            }

            // Close the eyes
            _blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_LEFT] = 0.0f;  
            _blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT] = 0.0f;
            _blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_LEFT] = 50.0f;  
            _blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_RIGHT] = 50.0f;
            return _blendshapes;
        }
        public float[] GetOffScreenFace(float[] blendshapes)
        {
            for (int i = 0; i < NUM_COEFFICIENTS; i++)
            {
                blendshapes[i] = 0.0f;
            }

            // Close the eyes
            blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_LEFT] = 100.0f;  
            blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT] = 100.0f; 
            return blendshapes;
        }
        
        public void ResetToStartingValues()
        {
            ArCoreBlendShapeMouth.ResetToStartingValues();
        }
        
        public void SetOverrideValue(string index, string value)
        {
            int i = int.Parse(index);
            if (i >= 0 && i < NUM_COEFFICIENTS)
            {
                float v = float.Parse(value);
                if (v >= 0.0f)
                {
                    _debugDict[i] = v;
                }
                else // if value is negative then lets remove/clear the override
                {
                    _debugDict.Remove(i);
                }
            }
        }

        public void ToggleDataCollection()
        {
            _doDataCollection = !_doDataCollection;
            ArCoreBlendShapeMouth.ToggleDataCollection();
            ArCoreBlendShapeEyes.ToggleDataCollection();
            if (_doDataCollection)
            {
                Debug.Log("DataCollection begin\n");
            }
            else
            {
                Debug.Log("DataCollection end\n");
            }
        }

        public void ToggleZeroAllExceptOverrides()
        {
            _zeroOutAllExceptOverrides = !_zeroOutAllExceptOverrides;
        }
        
    
    }
}
