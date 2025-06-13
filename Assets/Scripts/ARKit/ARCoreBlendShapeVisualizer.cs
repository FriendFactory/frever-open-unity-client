using System;
using System.Runtime.CompilerServices;
#if UNITY_IOS
using SwiftPlugin.Source;
#endif
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Linq;
using JetBrains.Annotations;
using System.Collections;
using Sentry.Protocol;

namespace ARKit
{
    public sealed class ARCoreBlendShapeVisualizer : NonTrueDepthBlendShapeVisualizer
    {
        private const float REQUIRED_LIGHTING_BRIGHTNESS_THRESHOLD = 0.4f;
        private ARCoreBlendShapeCoefficients _blendShapeCoefficients;
        #if UNITY_ANDROID
        private ARFaceManager _arFaceManager;
        #endif
        private ARCameraManager _arCameraManager;
        // Onscreen Debug options
        private bool _isDebuggerEnabled = false;
        private bool _datacollection = false;
        private bool _isDebuggerEnabledMouth = false;
        private bool _isDebuggerEnabledEyes = false;

        private string _debugText = "debugText";
        private string _overrideBSCIndexString = "0";
        private string _overrideBSCValueString = "0.0";
        private ARCoreMeshDebugger _meshDebugger;
        private bool _meshDebuggerEnabled = false;
        private Vector3[] _vertices = new Vector3[468];
        private int fontStartSize = 8;
        private bool isVerboseDebug = false;
        private string _datacollectionOff = "Data Collection";
        private string _datacollectionON = "*Data Collection";
        
        private float _cameraBrightness;
        private float _cameraColorTemp;
        private Color _cameraCorrection;
        private Vector3 _lightDirection;

        private string _blendShapeIndexString = "0";
        private string _blendShapeValueString = "-1";
        private bool _dataMarker = false;
        

        protected override void Awake()
        {
            base.Awake();
            _vertices = new Vector3[468];
            if (_blendShapeCoefficients == null)
            {
                _blendShapeCoefficients = new ARCoreBlendShapeCoefficients();
                if (_isDebuggerEnabled)
                {
                    _blendShapeCoefficients.EnableDebugTextMouth = _isDebuggerEnabledMouth;
                    _blendShapeCoefficients.EnableDebugTextEyes = _isDebuggerEnabledEyes;
                    if(_datacollection)
                        _blendShapeCoefficients.ToggleDataCollection();
                }

                _blendShapeCoefficients.RequestPlayerCenterFaceStarted += PlayerNeedsToCenterFaceStart;
                _blendShapeCoefficients.RequestPlayerCenterFaceFinished += OnPlayerNeedsToCenterFaceFinished;
            }
            
            if (_meshDebugger == null)
            {
                _meshDebugger = GameObject.FindObjectOfType<ARCoreMeshDebugger>();
                if (_meshDebugger != null)
                {
                    _meshDebugger.gameObject.SetActive(false);
                    _meshDebuggerEnabled = _meshDebugger.gameObject.activeSelf;  // incase is is active by default
                }
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _blendShapeCoefficients.OnEnable();
#if UNITY_ANDROID             
            _arFaceManager = GameObject.FindObjectOfType<ARFaceManager>();
            _arCameraManager = GameObject.FindObjectOfType<ARCameraManager>();
            if (_arFaceManager != null)
            {
                _arFaceManager.facesChanged += FaceChanged;
            }

            if (_arCameraManager)
            {
                _arCameraManager.frameReceived += CameraFrameUpdated;
            }
#endif
        }
        
        void OnDisable()
        {
            // if we are requesting to show face lets remove that.
            _blendShapeCoefficients.OnDisable();
#if UNITY_ANDROID  
            if (!IsBlendShapeMeshValid()) return;
            ResetFace();
            if (_arFaceManager != null)
            {
                _arFaceManager.facesChanged -= FaceChanged;
            }
            if (_arCameraManager)
            {
                _arCameraManager.frameReceived -= CameraFrameUpdated;
            }
#endif
        }
#if UNITY_ANDROID
[UsedImplicitly]
        private void FaceChanged(ARFacesChangedEventArgs obj)
        {
            var face = obj.updated.FirstOrDefault();
            if (face == null) return;
            Face = face;
        }

        private void CameraFrameUpdated(ARCameraFrameEventArgs args)
        {
            if (args.lightEstimation.averageBrightness.HasValue)
            {
                _cameraBrightness = args.lightEstimation.averageBrightness.Value;
                if (_cameraBrightness < REQUIRED_LIGHTING_BRIGHTNESS_THRESHOLD)
                {
                    PlayerNeedsBetterLightingStarted();
                }
                else
                {
                    OnPlayerNeedsBetterLightingFinished();
                }
            }

            if (args.lightEstimation.mainLightDirection.HasValue)
            {
                _lightDirection = args.lightEstimation.mainLightDirection.Value;
            }

            if (args.lightEstimation.averageColorTemperature.HasValue)
            {
                _cameraColorTemp = args.lightEstimation.averageColorTemperature.Value;               
            }

            if (args.lightEstimation.colorCorrection.HasValue)
            {
                _cameraCorrection = args.lightEstimation.colorCorrection.Value;             
            }
        }
#endif
        private void ResetFace()
        {
            float[] blendShapeValues = _blendShapeCoefficients.GetNeutralScreenFace();
            SetBlendShapes(blendShapeValues);
        }
        
        protected override void UpdateFaceFeatures()
        {
            if (!IsBlendShapeMeshValid()) return;

            bool lastFrameCapturedVerts = false;
            Vector3 faceForward;
            Vector3 facePos = new Vector3();
#if UNITY_IOS
            _vertices = SwiftForUnity.faceVertices;
            Vector4[] centerTransform4X4 = SwiftForUnity.centerTransform4X4;
            lastFrameCapturedVerts = SwiftForUnity.lastFrameCapturedVerts;
            faceForward = centerTransform4X4[2];
#endif
#if UNITY_ANDROID    
            if (Face == null)
            {
                return;
            }
            
            if (Face.vertices.IsCreated && Face.vertices.Length > 0)
            {
                Face.vertices.CopyTo(_vertices); 
                lastFrameCapturedVerts = true;
                faceForward = Face.transform.forward;
                facePos = Face.transform.position;
            }
            else
            {
                return;
            }
#endif
            
            Vector3 cameraPos = _arCameraManager ? _arCameraManager.transform.position : new Vector3();
            Vector3 cameraForward = _arCameraManager ? _arCameraManager.transform.forward : new Vector3();

            float[] blendShapeValues = _blendShapeCoefficients.CalculateBlendShapeCoefficients(_vertices, faceForward,facePos, cameraForward, cameraPos, lastFrameCapturedVerts);
            if (_isDebuggerEnabled)
            {
                _debugText = _blendShapeCoefficients.DebugText;
                if (_meshDebuggerEnabled)
                    UpdateMeshDebugger();
            }
            SetBlendShapes(blendShapeValues);
            base.UpdateFaceFeatures();
        }

        private void SetBlendShapeDebugOverrides()
        {
            _blendShapeCoefficients.SetOverrideValue(_overrideBSCIndexString, _overrideBSCValueString);
        }

        private void ToggleMeshDebugger()
        {
            if (_meshDebugger != null)
            {
                _meshDebuggerEnabled = _meshDebugger.ToggleMeshDebugger();
            }
        }

        private void UpdateMeshDebugger()
        {
            if (_meshDebugger == null)
            {
                _meshDebugger = GameObject.FindObjectOfType<ARCoreMeshDebugger>();
            }

            if (_meshDebugger != null)
            {
#if UNITY_IOS
                Vector3[] vertices = SwiftForUnity.faceVertices;
                _meshDebugger.SetVertices(vertices);
#endif
            }
        }
        
        void OnGUI()
        {
            if (!_isDebuggerEnabled)
                return;
            
            int midFont = (int) ((Screen.height / 1334f * 3) * fontStartSize * 0.7f);
            GUI.skin.label.fontSize = midFont;
            GUI.skin.button.fontSize = midFont;
            GUI.skin.textField.fontSize = midFont;

            int width = (int)(Screen.width * 0.20f);
            int y = 260;
            int height = (int)(Screen.height * 0.03f);
            int nextSpot = height + (int)(Screen.height * 0.003f);

            if (isVerboseDebug)
            {
                Vector3 facePos = Face?Face.transform.position : new Vector3();
                Vector3 cameraPos = _arCameraManager ? _arCameraManager.transform.position : new Vector3();

                string debug = String.Format(
                    " bright:{0:0.0000} colortemp:{1:0.0000} correction:{2:0.0000} faceLocation: {3} faceLocation: {4}\n",
                    _cameraBrightness, _cameraColorTemp, _cameraCorrection, facePos, cameraPos) + _debugText;
                GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f), debug);
            }
            y += nextSpot;
            y += (int) (Screen.height * 0.3);
            
            if(GUI.Button(new Rect(10,y,width,height), "Eyes"))
            {
                _isDebuggerEnabledEyes = !_isDebuggerEnabledEyes;
                _blendShapeCoefficients.EnableDebugTextEyes = _isDebuggerEnabledEyes;
                isVerboseDebug = _isDebuggerEnabledMouth || _isDebuggerEnabledEyes;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "Mouth"))
            {
                _isDebuggerEnabledMouth = !_isDebuggerEnabledMouth;
                _blendShapeCoefficients.EnableDebugTextMouth = _isDebuggerEnabledMouth;
                isVerboseDebug = _isDebuggerEnabledMouth || _isDebuggerEnabledEyes;
            }
            y += nextSpot;

            // DATA COLLECTION
            string data = _datacollectionOff;
            if(_datacollection)
                data = _datacollectionON;
            if(GUI.Button(new Rect(10,y,width,height), data))
            {
                _datacollection = !_datacollection;
                _blendShapeCoefficients.ToggleDataCollection();
            }
            y += nextSpot;
            
            if(GUI.Button(new Rect(10,y,width,height), "ZeroExceptOver"))
            {
                _blendShapeCoefficients.ToggleZeroAllExceptOverrides();
            }
            y += nextSpot;
            
            _blendShapeIndexString = GUI.TextField(new Rect(10, y, width, height), _blendShapeIndexString);
            y += nextSpot;
            _blendShapeValueString = GUI.TextField(new Rect(10, y, width, height), _blendShapeValueString);
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "set blendshape"))
            {
                if (_blendShapeIndexString.Length >= 1 && _blendShapeValueString.Length >= 1)
                {
                    _blendShapeCoefficients.SetOverrideValue(_blendShapeIndexString, _blendShapeValueString);
                }
            }
            y += nextSpot;

            string oncreentext = "";
        /*    oncreentext = _blendShapeCoefficients.ArCoreBlendShapeEyes.GetBlinkV();
            if(GUI.Button(new Rect(10,y,width*2,height), "BlinkV +: " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.IncreaseBlinkV();
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "BlinkV -: " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.DecreaseBlinkV();
            }
            y += nextSpot; */
            if(GUI.Button(new Rect(Screen.width - width ,y ,width,height*2), "Data Mark: " + _dataMarker))
            {
                _dataMarker = !_dataMarker;
            }
            _blendShapeCoefficients.ArCoreBlendShapeEyes.DataMark(_dataMarker);
            // y += nextSpot; keep on same level y
   /*         oncreentext = _blendShapeCoefficients.ArCoreBlendShapeMouth.GetBrowDeadZone();
            if(GUI.Button(new Rect(10,y,width*2,height), "BrowDeadZone + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth.IncreaseDeadZone();
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "BrowDeadZone - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth.DecreaseDeadZone();
            } 
            y += nextSpot; */
    /*        oncreentext = _blendShapeCoefficients.ArCoreBlendShapeEyes.GetFaceHeadingVelocityThreshold();
            if(GUI.Button(new Rect(10,y,width*2,height), "HeadThreshold + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.IncreaseFaceHeadingVelocityThreshold();
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "HeadThreshold - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.DecreaseFaceHeadingVelocityThreshold();
            }
            y += nextSpot;
            oncreentext = _blendShapeCoefficients.ArCoreBlendShapeEyes.GetJawDiffThreshold();
            if(GUI.Button(new Rect(10,y,width*2,height), "JawDiffThreshold + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.IncreaseJawDiffThreshold();
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "JawDiffThreshold - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.DecreaseJawDiffThreshold();
            }
            y += nextSpot; */
    
    /*
            if(GUI.Button(new Rect(10,y,width*2,height), "FPA " + _blendShapeCoefficients.ArCoreBlendShapeMouth._facePositionAdjustment))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._facePositionAdjustment =
                    !_blendShapeCoefficients.ArCoreBlendShapeMouth._facePositionAdjustment;
            }
            y += nextSpot; */
            
            if(GUI.Button(new Rect(10,y,width*2,height), "UseDynamicHeights " + _blendShapeCoefficients.ArCoreBlendShapeEyes.UseDynamicHeights))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.UseDynamicHeights = !_blendShapeCoefficients.ArCoreBlendShapeEyes.UseDynamicHeights;
            }
            y += nextSpot;
    
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes MovAve + " + _blendShapeCoefficients.ArCoreBlendShapeEyes.MovingAverageSize))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.MovingAverageSize += 1;
                _blendShapeCoefficients.ArCoreBlendShapeEyes.ResetMovingAverage();
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes MovAve - " + _blendShapeCoefficients.ArCoreBlendShapeEyes.MovingAverageSize))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.MovingAverageSize -= 1;
                _blendShapeCoefficients.ArCoreBlendShapeEyes.ResetMovingAverage();
            }
            y += nextSpot;
            
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes MouthCountSmoothTime + " + _blendShapeCoefficients.ArCoreBlendShapeEyes.MouthCountSmoothTime))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.MouthCountSmoothTime += 0.01f;
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes MouthCountSmoothTime - " + _blendShapeCoefficients.ArCoreBlendShapeEyes.MouthCountSmoothTime))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.MouthCountSmoothTime -= 0.01f;
            }
            y += nextSpot; 
            
  /*          if(GUI.Button(new Rect(10,y,width*2,height), "Eyes SmileComp + " + _blendShapeCoefficients.ArCoreBlendShapeEyes.SmileCompensatorGain))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.SmileCompensatorGain += 0.0001f;
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes SmileComp - " + _blendShapeCoefficients.ArCoreBlendShapeEyes.SmileCompensatorGain))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.SmileCompensatorGain -= 0.0001f;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes JawComp + " + _blendShapeCoefficients.ArCoreBlendShapeEyes.JawCompensatorGain))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.JawCompensatorGain += 0.0005f;
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width*2,height), "Eyes JawComp - " + _blendShapeCoefficients.ArCoreBlendShapeEyes.JawCompensatorGain))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.JawCompensatorGain -= 0.0005f;
            }
            y += nextSpot;*/
    
            
            
     /*       if(GUI.Button(new Rect(10,y,width*2,height), "_useFiltered Brows " + _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredBrows))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredBrows =
                    !_blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredBrows;
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width*2,height), "filter smile " + _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthSmile))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthSmile =
                    !_blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthSmile;
            }
            y += nextSpot;  
            if(GUI.Button(new Rect(10,y,width*2,height), "filter mouth R/L " + _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthRightLeft))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthRightLeft =
                    !_blendShapeCoefficients.ArCoreBlendShapeMouth._useFilteredMouthRightLeft;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "filter eyes " + _blendShapeCoefficients.ArCoreBlendShapeEyes._useFiltered))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes._useFiltered =
                    !_blendShapeCoefficients.ArCoreBlendShapeEyes._useFiltered;
            }
            y += nextSpot; */
            oncreentext = _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightDown.ToString() + " " + String.Format("{0:0.000000}", _blendShapeCoefficients.ArCoreBlendShapeEyes.lastLow);
            if(GUI.Button(new Rect(10,y,width*2,height), "EyeHeightDown + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightDown += 0.0001f;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "EyeHeightDown - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightDown -= 0.0001f;
            }
            y += nextSpot;
            oncreentext = _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightUp.ToString() + " " + String.Format("{0:0.000000}", _blendShapeCoefficients.ArCoreBlendShapeEyes.lastHigh);
            if(GUI.Button(new Rect(10,y,width*2,height), "EyeHeightUp + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightUp += 0.0001f;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "EyeHeightUp - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeEyes.EyeHeightUp -= 0.0001f;
            }
            y += nextSpot;
            
          /*  oncreentext = _blendShapeCoefficients.ArCoreBlendShapeMouth._CutoffFrequency.ToString();
            if(GUI.Button(new Rect(10,y,width*2,height), "_CutoffFrequency Mouth/Brows + : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._CutoffFrequency += 0.5f;
                _blendShapeCoefficients.ArCoreBlendShapeMouth.ResetLowpass();;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "_CutoffFrequency Mouth/Brows - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._CutoffFrequency -= 0.5f;
                _blendShapeCoefficients.ArCoreBlendShapeMouth.ResetLowpass();;
            }
            y += nextSpot;
            oncreentext = _blendShapeCoefficients.ArCoreBlendShapeMouth._Order.ToString();
            if(GUI.Button(new Rect(10,y,width*2,height), "_Order Mouth/Brows+ : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._Order += 1;
                if (_blendShapeCoefficients.ArCoreBlendShapeMouth._Order > 4)
                    _blendShapeCoefficients.ArCoreBlendShapeMouth._Order = 4;
                _blendShapeCoefficients.ArCoreBlendShapeMouth.ResetLowpass();;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width*2,height), "_Order Mouth/Brows - : " + oncreentext))
            {
                _blendShapeCoefficients.ArCoreBlendShapeMouth._Order -= 1;
                _blendShapeCoefficients.ArCoreBlendShapeMouth.ResetLowpass();;
            }
            y += nextSpot; */
        }
    }
}
