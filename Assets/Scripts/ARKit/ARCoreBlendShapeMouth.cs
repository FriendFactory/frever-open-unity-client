using System;
using System.Collections.Generic;
using LowPassFilter;
using UnityEngine;

namespace ARKit
{
    public sealed class ARCoreBlendShapeMouth
    {
        private const float COEFFICIENT_SCALE = 100.0f;
        private const float DATA_JAW_MIN_AVE = 0.07095f;
        private const float DATA_JAW_MAX = 0.0743f; // measured from test data this was the Max value
        private const float DATA_JAW_MAX_HIGH_PEAK = 0.0852f;
        // There is more movement above the average resting point then the bottom for me
        private const float BROW_UP_RANGE = 8f;
        private const float BROW_DOWN_RANGE = 4f;
        
        // MOUTH
        private float _jawMinOpened = DATA_JAW_MAX;
        private MovingAverage _jawMinAve = new MovingAverage();
        private MovingAverage _jawAve = new MovingAverage(90);
        
#if MOUTH_RIGHT_LEFT
        public bool _facePositionAdjustment = true;
        private ILowPassFilter _LowPassFilterMouthRightLeftDiff;
        public bool _useFilteredMouthRightLeft=true; 
 #endif

        //Brow
        private MovingAverage _browAve = new MovingAverage(120);
        private MovingAverage _playerFacingY = new MovingAverage(7);
        private float _browCenterVectorY = 0;
        private float _browDeadZone = 0.0f;
        private ILowPassFilter _lowPassFilterBrows;
        private ILowPassFilter _lowPassFilterFaceForwardY;
        public uint _Order = 4;
        public float _CutoffFrequency = 3;   // [Hz]
        private float _SamplingFrequency = 30; // [Hz]
        private uint _VectorSize = 1;

        //DEBUGGING
        private bool _isDebuggerEnabled = false;
        private string _debugText = "";
        private bool _doDataCollection = false;
        private bool _printDataHeader = false;


        public ARCoreBlendShapeMouth()
        {
            _jawMinAve.LastMagnitude = DATA_JAW_MIN_AVE;
            ResetLowpass();
        }

        public void ResetLowpass()
        {
            _lowPassFilterBrows = new ButterworthFilter(_Order, _SamplingFrequency, _CutoffFrequency, _VectorSize);
            _lowPassFilterFaceForwardY = new ButterworthFilter(_Order, _SamplingFrequency, _CutoffFrequency, _VectorSize);
#if MOUTH_RIGHT_LEFT   
            float[] init = new float[1];
            init[0] = 0;
            _lowPassFilterMouthRightLeftDiff = new ButterworthFilter(_Order, _SamplingFrequency, _CutoffFrequency, _VectorSize);
            _lowPassFilterMouthRightLeftDiff.Init(init);
#endif
        }

        public string DebugText
        {
            get { return _debugText; }
        }
        
        public bool EnableDebugText
        {
            get { return _isDebuggerEnabled; }
            set
            {
                _isDebuggerEnabled = value;
            }
        }
        
        public string GetBrowDeadZone()
        {
            return _browDeadZone.ToString();
        }
        public void IncreaseDeadZone()
        {
            _browDeadZone += 0.05f;
        }
        
        public void DecreaseDeadZone()
        {
            _browDeadZone -= 0.05f;
            if (_browDeadZone < 0.0f)
            {
                _browDeadZone = 0.0f;
            }
        }
        
        // Calculate blendshapes value based on ARCore face mesh vertices, function is called every frame
        public float[] CalculateMouthBlendShapeCoefficients(Vector3[] vertices, float[] blendshapes, Vector3 faceForward, Vector3 facePos, Vector3  cameraForward, Vector3  cameraPos)
        {
            Vector3 toFaceFromCamera = facePos - cameraPos;
            if (_isDebuggerEnabled)
                _debugText = "";
            
            if (_doDataCollection)
            {
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.MID_PHILTRUM",
                              vertices[ARCoreFaceMeshConstants.MID_PHILTRUM].x,
                              vertices[ARCoreFaceMeshConstants.MID_PHILTRUM].y,
                              vertices[ARCoreFaceMeshConstants.MID_PHILTRUM].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.CHIN_CENTER",
                              vertices[ARCoreFaceMeshConstants.CHIN_CENTER].x,
                              vertices[ARCoreFaceMeshConstants.CHIN_CENTER].y,
                              vertices[ARCoreFaceMeshConstants.CHIN_CENTER].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER",
                              vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER].x,
                              vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER].y,
                              vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER",
                              vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].x,
                              vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].y,
                              vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.BROW_UPPER_LEFT",
                              vertices[ARCoreFaceMeshConstants.BROW_UPPER_LEFT].x,
                              vertices[ARCoreFaceMeshConstants.BROW_UPPER_LEFT].y,
                              vertices[ARCoreFaceMeshConstants.BROW_UPPER_LEFT].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT",
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT].x,
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT].y,
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT].z));
                Debug.Log(String.Format(
                              " new Vector3({0}f,{1}f,{2}f), // ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT",
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT].x,
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT].y,
                              vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT].z));
            }
            
            // JAW OPEN:
            Vector3 JawOpenVector = (vertices[ARCoreFaceMeshConstants.MID_PHILTRUM] - vertices[ARCoreFaceMeshConstants.CHIN_CENTER]);
            float jawOpenMagnitude = JawOpenVector.magnitude;
            
            float threshold = DATA_JAW_MAX; //0.01 normal or 0.04 when in a wierd place
            // peak to peak flucuates alot so instead lets look at our average value and min over time
            float jawAverage = _jawAve.AddValue(jawOpenMagnitude);

            if (_isDebuggerEnabled)
                _jawMinAve.DebugContext = "_jawMinOpened";
            _jawMinOpened = _jawMinAve.AddIfMinPeakLastFrame(jawOpenMagnitude, threshold);
            if (_jawMinOpened == 0.0f)
                _jawMinOpened = threshold;

            float jawMaxOpened = _jawMinOpened + (0.0254f * (1 + faceForward.y)); // 0.0254 1 inch, scale our value with y since it seems to work that way
            float jawOpenDiff = jawOpenMagnitude - _jawMinOpened;
            float jawOpenRatio = (jawOpenDiff/ (jawMaxOpened - _jawMinOpened));
            float jawOpen = Mathf.Clamp(COEFFICIENT_SCALE * jawOpenRatio, 0f, 300f);

            // Mouth Closed
            // https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/2928266-mouthclose
            // jawOpen and mouth close work together.
            // if jaw fully closed then no matter what mouth is closed: jawOpen 0.0 => mouthclosed 0.0
            // if jaw is fully open and lips are fully open: jawOpen 1.0 => mouthclosed 0.0
            // if jaw is fully open and our lips are closed: jawOpen 1.0 => mouthclosed 1.0
            // if we don't move our lips closed further then we will assume that our lips will be jawOpenDiff and that mouthclosed should be 0.0f
            // so the diff between jawOpenDiff and lipsOpenMagnitude is what we will use for calculating our 0-1 value
            float mouthClose = 0.0f;
            float lipsOpenMagnitude =
                (vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER] - vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER]).magnitude;

            if (jawOpenDiff <= 0f || lipsOpenMagnitude > jawOpenDiff || jawOpenRatio < 0.10f)  // I think we can math this out instead o these hard numbers but need to talk with Artist about expectation in the closed Jaw range  or put ins peaks filter
            {
                mouthClose = 0.0f;
            }
            else
            {
                float scaleRatio = Mathf.Clamp(jawOpenRatio / 0.35f,0.0f,1.0f); // scale this down up to 30% open
                mouthClose = (1.0f - (lipsOpenMagnitude / jawOpenDiff)) * scaleRatio; //as we get more open we can add more mouth close.   this is a work around until talk with Art 
            }
            mouthClose = Mathf.Clamp(COEFFICIENT_SCALE * mouthClose, 0f, 38f); // things look very strange in the mode if this gets more then 50.  should talk with Art team or filter
            if (_isDebuggerEnabled)
            {
                _debugText = _debugText +
                             String.Format(
                                 "jawOpenMagnitude: {0:0.0000}, _jawMinOpened: {1:0.00000} jawMaxOpened: {2:0.00000} jawOpenDiff: {3:0.00000} jawOpenRatio: {4:0.0000} jawAverage: {5:0.0000} threshold: {6:0.0000}\n",
                                 jawOpenMagnitude, _jawMinOpened, jawMaxOpened, jawOpenDiff, jawOpenRatio, jawAverage,
                                 threshold);

                _debugText = _debugText +
                             String.Format(
                                 "lipsOpenMagnitude: {0:000.0000} jawOpenDiff: {1:000.0000} mouthClose: {2:0.00000} dTime: {3}\n",
                                 lipsOpenMagnitude, jawOpenDiff, mouthClose, Time.realtimeSinceStartup);
            }

            if (_doDataCollection)
            {
                if (_printDataHeader)
                {
                    Debug.Log(" jawOpenMagnitude, _jawMinOpened,jawMaxOpened,jawOpenDiff, jawOpenRatio, lipsOpenMagnitude, jawOpenDiff, mouthClose, Time.realtimeSinceStartup, threshold, jawAverage, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z, //cham data mouth ");
                }
                Debug.Log(String.Format(" {0},{1},{2},{3},{4},{5}," +
                                        "{6},{7},{8}, {9}, {10}, {11}, {12}, {13}, //cham data mouth ",
                                        jawOpenMagnitude, _jawMinOpened,jawMaxOpened,jawOpenDiff, jawOpenRatio, lipsOpenMagnitude, jawOpenDiff, mouthClose, Time.realtimeSinceStartup, threshold, jawAverage, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z ));
            }

            
            // FACEMESH IS IN PLAYER PERSPECTIVE BUT WE SWAP PERSPECTIVE AND DO A MIRROR FOR THE BLEND SHAPE
            // FACEMESH RIGHT = BSW LEFT
            // FACEMESH LEFT = BSW RIGHT
            // WILL DO SWITCH WHEN SETTING BSW Below
            #region OriginalSmileFunnelCalulationsNeedsReWrite
            
            float mouthCloseRawY =
                (vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER] - vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER]).y; // 13 lowest center upper lip, 14 highest center lower lip
            // DISABLE ORIGINAL CODE: could calculate:  float jawForward = vertices[ARCoreFaceMeshConstants.CHIN_CENTER].y * 1000f - 48f;
            
            float mouthCloseRaw = mouthCloseRawY * COEFFICIENT_SCALE; // what is this for?
            // MOSTLY WORKS:  I don't like this code from the original but it mostly works
            float mouthSmileLeft =
                (vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT][1] * 1000 + 44f) *
                14f; // 287 player 2nd point outside left corner mouth

            float mouthSmileRight =
                (vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT][1] * 1000 + 44f) *
                14f; // 57 player 2nd point outside right corner mouth

            // MOSTLY WORKS:  I don't like this code from the original but it mostly works
            float mouthCornersDistance =
                (Mathf.Abs(vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_1ST_POINT_LEFT][0] -
                           vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_1ST_POINT_RIGHT][0]) * 1000f) -
                49.5f; // 291 player 1st point outside left corner mouth, 61 player 1st point outside right corner mouth

            float mouthCornerLeft =
                vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_LEFT][0] * 1000 -
                31.5f; // 287 player 2nd point outside left corner mouth
            
            float mouthCornerRight =
                vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_2ND_POINT_RIGHT][0] * 1000 +
                31.5f; // 57 player 2nd point outside right corner mouth

            float mouthPucker = 0.0f;
            float mouthFunnel = 0.0f;

            if (mouthCornerLeft < 0f && mouthCornerRight > 0f)
            {
                if (mouthCornersDistance < 0f)
                {
                    mouthPucker = Mathf.Abs(mouthCornersDistance) * 10f;
                    mouthFunnel = Mathf.Abs(mouthCornersDistance) * 10f;
                }

                if (mouthCloseRaw < 0.6f)
                {
                    mouthFunnel = 0f;
                }
            }
            
            #endregion
            
            #region MouthRightLeftDisabled
#if MOUTH_RIGHT_LEFT
            // Works but highly impacted by the angle of phone right-left which is different than head turn right-left with camera facing forward.
            float mouthLeft = 0.0f;
            float mouthRight = 0.0f;
            float diffAmount = 0.0f;
            const float MOUTH_RIGHT_LEFT_DIFF = 0.004f;
            float centerPoint = -faceForward.x / 100f; // from data measurements  problem when I move my mouth it moves the centerPoint
            if(_facePositionAdjustment)
                centerPoint += ((-toFaceFromCamera.x / 10f)*0.7f) - 0.0003f; // from data measurements the angle of phone to face shifts the coordinates https://docs.google.com/spreadsheets/d/1WwIREoLKuvrv-vGUt8ICdGTzM5xrGFtiX11dIvK-Ca4/edit#gid=1428638093
            float lowerLipMiddleRaw = vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].x; //  14 highest center lower lip
            
            diffAmount = lowerLipMiddleRaw - centerPoint;
            input[0] = diffAmount;
            _LowPassFilterMouthRightLeftDiff.Apply(input, ref output);
            if (_useFilteredMouthRightLeft)
                diffAmount = output[0];
            
            // need dead zone and to deal with player pos to camera angle not just face to camera
            if (lowerLipMiddleRaw > centerPoint) // Player Left/model right
            {
                mouthLeft = Mathf.Abs(diffAmount/MOUTH_RIGHT_LEFT_DIFF);
                mouthLeft = Mathf.Clamp(COEFFICIENT_SCALE * mouthLeft, 0f, 100f);
            }
            else // Player Right/model left
            {
                mouthRight = Mathf.Abs(diffAmount/MOUTH_RIGHT_LEFT_DIFF);
                mouthRight = Mathf.Clamp(COEFFICIENT_SCALE * mouthRight, 0f, 100f);
            }

            if ((mouthSmileLeft > 0 && mouthSmileRight > 0) || Mathf.Abs(toFaceFromCamera.x) > 0.05f)
            {
                mouthLeft = 0;
                mouthRight = 0;
            }

            if (_isDebuggerEnabled)
            {
                _debugText = _debugText + String.Format(" LHCL.x 0: {0:0.000000}",
                                                        vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].x);
                _debugText = _debugText + String.Format(" centerTransform.x 0: {0:0.000000}", faceForward.x);
                _debugText = _debugText + String.Format(" lowerLipMiddleRaw 0: {0:0.000000}", lowerLipMiddleRaw);
                _debugText = _debugText + String.Format(" centerPoint 0: {0:0.000000}", centerPoint);
                _debugText = _debugText + String.Format(" diffAmount 0: {0:0.000000} ", diffAmount);
            }

            if (_doDataCollection)
            {
                if (_printDataHeader)
                {
                    Debug.Log(" LHCL.x, centerTransform.x, lowerLipMiddleRaw,centerPoint, diffAmount, time, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z //cham data mouthrightleft ");
                }
                Debug.Log(String.Format(" {0},{1},{2},{3},{4},{5},{6},{7},{8}," +
                                        " //cham data mouthrightleft ",
                                        vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER].x, faceForward.x, lowerLipMiddleRaw,centerPoint, diffAmount,Time.realtimeSinceStartup, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z ));
            }
#endif            
#endregion

            #region OriginalCalulationsDisabled
            // old calculations from swift file before Cham started working on this.  Still has magic in it, but is working will revisit sometime
            // BEGIN: original code before DWC except for the _debugText

            
      /*    DISABLE ORIGINAL CODE:  I would try to sample the current value and then see if it goes up or down but on Android this might cause twitching
            float cheekSquintLeft =
                (vertices[ARCoreFaceMeshConstants.CHEEK_LEFT][1] * 10000 - 9f) *
                3f; //  player Left 330 cheek,  I feel like it would be better with 253
            float cheekSquintRight =
                (vertices[ARCoreFaceMeshConstants.CHEEK_RIGHT][1] * 10000 - 9f) *
                3f; //  player Right 101 cheek,  I feel like it would be better with 23 */
      

            
            // Support code for Blendshapes below that do not seem to do much and are disabled because they might hurt the simulation
           /* float leftMouthCorner =
                ((Mathf.Abs(vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_1ST_POINT_LEFT].y -
                            vertices[ARCoreFaceMeshConstants.CHIN_CENTER_TO_LIP_LOWEST_MID_POINT].y) * 1000) - 25f) *
                10f; // 291 player 1st point outside left corner mouth,  200?
            var rightMouthCorner =
                ((Mathf.Abs(vertices[ARCoreFaceMeshConstants.MOUTH_OUTSIDE_CORNER_1ST_POINT_RIGHT].y -
                            vertices[ARCoreFaceMeshConstants.CHIN_CENTER_TO_LIP_LOWEST_MID_POINT].y) * 1000) - 25f) *
                10f; // 61 player 1st point outside right corner mouth,   200? */
            
            /* // This code was part of causing the teeth to show through the skin when combined with NoseSneerLeft/right mouthDimpleLeft/rightand mouthRight/left
            // the values were opposite of each other and pull the face away from each other in a unnatural way that caused tearing
            //  Keeping it here as a ref and a warning for any attempts to improve 
            //  for reference mouthDimpleLeft (26) was 90.4, noseSneerLeft (5) was 83.411 and mouthRight (23) was 47.38 
            //  and these Blendshape numbers would show stretch in opposite directions and  cause the teeth to show
            //  removing  Dimple and sneer fixed the issue and the previous code calculations did not seem to show either value well
            float noseSneerLeft =
                ((vertices[ARCoreFaceMeshConstants.NOSTRIL_LEFT][1] * 1000) + 17f) * 30f; // 327 player left most lower nostril
            float noseSneerRight =
                ((vertices[ARCoreFaceMeshConstants.NOSTRIL_RIGHT][1] * 1000) + 17f) * 30f; // 98 player right most lower nostril
            
            float mouthDimpleLeft = 0.0f;
            if (leftMouthCorner > 0f)
            {
                mouthDimpleLeft = leftMouthCorner * 0.5f;
            }
            
            float mouthDimpleRight = 0.0f;
            if (rightMouthCorner > 0f)
            {
                mouthDimpleRight = rightMouthCorner * 0.5f;
            }
            */
            
            /*  This code does not seem to do anything
             For future reference https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/2928235-mouthstretchleft
             float mouthStretchLeft = 0.0f;
            if (leftMouthCorner < 0f)
            {
                mouthStretchLeft = Mathf.Abs(leftMouthCorner);
            }
            
            float mouthStretchRight = 0.0f;
            if (rightMouthCorner < 0f)
            {
                mouthStretchRight = Mathf.Abs(rightMouthCorner);
            }*/

            
            /* Disabled because they could hurt the simulation for future reference
             https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/2928242-mouthlowerdownleft
             https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/2928240-mouthupperupleft
             
            float mouthUpperUpLeft = 0.0f;
            float mouthUpperUpRight = 0.0f;
            float mouthLowerDownLeft = 0.0f;
            float mouthLowerDownRight = 0.0f;

            if (vertices[12][1] * 1000f > -36f)
            {
                mouthUpperUpLeft =
                    (vertices[ARCoreFaceMeshConstants.LIP_UPPER_OFF_CENTER_RIGHT].y -
                     vertices[ARCoreFaceMeshConstants.LIP_LOWER_OFF_CENTER_RIGHT].y) * 1000 -
                    7f; // 72 upper lip right one vertex off center lip,  86 lower lip right one off center lip,
                mouthUpperUpRight =
                    (vertices[ARCoreFaceMeshConstants.LIP_UPPER_OFF_CENTER_LEFT].y - vertices[ARCoreFaceMeshConstants.LIP_LOWER_OFF_CENTER_LEFT].y) *
                    1000 - 7f; // 302 upper lip Left one vertex off center lip,  316 lower lip left one off center lip,
            }
            else
            {
                mouthLowerDownLeft =
                    (vertices[ARCoreFaceMeshConstants.LIP_UPPER_OFF_CENTER_RIGHT].y -
                     vertices[ARCoreFaceMeshConstants.LIP_LOWER_OFF_CENTER_RIGHT].y) * 1000 -
                    7f; // 72 upper lip right one vertex off center lip,  86 lower lip right one off center lip,
                mouthLowerDownRight =
                    (vertices[ARCoreFaceMeshConstants.LIP_UPPER_OFF_CENTER_LEFT].y - vertices[ARCoreFaceMeshConstants.LIP_LOWER_OFF_CENTER_LEFT].y) *
                    1000 - 7f; // 302 upper lip Left one vertex off center lip,  316 lower lip left one off center lip,
            }
            */
            
          /*  This code does not seem to do anything 
           for future reference: https://developer.apple.com/documentation/arkit/arfaceanchor/blendshapelocation/2928241-mouthpressleft
           
           float mouthPressRaw =
                vertices[ARCoreFaceMeshConstants.LIP_LOWER_MIDDLE_RIGHTHALF].y * 1000 +
                46f; // 180 player right middle of right half of lower lip
            float mouthPressLeft = 0.0f;
            float mouthPressRight = 0.0f;
            if (mouthPressRaw > 1f)
            {
                // dwc why did he do this? float mouthStretchLeft = mouthStretchLeft;
                if (mouthStretchLeft > 25f)
                {
                    mouthPressLeft = mouthPressRaw * 15f;
                }

                // dwc why did he do this? float mouthStretchRight = mouthStretchRight;
                if (mouthStretchRight > 25f)
                {
                    mouthPressRight = mouthPressRaw * 15f;
                }
            } */

            // END: original code before DWC 

            #endregion
            
            #region Brows
            // Brows https://docs.google.com/spreadsheets/d/1fkypgUChWWgpb01hPmxq80afVErH2cJW3kJzBAZQldw/edit#gid=2107390111
            // seeing some oscillation when moving mouth future things to try
            // 1) moving mouth seems to also effect faceForward so maybe our average is getting reset, but I see this mostly on brow up which is not using it
            // 2) maybe we just need to compensate for the jaw movement like the eyes
            float
                browRaw = vertices[ARCoreFaceMeshConstants.BROW_UPPER_LEFT].y *
                          1000; // 282 player left upper brow,  why not also use 52 which is player right upper brow
         
            float[] input= new float[1];
            float[] output= new float[1]; 
            float browY= browRaw;
            float smilepercent = Mathf.Clamp(mouthSmileRight, 0f, 100f) / 100f;
            float smileCompensation = 3.27f * smilepercent; // smile of 0.61 percent can cause a -2 decrease in browRaw https://docs.google.com/spreadsheets/d/1x3EAYLX0tZ-gtVtPplBkbJ0tKNJR4Oyg0uaA83sCrGs/edit#gid=1245439388
            input[0] = browRaw + smileCompensation;
            if (_lowPassFilterBrows.IsInit())
            {
                _lowPassFilterBrows.Apply(input , ref output);
                browY = output[0];               
            }
            else
            {
                _lowPassFilterBrows.Init(input);
                browY = input[0];
            }

            float faceForwardYFiltered;
            input[0] = faceForward.y;
            if (_lowPassFilterFaceForwardY.IsInit())
            {
                _lowPassFilterFaceForwardY.Apply(input , ref output);
                faceForwardYFiltered = output[0];               
            }
            else
            {
                _lowPassFilterFaceForwardY.Init(input);
                faceForwardYFiltered = input[0];
            }

            _playerFacingY.AddValue(faceForwardYFiltered);

            float browaverage = 0.0f;
            float browaverageRAW = 0.0f;

            if (_browAve.isWindowFull()) // measure a couple of seconds at the begining to use as our mid point
            {
                browaverageRAW = _browAve.Average;
            }
            else
            {
                _browCenterVectorY = faceForwardYFiltered;
                browaverageRAW = _browAve.AddValue(browY);
            }

            // might only work in one direction so might need a different one for the other
            float faceAngleDiff = faceForwardYFiltered - _browCenterVectorY; 
            float averageFaceCompensation = -8.68f * faceAngleDiff + -0.244f; // numbers were from data analysis in these=> https://docs.google.com/spreadsheets/d/1fkypgUChWWgpb01hPmxq80afVErH2cJW3kJzBAZQldw/edit#gid=2107390111  https://docs.google.com/spreadsheets/d/14_E0l00oouJYiTtu0Y0PoTeaJrIhD2D85BBn0PMtT1I/edit#gid=1594202075
            browaverage = browaverageRAW + averageFaceCompensation; 

            float browUp = 0.0f;
            float browDown = 0.0f;
            float browPercent = 0.0f;
            float browDiff = browaverage - browY;

            if (browDiff < 0f)
            {
                browDiff = -browDiff;
                browPercent = 100f * ((browDiff-_browDeadZone) / (BROW_UP_RANGE-_browDeadZone));
                browUp = Mathf.Clamp(browPercent,0f,100f);
            }
            else
            {
                browPercent = 100f * ((browDiff-_browDeadZone) / (BROW_DOWN_RANGE-_browDeadZone));
                browDown = Mathf.Clamp(browPercent,0f,100f);
            }
            
            if (_doDataCollection)
            {
                if (_printDataHeader)
                {
                    Debug.Log(" jawOpen, faceForward.y, faceForwardYFiltered, browRaw, browY, browaverage, browDiff, browUp, browDown, Time.realtimeSinceStartup, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z, browPercent,_browAve.isWindowFull(), faceAngleDiff, browaverageRAW, averageFaceCompensation, faceForward.y Trend Slope,smileCompensation, smile   //cham data brow ");
                }
                Debug.Log(String.Format(" {0},{1},{2},{3},{4},{5}," +
                                        "{6},{7},{8}, {9}, {10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20} //cham data brow ",
                                        jawOpen, faceForward.y, faceForwardYFiltered, browRaw, browY, browaverage, browDiff, browUp, 
                                        browDown,Time.realtimeSinceStartup, toFaceFromCamera.x, toFaceFromCamera.y, toFaceFromCamera.z,
                                        browPercent, _browAve.isWindowFull(), faceAngleDiff, browaverageRAW, averageFaceCompensation, 
                                        _playerFacingY.GetTrendSlope(), smileCompensation, mouthSmileLeft));
            }
            #endregion

            // SWITCH INPUT
            // FACEMESH IS IN PLAYER PERSPECTIVE BUT WE SWAP PERSPECTIVE AND DO A MIRROR FOR THE BLEND SHAPE
            // FACEMESH RIGHT = BSW LEFT, FACEMESH LEFT = BSW RIGHT
            
            // blendshapes that are newly calculated
            blendshapes[NonDepthBlendShapeConstants.JAW_OPEN] = jawOpen;
            blendshapes[NonDepthBlendShapeConstants.BROW_UP] = browUp;
            blendshapes[NonDepthBlendShapeConstants.BROW_DOWN] = browDown;
            blendshapes[NonDepthBlendShapeConstants.MOUTH_CLOSE] = mouthClose;
            
            //Blend shapes that seem to work but original code is weird
            blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_RIGHT] = mouthSmileLeft;   // FACEMESH mouthSmileLeft;
            blendshapes[NonDepthBlendShapeConstants.MOUTH_SMILE_LEFT] = mouthSmileRight ;  // FACEMESH mouthSmileRight;         
            blendshapes[NonDepthBlendShapeConstants.MOUTH_FUNNEL] = mouthFunnel;
            blendshapes[NonDepthBlendShapeConstants.MOUTH_PUCKER] = mouthPucker;
  
#if MOUTH_RIGHT_LEFT
            //Blend shapes that sort of work and original code is weird should try and rework
            blendshapes[NonDepthBlendShapeConstants.MOUTH_RIGHT] = mouthLeft ; // FACEMESH mouthLeft;
            blendshapes[NonDepthBlendShapeConstants.MOUTH_LEFT] = mouthRight;  // FACEMESH mouthRight;
#endif

            // FOR FUTURE BLENDSHAPE FOLKS
            // Previous version of the code did try to calculate these Blendshapes but they were previous to dwc
            // and were causing conflict with the other Blendshapes. so be care full with them as they can lead
            // to teeth cutting through the skin when they calculate opposite directions and stretch the face when combined
            // with mouthLeft/Right
            // blendshapes[NonDepthBlendShapeConstants.NOSE_SNEER_RIGHT] = 0.0f; //noseSneerRight was calculated in previous code but was wrong; // FACEMESH noseSneerRight;
            // blendshapes[NonDepthBlendShapeConstants.NOSE_SNEER_LEFT] = 0.0f; //noseSneerLeft was calculated in previous code but was wrong; // FACEMESH noseSneerLeft;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_DIMPLE_RIGHT] = 0.0f; //mouthDimpleRight;       // FACEMESH mouthDimpleRight;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_DIMPLE_LEFT] = 0.0f; //mouthDimpleLeft;         // FACEMESH mouthDimpleLeft;
            
            // -Blendshapes that could be calculated but old code was wrong was possibly hurting current values-
            // blendshapes[NonDepthBlendShapeConstants.JAW_FORWARD] = jawForward;
            // blendshapes[NonDepthBlendShapeConstants.JAW_RIGHT] = 0.0f; //FACEMESH jawLeft;
            // blendshapes[NonDepthBlendShapeConstants.JAW_LEFT] = 0.0f;  //FACEMESH jawRight;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_STRETCH_RIGHT] = mouthStretchLeft;     // FACEMESH mouthStretchLeft;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_STRETCH_LEFT] = mouthStretchRight ;       // FACEMESH mouthStretchRight;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_PRESS_RIGHT] = mouthPressLeft ; // FACEMESH mouthPressLeft;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_PRESS_LEFT] = mouthPressRight;  // FACEMESH mouthPressRight;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_UPPER_UP_RIGHT] = mouthUpperUpLeft ;     // FACEMESH mouthUpperUpLeft;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_UPPER_UP_LEFT] = mouthUpperUpRight;       // FACEMESH mouthUpperUpRight;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_LOWER_DOWN_RIGHT] = mouthLowerDownLeft; // FACEMESH mouthLowerDownLeft;
            // blendshapes[NonDepthBlendShapeConstants.MOUTH_LOWER_DOWN_LEFT] = mouthLowerDownRight ;   // FACEMESH mouthLowerDownRight;
            
            // -Blendshapes that previously were calculated but old code was doing something odd and they do not seem to help simulation and might be hurting-
           // blendshapes[NonDepthBlendShapeConstants.CHEEK_SQUINT_RIGHT] = cheekSquintLeft; // FACEMESH cheekSquintLeft;
           // blendshapes[NonDepthBlendShapeConstants.CHEEK_SQUINT_LEFT] = cheekSquintRight; // FACEMESH cheekSquintRight;
            
            // -Blendshapes that are not really being calculated but the old code was setting to zero-
           // blendshapes[NonDepthBlendShapeConstants.MOUTH_ROLL_UPPER] = 0.0f;                     
           // blendshapes[NonDepthBlendShapeConstants.MOUTH_ROLL_LOWER] = 0.0f;
          //  blendshapes[NonDepthBlendShapeConstants.MOUTH_SHRUG_LOWER] = mouthShrugLower;
           // blendshapes[NonDepthBlendShapeConstants.MOUTH_SHRUG_UPPER] = mouthShrugUpper;
            
            // -FROWN: OK ARCore just simply does not read frowning.  It will show an open mouth. 
           // blendshapes[NonDepthBlendShapeConstants.MOUTH_FROWN_RIGHT] = 0.0f;  // FACEMESH mouthFrownLeft;  ARCore does not read frowns
           // blendshapes[NonDepthBlendShapeConstants.MOUTH_FROWN_LEFT] = 0.0f ; // FACEMESH mouthFrownRight; ARCore does not read frowns


           if (_isDebuggerEnabled)
           {
               _debugText = _debugText + String.Format(" jawOpen 0: {0:0.0000}", jawOpen);
               _debugText = _debugText + String.Format(" mouthSmileLeft 17: {0:0.0000}", mouthSmileLeft);
               _debugText = _debugText + String.Format(" mouthSmileRight 16: {0:0.0000}", mouthSmileRight);
               _debugText = _debugText + String.Format(" mouthClose 18: {0:0.0000}", mouthClose);
               _debugText = _debugText + String.Format(" mouthFunnel 19: {0:0.0000}", mouthFunnel);
               _debugText = _debugText + String.Format(" mouthPucker 20: {0:0.0000}", mouthPucker);
#if MOUTH_RIGHT_LEFT
               _debugText = _debugText + String.Format(" mouthLeft 24: {0:0.0000}", mouthLeft);
               _debugText = _debugText + String.Format(" mouthRight 23: {0:0.0000}", mouthRight);
#endif
               _debugText = _debugText + "\n";
           }

            if (_doDataCollection && _printDataHeader)
            {
                _printDataHeader = false;
            }
            
            return blendshapes;
        }

        public void ResetToStartingValues()
        {
            ResetJawToStartValue();
        }

        public void ResetJawToStartValue()
        {
            _jawMinOpened = DATA_JAW_MAX;
            _jawMinAve.ResetAverage();
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
