using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class VfxAssetPlayer : GenericTimeDependAssetPlayer<IVfxAsset>
    {
        private readonly AnimatorMonitorProvider _animatorMonitorProvider;

        public VfxAssetPlayer(AnimatorMonitorProvider animatorMonitorProvider)
        {
            _animatorMonitorProvider = animatorMonitorProvider;
        }
        
        public override void Simulate(float time)
        {
            var vfxBundle = Target.RepresentedModel.BodyAnimationAndVfx;

            if (vfxBundle == null)
            {
                foreach (var component in Target.Components)
                {
                    component.Simulate(StartTime);
                }
            }
            else
            {
                foreach (var component in Target.Components)
                {
                    component.Stop();
                    component.Simulate(StartTime);
                }
            }

            Target.EnableAudio(false);
        }

        public override void SetTarget(IAsset target)
        {
            base.SetTarget(target);

            var asset = target as IVfxAsset;
            
            if (asset == null) return;

            asset.UnloadStarted += OnAssetUnloaded;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            
            Target.UnloadStarted -= OnAssetUnloaded;
            
            RemoveTimeTriggers();
        }

        protected override void OnPlay()
        {
            if (Target == null) return;

            var vfxBundle = Target.RepresentedModel.BodyAnimationAndVfx;
            if (vfxBundle == null)
            {
                foreach (var component in Target.Components)
                {
                    component.Play();
                }
            }
            else
            {
                var animationId = vfxBundle.BodyAnimation.Id;
                if (!_animatorMonitorProvider.TryGetMonitor(animationId, out var animatorMonitor))
                {
                    Debug.LogError($"[{GetType().Name}] AnimatorMonitor for BodyAnimation {animationId} is not found");
                    return;
                }
                
                var startTime = vfxBundle.StartTime / 1000f ?? 0;
                var endTime = vfxBundle.EndTime / 1000f ?? 0;
                
                if (StartTime > 0f)
                {
                    foreach (var component in Target.Components)
                    {
                        var simulationStartTime = StartTime % animatorMonitor.CurrentLength;

                        component.Stop();
                        component.Simulate(simulationStartTime);
                    }
                }
                
                animatorMonitor.AddTimeTrigger(animationId, startTime, PlayVfx);
                animatorMonitor.AddTimeTrigger(animationId, endTime, StopVfx);
            }
            
            Target.EnableAudio(true);
            
            void PlayVfx()
            {
                foreach (var component in Target.Components)
                {
                    component.Play();
                }
            }
            
            void StopVfx()
            {
                foreach (var component in Target.Components)
                {
                    component.Stop();
                }
            }
        }

        protected override void OnResume()
        {
            if (Target == null) return;

            Target.Stopwatch.Start();
            
            foreach (var component in Target.Components)
            {
                component.Resume();
            }
            
            Target.EnableAudio(true);
        }
        
        protected override void OnPause()
        {
            if (Target == null) return;

            foreach (var component in Target.Components)
            {
                component.Pause();
            }

            Target.EnableAudio(false);
            
            Target.Stopwatch.Stop();
        }

        protected override void OnStop()
        {
            Target.EnableAudio(false);
            
            RemoveTimeTriggers();
        }

        private void RemoveTimeTriggers()
        {
            var vfxBundle = Target.RepresentedModel.BodyAnimationAndVfx;

            if (vfxBundle == null) return;

            var animationId = vfxBundle.BodyAnimation.Id;
            
            if (!_animatorMonitorProvider.TryGetMonitor(animationId, out var animatorMonitor)) return;

            animatorMonitor.RemoveTimeTriggers(animationId);
        }

        private void OnAssetUnloaded()
        {
            RemoveTimeTriggers();
        }
    }
}
