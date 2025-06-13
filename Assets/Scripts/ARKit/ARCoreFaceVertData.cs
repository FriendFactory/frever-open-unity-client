using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARKit
{
    public abstract class ARCoreFaceVertData
    {
        // from collecting data withe debug button
        // on Android then doing:  ./adb logcat > davetest.txt
        //  grep "ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID" davetest.txt | rev | cut -d: -f1 | rev | > EYE_RIGHT_BOTTOM.txt
        //  eyes closed up, down, right left
        private int _currentIndex = 0;
        private int _oppsIndex = 0;
        protected Vector3[] eyeLeftTop;
        protected Vector3[] eyeLeftBottom;
        protected Vector3[] eyeRightTop;
        protected Vector3[] eyeRightBottom;
        protected Vector3[] chinCenter;
        protected Vector3[] lipHigh;
        protected Vector3[] lipLow;
        protected Vector3[] lipMid;
        protected Vector3[] centerTransform;

        public ARCoreFaceVertData()
        {
            SetUpData();
        }

        public abstract void SetUpData();
        public Vector3[] GetVertsCurrentFrame(int frame=-1)
        {
            int index = frame;
            if (index < 0)
            {
                index = _currentIndex;
                _currentIndex++;
                if (_currentIndex >= eyeRightTop.Length || _currentIndex >= eyeRightBottom.Length)
                {
                    _currentIndex = 0;
                }
            }
            else
            {
                _currentIndex = index;
            }
            
            Vector3[] vertices = new Vector3[468];
            vertices[ARCoreFaceMeshConstants.EYE_RIGHT_TOP_LID] = eyeRightTop[index];
            vertices[ARCoreFaceMeshConstants.EYE_RIGHT_BOTTOM_LID] = eyeRightBottom[index];
            vertices[ARCoreFaceMeshConstants.EYE_LEFT_TOP_LID] = eyeLeftTop[index];
            vertices[ARCoreFaceMeshConstants.EYE_LEFT_BOTTOM_LID] = eyeLeftBottom[index];
            vertices[ARCoreFaceMeshConstants.MID_PHILTRUM] = lipMid[index];
            vertices[ARCoreFaceMeshConstants.CHIN_CENTER] = chinCenter[index];
            vertices[ARCoreFaceMeshConstants.LIP_LOW_CENTER_UPPER] = lipLow[index];
            vertices[ARCoreFaceMeshConstants.LIP_HIGH_CENTER_LOWER] = lipHigh[index];            
            
            return vertices;
        }

        public Vector3 GetCenterTransform(int frame=-1)
        {
            int index = frame;
            if (index < 0)
            {
                index = _oppsIndex;
                _oppsIndex++;
                if (_oppsIndex >= centerTransform.Length)
                {
                    _oppsIndex = 0;
                }
            }
            else
            {
                _oppsIndex = index;
            }
            
            return centerTransform[index];
        }

        public int GetCurrentFrameIndex()
        {
            return _currentIndex;
        }

        public int getDataLength()
        {
            return eyeRightTop.Length;
        }

        public float GetCenterTransformZAxisCosineCurrentFrame(int frame=-1)
        {
            return GetCenterTransform(frame).z;
        }
    }

}
