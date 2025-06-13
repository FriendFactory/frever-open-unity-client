using UnityEngine;

namespace ARKit
{
    public sealed class EditorBlendShapeVisualizer : NonTrueDepthBlendShapeVisualizer
    {
        private TrackedFaceBlendShapersProvider _blendShapesProvider = new TrackedFaceBlendShapersProvider();
        private bool _useDataFile = true;
        private bool _showOnScreenDebug = false;

        protected override void UpdateFaceFeatures()
        {
            if (!IsBlendShapeMeshValid()) return;
            
            SetBlendShapes(_blendShapesProvider.GetFaceBlendShapes());
            base.UpdateFaceFeatures();
        }

        private sealed class TrackedFaceBlendShapersProvider
        {
            private const float FACE_EXPRESSION_POWER = 10f;
            private const float MAX_BLEND_SHAPE_WEIGHT = 100;

            private float[] _currentFrameFaceBlendShapes;

            private bool _useDataFile = false;
            // lets fake out but with real data:
            private ARCoreBlendShapeMouth _arCoreBlendShapeMouth = new ARCoreBlendShapeMouth();
            private ARCoreBlendShapeEyes _arCoreBlendShapeEyes = new ARCoreBlendShapeEyes();
            private ARCoreFaceVertData _vertsData = new ARCoreFaceVertDataEyesClosed();
            private int _frame = -1;
            private bool _useCenterCosine = true;

            public TrackedFaceBlendShapersProvider()
            {
                _arCoreBlendShapeEyes.EnableDebugText = true;
            }
            public bool UseDataFile
            {
                get { return _useDataFile; }
                set { _useDataFile = value; }
            }
            
            public bool UseCenterCosine
            {
                get { return _useCenterCosine; }
                set { _useCenterCosine = value; }
            }

            public int Frame
            {
                get { return _frame; }
                set
                {
                    _frame = value;
                    if (_frame >= _vertsData.getDataLength())
                    {
                        _frame = _vertsData.getDataLength()-1;
                    }
                    if (_frame < 0)
                    {
                        _frame = 0;
                    }
                }
            }

            public void PlayFrame()
            {
                _frame = -1;
            }

            public void StopFrame()
            {
                _frame = _vertsData.GetCurrentFrameIndex();
            }

            public int GetDataFrame()
            {
                return _vertsData.GetCurrentFrameIndex();
            }

            private float[] CurrentFrameFaceBlendShapes => _currentFrameFaceBlendShapes ?? (_currentFrameFaceBlendShapes = GetStartBlendShapeValues());

            public float[] GetFaceBlendShapes()
            {
                for (var i = 0; i < CurrentFrameFaceBlendShapes.Length; i++)
                {
                    if (_useDataFile)
                    {
                        CurrentFrameFaceBlendShapes[i] = 0.0f; //GetNearValueRandom(CurrentFrameFaceBlendShapes[i]);
                    }
                    else
                    {
                        CurrentFrameFaceBlendShapes[i] = GetNearValueRandom(CurrentFrameFaceBlendShapes[i]);
                    }
                    
                }

                if (_useDataFile)
                {
                    Vector3 faceForward = new Vector3(0f,0f,1f);
                    Vector3 facePos = new Vector3(0f,0f,0f);
                    Vector3 cameraForward = new Vector3(0f,0f,1f);
                    Vector3 cameraPos = new Vector3(0f,0f,0f);

                    float dataCenterTransform = _vertsData.GetCenterTransformZAxisCosineCurrentFrame(_frame);
                    if (_useCenterCosine)
                    {
                        faceForward.z = dataCenterTransform;
                    }
                    _currentFrameFaceBlendShapes = _arCoreBlendShapeMouth.CalculateMouthBlendShapeCoefficients(_vertsData.GetVertsCurrentFrame(), _currentFrameFaceBlendShapes, faceForward, facePos, cameraForward, cameraPos);
                    // override random with real data

                    _currentFrameFaceBlendShapes = _arCoreBlendShapeEyes.CalculateEyeBlendShapeCoefficients(
                        _vertsData.GetVertsCurrentFrame(_frame), _currentFrameFaceBlendShapes, faceForward, facePos, cameraForward, cameraPos);
                }

                return CurrentFrameFaceBlendShapes;
            }

            public float GetCosine()
            {
                return _vertsData.GetCenterTransformZAxisCosineCurrentFrame(_frame);
            }

            public string GetDebugText()
            {
                return _arCoreBlendShapeEyes.DebugText;
            }

            private float GetNearValueRandom(float currentValue)
            {
                var minValue = Mathf.Clamp(currentValue - FACE_EXPRESSION_POWER, 0, MAX_BLEND_SHAPE_WEIGHT);
                var maxValue = Mathf.Clamp(currentValue + FACE_EXPRESSION_POWER, 0, MAX_BLEND_SHAPE_WEIGHT);
                return Random.Range(minValue, maxValue);
            }

            private float[] GetStartBlendShapeValues()
            {
                var currentFrameFaceBlendShapes = new float[BlendShapesConstants.BLENDSHAPE_COUNT];
                for (var i = 0; i < currentFrameFaceBlendShapes.Length; i++)
                {
                    currentFrameFaceBlendShapes[i] = Random.Range(0, MAX_BLEND_SHAPE_WEIGHT);
                }
                return currentFrameFaceBlendShapes;
            }
        }
        void OnGUI()
        {
            if (!_showOnScreenDebug)
                return;
            
            _blendShapesProvider.UseDataFile = _useDataFile;
            if (_useDataFile)
            {
                int fontStartSize = 8;
                //GUI.skin.label.fontSize = debugMulti * fontStartSize;
                int y = 260;
                int height = 75;
                int nextSpot = height + 5;
                
                int largerFont = (int) ((Screen.height / 1334f * 3) * fontStartSize);
                GUI.skin.button.fontSize = largerFont;
                if (GUI.Button(new Rect(10, y, 120, height), "Play"))
                {
                    _blendShapesProvider.PlayFrame();
                    ;
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), "Stop"))
                {
                    _blendShapesProvider.StopFrame();
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), ">"))
                {
                    _blendShapesProvider.Frame++;
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), "<"))
                {
                    _blendShapesProvider.Frame--;
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), ">>"))
                {
                    int frame = _blendShapesProvider.Frame;
                    _blendShapesProvider.Frame = frame + 10;
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), "<<"))
                {
                    int frame = _blendShapesProvider.Frame;
                    _blendShapesProvider.Frame = frame - 10;
                }

                y += nextSpot;
                if (GUI.Button(new Rect(10, y, 120, nextSpot), "UseCenterCosine Toggle"))
                {
                    _blendShapesProvider.UseCenterCosine = !_blendShapesProvider.UseCenterCosine;
                }

                y += nextSpot;
                GUI.Label(new Rect(10, y, 350, nextSpot),
                          "Frame= " + _blendShapesProvider.GetDataFrame() + "  Using Cosine= " +
                          _blendShapesProvider.UseCenterCosine + " Cosine= " + _blendShapesProvider.GetCosine());
                
                GUI.skin.label.fontSize = (int)((Screen.height/1334f * 3)*8);

                int width = (int)(Screen.width * 0.16f);

                GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f), _blendShapesProvider.GetDebugText());
            }
        }
    }
}
