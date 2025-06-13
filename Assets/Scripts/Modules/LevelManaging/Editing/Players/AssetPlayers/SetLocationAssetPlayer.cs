using System.Collections.Generic;
using Bridge.Models.ClientServer.Level.Full;
using Common;
using Extensions;
using Modules.LevelManaging.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class SetLocationAssetPlayer : GenericTimeDependAssetPlayer<ISetLocationAsset>
    {
        private readonly List<SceneAnimationPlayer> _animationPlayers = new List<SceneAnimationPlayer>();
        private bool _playDefaultBackground;
        private PictureInPictureSettings _pictureInPictureSettings;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetPlayDefaultBackground(bool playDefault)
        {
            _playDefaultBackground = playDefault;
        }

        public void SetPictureInPictureSettings(PictureInPictureSettings pictureSettings)
        {
            _pictureInPictureSettings = pictureSettings;
        }

        public override void SetTarget(IAsset target)
        {
            base.SetTarget(target);

            var scene = SceneManager.GetSceneByName(Target.SceneName);
            var rootObjects = scene.GetRootGameObjects();

            foreach (var rootObject in rootObjects)
            {
                var animations = rootObject.GetComponentsInChildren<UnityEngine.Animation>();
                foreach (var animation in animations)
                {
                    var animationPlayer = animation.gameObject.AddOrGetComponent<SceneAnimationPlayer>();
                    _animationPlayers.Add(animationPlayer);
                }
            }
        }

        public override void Simulate(float time)
        {
            SetupPictureInPicture();
        }

        protected override void OnPlay()
        {
            if (Target == null) return;

            foreach (var animationPlayer in _animationPlayers)
            {
                animationPlayer.PlayAtTime(StartTime);
            }

            Target.StopWatch.Start();

            if (_playDefaultBackground)
            {
                Target.MediaPlayer.PlayDefaultVideo();
            }

            SetupPictureInPicture();
        }

        protected override void OnPause()
        {
           if (Target == null) return;

           foreach (var animationPlayer in _animationPlayers)
           {
               animationPlayer.Pause();
           }
           
           if (_playDefaultBackground)
           {
               Target.MediaPlayer.VideoPlayer.Pause();
           }

           Target.StopWatch.Stop();
        }

        protected override void OnResume()
        {
            if (Target == null) return;

            foreach (var animationPlayer in _animationPlayers)
            {
                animationPlayer.Resume();
            }

            Target.StopWatch.Start();
            
            if (_playDefaultBackground)
            {
                Target.MediaPlayer.VideoPlayer.Play();
            }
        }

        protected override void OnStop()
        {
            if (Target == null) return;

            foreach (var animationPlayer in _animationPlayers)
            {
                animationPlayer.Stop();
            }
            
            if (_playDefaultBackground)
            {
                Target.MediaPlayer.VideoPlayer.Stop();
            }

            Target.StopWatch.Reset();
        }
        
        private void SetupPictureInPicture()
        {
            if (Target.PictureInPictureController == null || _pictureInPictureSettings == null) return;
            
            Target.PictureInPictureController.PictureSize = _pictureInPictureSettings.Scale;
            var position = new Vector2
            {
                x = _pictureInPictureSettings.Position.X,
                y = _pictureInPictureSettings.Position.Y
            };

            Target.PictureInPictureController.PictureNormalizedPosition = position;
            Target.PictureInPictureController.Rotation = _pictureInPictureSettings.Rotation;
            Target.Camera.aspect = Constants.VideoRenderingResolution.PORTRAIT_ASPECT;
        }
    }
}