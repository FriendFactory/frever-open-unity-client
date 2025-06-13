using System.Collections.Generic;
using Common.TimeManaging;
using Modules.FaceAndVoice.Face.Core;
using UMA;
using UnityEngine;

namespace Modules.FaceAndVoice.Face.Playing.Core
{
    [DisallowMultipleComponent]
    public sealed class FaceAnimPlayer : MonoBehaviour
    {
        private SkinnedMeshRenderer _faceMesh;
        private ITimeSource _syncSource;

        private float _playbackTime;
        private float _prevFramePlaybackTime;

        private bool _playRemainingFrames;
        private float[] _remainingFrames;
        private UMAData _umaData;

        private Dictionary<int,string> _cachedBlendShapes;
        private FaceBlendShapeMap _faceBlendShapeMap;
        private bool _initialized;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private FaceAnimationClip Clip { get; set; }
        private bool IsPlaying { get; set; }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void LateUpdate()
        {
            if (!IsPlaying || Clip == null) return;

            UpdatePlaybackTime();
            UpdateBlendShapeValues();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(FaceBlendShapeMap faceBlendShapeMap, SkinnedMeshRenderer faceMesh, UMAData umaData)
        {
            _faceMesh = faceMesh;
            _umaData = umaData;
            _faceBlendShapeMap = faceBlendShapeMap;
            CacheBlendShapes();
            _initialized = true;
        }
        
        public void SetClip(FaceAnimationClip clip)
        {
            Clip = clip;
        }

        public void Play(ITimeSource syncSource)
        {
            _syncSource = syncSource;
            IsPlaying = true;
        }

        public void Pause()
        {
            if (!IsPlaying) return;

            IsPlaying = false;
        }

        public void Resume()
        {
            Debug.LogWarning("Resume for face animation is not implemented");
        }

        public void Stop()
        {
            if (!IsPlaying) return;

            ResetState();
        }

        public void Simulate(float time)
        {
            var blendShapeCount = _cachedBlendShapes.Count;
            
            for (var index = 0; index < blendShapeCount; index++)
            {
                var blendShapeName = _cachedBlendShapes[index];
                UpdateBlendShape(index, blendShapeName, time, _faceMesh);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void UpdatePlaybackTime()
        {
            _playbackTime = _syncSource.ElapsedSeconds;
            //todo:FREV-6864, check why we need that code line. It must prevents playing first face animation frame at the end of event preview
            _playbackTime = _prevFramePlaybackTime > 0 && _playbackTime == 0 ? _prevFramePlaybackTime : _playbackTime;
            _prevFramePlaybackTime = _playbackTime;
        }

        private void UpdateBlendShapeValues(bool reset = false)
        {
            if (_faceMesh == null) return;

            if (reset)
            {
                _playbackTime = 0;
                _prevFramePlaybackTime = 0;
            }
            
            var blendShapeCount = _cachedBlendShapes.Count;

            for (var index = 0; index < blendShapeCount; index++)
            {
                var blendShapeName = _cachedBlendShapes[index];
                UpdateBlendShape(index, blendShapeName ,_playbackTime, reset);
            }
        }
        
        private void UpdateBlendShape(int blendShapeNumber, string blendShapeName, float time, bool reset = false)
        {
            if(!_initialized) return;
            if(!_faceBlendShapeMap.IsFaceBlendShape(blendShapeName)) return;

            var blendValue = 0f;
            var blendShape = _faceBlendShapeMap.GetBlendShapeByName(blendShapeName);
            
            if (!reset && TryGetFrameValue(blendShape, time, out var frameValue))
            {
                blendValue = frameValue;
            }
            else if (_umaData != null && _umaData.blendShapeSettings.blendShapes.TryGetValue(blendShapeName, out var shapeValue))
            {
                blendValue = shapeValue.value * 100f;
            }
            _faceMesh.SetBlendShapeWeight(blendShapeNumber, blendValue);
        }

        private bool TryGetFrameValue(BlendShape blendShape, float time, out float value)
        {
            value = Clip.GetValueAtTime(blendShape, time);
            return value != 0;
        }

        private void CacheBlendShapes()
        {
            var blendShapeCount = _faceMesh.sharedMesh.blendShapeCount;
            _cachedBlendShapes = new Dictionary<int, string>();
            for (var i = 0; i < blendShapeCount; i++)
            {
                var blendShape = _faceMesh.sharedMesh.GetBlendShapeName(i);
                _cachedBlendShapes.Add(i,blendShape);
            }
        }
        
        private void ResetState()
        {
            IsPlaying = false;
            _playbackTime = 0;
            _prevFramePlaybackTime = 0;
        }
    }
}
