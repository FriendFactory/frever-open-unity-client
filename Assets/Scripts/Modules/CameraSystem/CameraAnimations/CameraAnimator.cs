using System;
using JetBrains.Annotations;
using Modules.CameraSystem.CameraAnimations.Players;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CameraSystem.Extensions;
using UnityEngine;
using Zenject;

namespace Modules.CameraSystem.CameraAnimations
{
    [UsedImplicitly]
    internal sealed class CameraAnimator : ILateTickable
    {
        private readonly ICameraController[] _allControllers;
        private float _playbackTime;
        private bool _enabled;
        private int _lastUpdateFrameNumber;
        private int _playedLoopCount;

        private PlayDirection _playDirection = PlayDirection.Forward;
        private PlayingType _playingType;

        private bool _resetPositionOnStop;
        private int _startPlayingFrameNumber;
        private ICameraController _targetController;
        private CameraAnimationClip _clip;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraAnimator(ICameraController[] cameraControllers)
        {
            _allControllers = cameraControllers;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public AnimationType CurrentPlayingAnimationType => _targetController.TargetAnimationType;

        public bool IsPlaying { get; private set; }
        public float PlaybackSpeed { get; private set; }


        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void LateTick()
        {
            if (!_enabled || !IsPlaying) return;

            PlayFrame();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetAnimation(CameraAnimationClip clip)
        {
            _clip = clip;
            SetupCameraController(clip.AnimationType);
        }

        public void SetSpeed(float speed)
        {
            PlaybackSpeed = speed;
        }

        public void SetPlayingType(PlayingType playingType)
        {
            _playingType = playingType;
        }

        public void SetResetAnimationOnStop(bool shouldReset)
        {
            _resetPositionOnStop = shouldReset;
        }

        public void Play(float startTimeInSec = 0)
        {
            IsPlaying = true;
            
            _playDirection = PlayDirection.Forward;
            _playedLoopCount = 0;
            _startPlayingFrameNumber = Time.frameCount;
            _playbackTime = startTimeInSec;

            PlayFrame();
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            if (!IsPlaying) return;

            IsPlaying = false;
            _playbackTime = 0;

            OnAnimationStopped();
        }

        public void SetAnimationOnLastFrame()
        {
            ForceUpdateCameraState(_clip.Length);
        }

        public void SetAnimationOnFirstFrame()
        {
            ForceUpdateCameraState(0);
        }

        public void Simulate(float time)
        {
            var playbackTime = time * PlaybackSpeed;
            var playDirection = GetPlayDirection(playbackTime);
            var clipPosition = GetPositionOnAnimationCurve(playbackTime, playDirection);
            ForceUpdateCameraState(clipPosition);
        }

        private void ForceUpdateCameraState(float clipPosition)
        {
            UpdateCameraState(clipPosition);
            _targetController.ForceUpdate();
        }

        public void Simulate(CameraAnimationFrame frame)
        {
            SetupCameraController(frame.GetAnimationType());
            UpdateCameraSettings(frame);
            _targetController.ForceUpdate();
        }

        public void Enable(bool isOn)
        {
            _enabled = isOn;
            _targetController?.Enable(isOn);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayFrame()
        {
            //preventing 2 times updating, since it is possible to call update from 2 places
            if (_lastUpdateFrameNumber == Time.frameCount) return;
            _lastUpdateFrameNumber = Time.frameCount;

            var deltaTime = GetFrameDeltaTime();

            _playbackTime += deltaTime * PlaybackSpeed;
            
            _playDirection = GetPlayDirection(_playbackTime);
            var animationPosition = GetPositionOnAnimationCurve(_playbackTime, _playDirection);

            UpdateCameraState(animationPosition);
            
            if (CalculateLoopsCount(_playbackTime) > _playedLoopCount)
            {
                _playedLoopCount++;
                HandleAnimationEnd();
            }
        }

        private void OnAnimationStopped()
        {
            if (_resetPositionOnStop) SetAnimationOnFirstFrame();
        }

        private float GetFrameDeltaTime()
        {
            var isFirstFrame = _startPlayingFrameNumber == Time.frameCount;
            if (isFirstFrame) return 0;

            var nextFrameTime = _playbackTime + Time.deltaTime;

            if (nextFrameTime < _clip.Length) return Time.deltaTime;

            if (_playingType == PlayingType.OneTimeAndKeepPlayingLastFrame)
                return 0;

            return Time.deltaTime;
        }

        private void HandleAnimationEnd()
        {
            switch (_playingType)
            {
                case PlayingType.Loop:
                    _playbackTime -= _clip.Length;
                    return;
                case PlayingType.OneTimeAndKeepPlayingLastFrame:
                    return;
                case PlayingType.OneTimeAndStop:
                    _resetPositionOnStop = true;
                    Stop();
                    return;
                case PlayingType.BouncingLoop:
                    break;
                case PlayingType.OneTimeBouncingAndStop:
                    if (_playedLoopCount == 2)
                    {
                        Stop();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_playingType), _playingType, null);
            }
        }

        private void UpdateCameraState(float time)
        {
            foreach (var animationCurve in _clip.AnimationCurves.Keys)
            {
                var frameValue = _clip.GetValueAtTime(animationCurve, time);
                _targetController.Set(animationCurve, frameValue);
            }
        }
        
        private void UpdateCameraSettings(CameraAnimationFrame frame)
        {
            foreach (var prop in frame)
            {
                _targetController.Set(prop.Key, prop.Value);
            }
        }

        private void SetupCameraController(AnimationType targetType)
        {
            foreach (var controller in _allControllers)
            {
                var isTarget = controller.TargetAnimationType == targetType;
                controller.Enable(isTarget);
                if (isTarget) _targetController = controller;
            }
        }

        private float GetPositionOnAnimationCurve(float playbackTime, PlayDirection playDirection)
        {
            var clipPosition = GetPositionOnAnimationCurve(playbackTime);
            return playDirection == PlayDirection.Forward
                ? clipPosition
                : _clip.Length - clipPosition;
        }

        private float GetTimeOnAnimationCurve(float time)
        {
            return Mathf.Abs(time % _clip.Length);
        }

        private float ClampToClipLength(float time)
        {
            return Mathf.Clamp(time, 0, _clip.Length);
        }
        
        
        private PlayDirection GetPlayDirection(float playbackTime)
        {
            switch (_playingType)
            {
                case PlayingType.Loop:
                case PlayingType.OneTimeAndStop:
                case PlayingType.OneTimeAndKeepPlayingLastFrame:
                    return PlayDirection.Forward;
                case PlayingType.BouncingLoop:
                case PlayingType.OneTimeBouncingAndStop:
                    return CalculateLoopsCount(playbackTime) % 2 == 0 ? PlayDirection.Forward : PlayDirection.Backward;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int CalculateLoopsCount(float playbackTime)
        {
            return (int)(playbackTime / _clip.Length);
        }

        private float GetPositionOnAnimationCurve(float playbackTime)
        {
            switch (_playingType)
            {
                case PlayingType.Loop:
                case PlayingType.BouncingLoop:
                case PlayingType.OneTimeBouncingAndStop:
                    return GetTimeOnAnimationCurve(playbackTime);
                default:
                    return ClampToClipLength(playbackTime);
            }
        }
    }
}