#define USE_TOP_LID_OVERRIDE
using System;
using System.Text;
using LowPassFilter;
using UnityEngine;

namespace ARKit
{
    public sealed class ARCoreBlendShapeEyes
    {
        private const float COEFFICIENT_SCALE = 100.0f;
        private const float EYE_DEFAULT_OPEN_PERCENT = 0.8f;
        
#if UNITY_IOS        
        public float EyeHeightDown = 0.0009f; //scale these //https://docs.google.com/spreadsheets/d/1FVllbI2F5c8TxTATheNUZuqDhwEhYd_eS6BcnSsl9TU/edit#gid=1515991151
        public float EyeHeightUp = 0.0003f; 
#else
        public float EyeHeightDown = 0.00035f; // was 9 this is better => 0.0006f;  //scale these //https://docs.google.com/spreadsheets/d/1FVllbI2F5c8TxTATheNUZuqDhwEhYd_eS6BcnSsl9TU/edit#gid=1515991151
        public float EyeHeightUp = 0.0003f; // this is better => 0.0002f;
#endif
        public float lastLow = 0.0f;
        public float lastHigh = 0.0f;
        
        public int MovingAverageSize = 120;      
        public int JawMovingAverageSize = 1;           
        public int MovingAverageSizeMinMax = 3; 
        public int JawMovingAverageSizeMinMax = 1;          
        public float SmileCompensatorGain = 0.0049f; // was 0.0042 but that was with out lowpass
        public float JawDiffAveResetThreshold = 0.1f;  //https://docs.google.com/spreadsheets/d/1MLr7ZLxxOr4aZngMSYSh3LED71HbUFljyb4o7CYQrbE/edit#gid=2125061242
        public float SmileDiffAveResetThreshold = 0.2f;  //https://docs.google.com/spreadsheets/d/1MLr7ZLxxOr4aZngMSYSh3LED71HbUFljyb4o7CYQrbE/edit#gid=2125061242
        public float BrowDiffAveResetThreshold = 0.1f;
        public int MouthResetFrames = 45; //previous at 10 but for longer dialog it was not working. TODO do this on length of time talking  https://docs.google.com/spreadsheets/d/1MLr7ZLxxOr4aZngMSYSh3LED71HbUFljyb4o7CYQrbE/edit#gid=194644954
        public float MouthCountSmoothTime = 0.15f;
        public bool UseDynamicHeights = false;
        
        //Android move the upper and lower verts at the same time (making the measurement useless) when blinking so just use the upper
        private MovingAverage _eyeTopLidLeftAve;
        private MovingAverage _eyeTopLidRightAve;
        private MovingAverage _eyeTopLidLeftAveMaxPeak;
        private MovingAverage _eyeTopLidRightAveMaxPeak;
        private MovingAverage _eyeTopLidLeftAveMinPeak;
        private MovingAverage _eyeTopLidRightAveMinPeak;
        private float _lastSmilePercent;
        private float _lastJawPercent;
        private float _lastBrowRAW;
        private float _previousRatioRight;
        private float _previousRatioLeft;      
        private Vector3 _lastCenterTransform;
        private int _countLastMouthReset;
        private bool _isMouthCountDown;
        private bool _extendAverage;
        
        //Filters
        private ILowPassFilter _LowPassLidRight; 
        private ILowPassFilter _LowPassLidLeft; 
        public uint _Order = 4;
        public float _CutoffFrequency = 3;     // [Hz]
        private float _SamplingFrequency = 30; // [Hz]
        private uint _VectorSize = 1;

        //DEBUGGING
        private bool _isDebuggerEnabled = false;
        private string _debugText = "";
        private bool _doDataCollection = false;
        private bool _printDataHeader = false;
        private bool _dataMark = false;
        
        public ARCoreBlendShapeEyes()
        {
            ResetMovingAverage();
            _lastCenterTransform = new Vector3(0f,0f,0f);
            ResetLowpass();
        }

        public void ResetMovingAverage()
        {
            _eyeTopLidLeftAve = new MovingAverage(MovingAverageSize);
            _eyeTopLidRightAve = new MovingAverage(MovingAverageSize);
            _eyeTopLidLeftAveMaxPeak = new MovingAverage(MovingAverageSizeMinMax);
            _eyeTopLidRightAveMaxPeak  = new MovingAverage(MovingAverageSizeMinMax);
            _eyeTopLidLeftAveMinPeak  = new MovingAverage(MovingAverageSizeMinMax);
            _eyeTopLidRightAveMinPeak  = new MovingAverage(MovingAverageSizeMinMax);
            
        }
        
        public void ResetLowpass(float intialvalue = 0.0f)
        {
            float[] init = new float[1];
            init[0] = intialvalue;
            _LowPassLidRight  = new ButterworthFilter(_Order, _SamplingFrequency, _CutoffFrequency, _VectorSize);
            _LowPassLidLeft  = new ButterworthFilter(_Order, _SamplingFrequency, _CutoffFrequency, _VectorSize);
        }
        
        public string DebugText
        {
            get { return _debugText; }
        }
        
        public bool EnableDebugText
        {
            get { return _isDebuggerEnabled; }
            set { _isDebuggerEnabled = value; }
        }
        public void DataMark(bool value)
        {
            _dataMark = value;
        }
        
        public float[] CalculateEyeBlendShapeCoefficients(Vector3[] vertices, float[] blendshapes, Vector3 faceForward, Vector3 facePos, Vector3  cameraForward, Vector3  cameraPos)
        {
            // MUST BE CALLED AFTER MOUTH is calculated
            // The Vertices are taken from the canonical_face_mesh.fbx
            // 263 Left outside vertex, 362 Left inside vertex,  386 top of Left eye lid, 374 bottom of Left eye lid
            // 33 Right outside vertex, 133 Right inside vertex, 159 top of Right eye lid, 145 bottom of Left eye lid
            // the players Eye does not match the Model eye in game on purpose from what I see in ARKit.
            // ex. reading the player right eye to drive the model left eye

            Vector3 toFaceFromCamera = facePos - cameraPos;
            // Model Eye Left (read player Right)
            if (_isDebuggerEnabled)
            {
                _debugText = "";
            }
            
            if (_doDataCollection)
            {
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreCenterTransform",
                              faceForward.x,
                              faceForward.y,
                              faceForward.z));               
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID",
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID].x,
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID].y,
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID",
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID].x,
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID].y,
                              vertices[ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID",
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID].x,
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID].y,
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID",
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID].x,
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID].y,
                              vertices[ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID].z));
                Debug.Log(String.Format(
                              " dataMark {0}, // DataMark",
                              _dataMark));
                
            }

            if (_isDebuggerEnabled)
            {
                _debugText = _debugText + String.Format(
                    "Face Forward x: {0:0.000}, y: {1:0.000}, z: {2:0.000}\n",
                    faceForward.x, faceForward.y, faceForward.z);
                    
                _debugText = _debugText + String.Format(
                    "Brow Up: {0} Brow Down: {1} Smile Left: {2} Smile Right: {3} JawOpen: {4}\n",
                    blendshapes[NonDepthBlendShapeConstants.BROW_UP],
                    blendshapes[NonDepthBlendShapeConstants.BROW_DOWN],
                    blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_LEFT],
                    blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_RIGHT],
                    blendshapes[NonDepthBlendShapeConstants.JAW_OPEN]
                );
            }
            
            // Mouth compensation because the mouth movement effects the eyes in a pretty drastic way
            _extendAverage = false;
            if (_isMouthCountDown)
            {
                _countLastMouthReset--;
                if (_countLastMouthReset < 0)
                {
                    _isMouthCountDown = false;
                    //doing both since the off switch above will only fire once
                    _eyeTopLidRightAve.MaxWindow = MovingAverageSize;
                    _eyeTopLidLeftAve.MaxWindow = MovingAverageSize;
                    _eyeTopLidRightAveMaxPeak.MaxWindow = MovingAverageSizeMinMax;
                    _eyeTopLidLeftAveMaxPeak.MaxWindow = MovingAverageSizeMinMax;
                    _eyeTopLidRightAveMinPeak.MaxWindow = MovingAverageSizeMinMax;
                    _eyeTopLidLeftAveMinPeak.MaxWindow = MovingAverageSizeMinMax;
                     
                    _extendAverage = true;
                }
            }

            _previousRatioRight = CalculateAndSetEyeBlendShapeCoefficient(vertices, blendshapes, _eyeTopLidRightAve, _LowPassLidRight,
                                                                          ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID, ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID,
                                                                          NonDepthBlendShapeConstants.EYE_WIDE_LEFT, NonDepthBlendShapeConstants.EYE_BLINK_LEFT,
                                                                          NonDepthBlendShapeConstants.EYE_SQUINT_LEFT, "Right",
                                                                          faceForward, toFaceFromCamera, _previousRatioRight,  _eyeTopLidRightAveMaxPeak, _eyeTopLidRightAveMinPeak);
            
            _previousRatioLeft = CalculateAndSetEyeBlendShapeCoefficient(vertices, blendshapes, _eyeTopLidLeftAve,_LowPassLidLeft,
                                                                          ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID, ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID,
                                                                          NonDepthBlendShapeConstants.EYE_WIDE_RIGHT, NonDepthBlendShapeConstants.EYE_BLINK_RIGHT,
                                                                          NonDepthBlendShapeConstants.EYE_SQUINT_RIGHT, "Left", 
                                                                          faceForward, toFaceFromCamera, _previousRatioLeft, _eyeTopLidLeftAveMaxPeak, _eyeTopLidLeftAveMinPeak);
            
            // Wink
            //  ARcore struggles with winking.  If you look at the Debug Facemesh it won't full close the winking eye
            //  so we are going to look for difference in right and left and a eyebrow diff
            float eyeWideRight = blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_RIGHT];
            float eyeWideLeft = blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_LEFT];
            float eyeWideDiff = Mathf.Abs(eyeWideLeft - eyeWideRight);
            float browLeft = vertices[ARCoreFaceMeshConstants.BROW_UPPER_LEFT].y;
            float browRight = vertices[ARCoreFaceMeshConstants.BROW_UPPER_RIGHT].y;
            float browDiff = Mathf.Abs(browLeft - browRight);

            // cheek squint goes from normal 6: 41% 7: 36% goes to 6: 70-100% 7: 70-100% when winking
            float cheekRight = blendshapes[NonDepthBlendShapeConstants.CHEEK_SQUINT_RIGHT];
            float cheekLeft = blendshapes[NonDepthBlendShapeConstants.CHEEK_SQUINT_LEFT];
            bool cheeksAreSquinting = (cheekLeft > 60f || cheekRight > 60f); // in percent

            bool isWink = false; //*((eyeWideDiff > 5f) && browDiff > 0.0018f && cheeksAreSquinting); // from data testing 5 was sometimes the best diff ARCore gives.  sometimes it does much better
            
            if (isWink)
            {
                if (eyeWideRight > eyeWideLeft)
                {
                    blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_LEFT] = 100.0f;
                }
                else
                {
                    blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT] = 100.0f;
                }
            }
            else 
            {
                //if we are not blinking then lets lock our eyes together
                if (eyeWideRight > eyeWideLeft)
                {
                    blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_LEFT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_RIGHT];
                    blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_LEFT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT];
                    blendshapes[NonDepthBlendShapeConstants.EYE_SQUINT_LEFT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_SQUINT_RIGHT];
                }
                else
                {
                    blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_RIGHT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_WIDE_LEFT];
                    blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_RIGHT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_BLINK_LEFT];
                    blendshapes[NonDepthBlendShapeConstants.EYE_SQUINT_RIGHT] =
                        blendshapes[NonDepthBlendShapeConstants.EYE_SQUINT_LEFT];  
                }
            }
            
            // capture values for next frame
            _lastCenterTransform = faceForward;
            _lastBrowRAW = blendshapes[NonDepthBlendShapeConstants.BROW_UP];
            _lastJawPercent = Mathf.Clamp(blendshapes[NonDepthBlendShapeConstants.JAW_OPEN],0f,100f)/100f;
            _lastSmilePercent = Mathf.Clamp(blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_LEFT],0f,100f)/100f;
            return blendshapes;
        }

        private float CalculateAndSetEyeBlendShapeCoefficient(Vector3[] vertices, float[] blendshapes, MovingAverage eyeTopLidAve,  ILowPassFilter lowPassLid,
                                                             int topLidVertIndex, int bottomLidVertIndex,
                                                             int bscIndexWide, int bscIndexBlink, int bscIndexSquint,
                                                             string debugContext, Vector3 faceForward,
                                                             Vector3 toFaceFromCamera,
                                                             float previousRatio, MovingAverage eyeTopLidAveMaxPeak, MovingAverage eyeTopLidAveMinPeak)
        {
            bool reset = false;
            // Let hurts my soul but on Android both the top lid and the bottom lid move down at the same time when eyes are closed... so the vector measurring them is not tha uses ful
            float topLidYRaw = vertices[topLidVertIndex].y;
            float topLidY = topLidYRaw; // will get altered below
            float
                squintCenterPoint =
                    0.3f; // from analyzing ARKit the squint value triangulate roughly around the squintCenterPoint of 0.3f
            float inverseSquintCenter = 1 - squintCenterPoint;
            Vector3 playerFaceVelocity = faceForward - _lastCenterTransform;

            // on Android Brows will increase the Eye measurement
            float browRawPercent = blendshapes[NonDepthBlendShapeConstants.BROW_UP];
            float browRAWDiff = browRawPercent - _lastBrowRAW;
            float browupPercent = Mathf.Clamp(browRawPercent,0f,100f)/100f;
            float browcompensation = Mathf.Min(0.0005f * browupPercent, 0.0005f); 
            
            // on Android JAW will increase the Eye measurement
            float jawopenRAW = blendshapes[NonDepthBlendShapeConstants.JAW_OPEN];
            float jawopenPercent = Mathf.Clamp(jawopenRAW,0f,120f)/120f;
            float jawcompensation = Mathf.Min(0.0005f * jawopenPercent, 0.0009f); 
            float jawDiff = Mathf.Abs(jawopenPercent - _lastJawPercent);
            
            // on Android Smiling will decrease eye measurement and make it seem like they are alwyas closing
            float smilePercent = Mathf.Clamp(blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_LEFT],0f,100f)/100f;
            float smilediff = smilePercent - _lastSmilePercent;
            float mouthClosedPercent = blendshapes[NonDepthBlendShapeConstants.MOUTH_CLOSE];
            
            float[] input= new float[1];
            float[] output= new float[1]; 
            float smilecompensationNew = Mathf.Min(SmileCompensatorGain * smilePercent, SmileCompensatorGain);
            // float jawcompensationNew = Mathf.Min(JawCompensatorGain * jawopenPercent, JawCompensatorGain);
            input[0] = topLidY + smilecompensationNew; // - jawcompensationNew; //  calculations were done on unflitered data
           

            if (lowPassLid.IsInit())
            {
                lowPassLid.Apply(input , ref output);
            }
            else
            {
                lowPassLid.Init(input);
                output[0] = input[0];
            }
            topLidY = output[0];


            // the Jaw has a dramatic effect on the eyes so lets make our moving average more dynamic for a couple of frames
            if (jawDiff > JawDiffAveResetThreshold || Mathf.Abs(smilediff) > SmileDiffAveResetThreshold) // || Mathf.Abs(browRAWDiff) > BrowDiffAveResetThreshold)  // reset our average because https://docs.google.com/spreadsheets/d/1MLr7ZLxxOr4aZngMSYSh3LED71HbUFljyb4o7CYQrbE/edit#gid=1580907687
            {
                _isMouthCountDown = true;
                _countLastMouthReset = MouthResetFrames;
                eyeTopLidAve.ResetAverage();
                eyeTopLidAve.MaxWindow = JawMovingAverageSize;
                eyeTopLidAveMaxPeak.ResetAverage();
                eyeTopLidAveMaxPeak.MaxWindow = JawMovingAverageSizeMinMax;
                eyeTopLidAveMinPeak.ResetAverage();
                eyeTopLidAveMinPeak.MaxWindow = JawMovingAverageSizeMinMax;
                reset = true;
            }
            float averageTopLid = eyeTopLidAve.AddValue(topLidY);
            float averageTopLidMax = eyeTopLidAveMaxPeak.AddIfMaxPeakLastFrame(topLidY,averageTopLid);
            float averageTopLidMin = eyeTopLidAveMinPeak.AddIfMinPeakLastFrame(topLidY,averageTopLid);

            if(_isDebuggerEnabled)
                _debugText += debugContext;
            
            // averageTopLid is our EYE_DEFAULT_OPEN_PERCENT open
            // the Data:  https://docs.google.com/spreadsheets/d/1URYXIlarxFrFDt6hXYoNF0EuAw4KNMiG8t9j2hGsOII/edit#gid=1332493673
            // adding the average while the eye are closed in data muddys the water... so thinking 
            // 1) average while in off state  say anything 10% away from the start average?  and track while in the other state? what is a parameter that we can watch?
            float openRatio = 0.0f;     
            float percent = 0.0f;
            float diff = averageTopLid - topLidY;
            float remainingPercent = 1 - EYE_DEFAULT_OPEN_PERCENT;  // was 0.2f
            float eyeHeightup = EyeHeightUp;
            float eyeHeightDown = EyeHeightDown;
            
            if (UseDynamicHeights && averageTopLidMin > 0f )
            {
                float diffHeight = averageTopLid - averageTopLidMin;
                if (diffHeight > 0.0001)
                {
                    eyeHeightDown = diffHeight;
                }
            }
                
                
            if (diff < 0f)
            {
                diff = -diff;
                lastHigh = diff;
                percent = EYE_DEFAULT_OPEN_PERCENT + remainingPercent * (diff / eyeHeightup);
                openRatio = Mathf.Clamp(percent,0f,1f);
            }
            else
            {
                lastLow = diff;
                percent = (EYE_DEFAULT_OPEN_PERCENT - EYE_DEFAULT_OPEN_PERCENT*(diff / eyeHeightDown));
                openRatio = Mathf.Clamp(percent,0f,1f);
            }

            if (_isMouthCountDown)
            {
                float yVelocity = 0.0f;
                openRatio = Mathf.SmoothDamp(previousRatio, openRatio, ref yVelocity, MouthCountSmoothTime);  // with out this it pops
            }

            blendshapes[bscIndexWide] = openRatio * COEFFICIENT_SCALE;
            
            // observed that we never get zero for fully closed so lets work with a min measurement
            float eyeClosedAdjustedRatio = 1f - openRatio;
            blendshapes[bscIndexBlink] = eyeClosedAdjustedRatio * COEFFICIENT_SCALE;
            // For Squint it is a triangle around the split point effectively roughly below center
            float eyeSquintCoef = (openRatio <= squintCenterPoint)
                ? (openRatio / squintCenterPoint)
                : (1 - ((openRatio - squintCenterPoint) / inverseSquintCenter));
            blendshapes[bscIndexSquint] =
                eyeSquintCoef * 0.6f * COEFFICIENT_SCALE; // ARKit never measure above 60% so scaling for that

            if (_isDebuggerEnabled)
            {
                _debugText = _debugText + String.Format(", FaceV mag: {0:0.000000} ({1:+0.000000;-#.000000},{2:+0.000000;-#.000000},{3:+0.000000;-#.000000}\n",
                                                        playerFaceVelocity.magnitude,playerFaceVelocity.x, playerFaceVelocity.y,
                                                        playerFaceVelocity.z);
                

                _debugText = _debugText + String.Format(
                    "brow comp {0:0.0000} , smile comp {1:0.0000}, open ratio {2:00.0000}, closed ratio {3:00.0000}",
                    browcompensation,
                    smilecompensationNew,
                    openRatio,
                    eyeClosedAdjustedRatio);
            }
            
            if (_doDataCollection)
            {
                if (_printDataHeader)
                {
                    Debug.Log(String.Format(
                                  "_dataMark, openRatio,jawopenPercent,topLidYRaw," +
                                  "topLidY,averageTopLid, averageTopLidMax, averageTopLidMin, _eyeTopLidAve.Count, reset," +
                                  "extendAverage, smilePercent, smilediff, jawDiff," +
                                  "jawopenRAW, mouthClosedPercent, browRAWDiff, browcompensation," +
                                  "smilecompensationNew, faceForward.x, faceForward.y, browRawPercent," +
                                  "vertices[topLidVertIndex].y, vertices[bottomLidVertIndex].y, Time.realtimeSinceStartup, toFaceFromCamera.x," +
                                  "toFaceFromCamera.y, toFaceFromCamera.z, diff, EyeHeightUp," +
                                  "EyeHeightDown,  //cham data Right cham data Left "));
                    _printDataHeader = false;
                }
                Debug.Log(String.Format(" {0},{1},{2},{3}," + 
                                        "{4},{5},{6},{7}," +
                                        "{8},{9},{10},{11}," +
                                        "{12},{13},{14},{15}," + 
                                        "{16},{17},{18},{19}," + 
                                        "{20},{21},{22},{23}," + 
                                        "{24},{25}, {26},{27}," +
                                        " {28},{29},{30} //cham data {31} ",
                                        _dataMark, openRatio,jawopenPercent,topLidYRaw,
                                        topLidY,averageTopLid, averageTopLidMax, averageTopLidMin, eyeTopLidAve.Count, reset,
                                        _extendAverage, smilePercent, smilediff, jawDiff,
                                        jawopenRAW, mouthClosedPercent, browRAWDiff, browcompensation,
                                        smilecompensationNew, faceForward.x, faceForward.y, browRawPercent,
                                        vertices[topLidVertIndex].y, vertices[bottomLidVertIndex].y, Time.realtimeSinceStartup, toFaceFromCamera.x,
                                        toFaceFromCamera.y, toFaceFromCamera.z, diff, EyeHeightUp,
                                        EyeHeightDown, debugContext ));
            }

            return openRatio;
        }
        
        public void ToggleDataCollection()
        {
            _doDataCollection = !_doDataCollection;
            if (_doDataCollection)
            {
                _printDataHeader = true;
                Debug.Log("DataCollection begin\n");
            }
            else
            {
                Debug.Log("DataCollection end\n");
            }
        }
        
    }
}
