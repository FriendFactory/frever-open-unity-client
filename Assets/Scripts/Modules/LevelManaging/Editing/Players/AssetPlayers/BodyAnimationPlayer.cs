using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking;
using UnityEngine;
using CharacterController = Models.CharacterController;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed partial class BodyAnimationPlayer: GenericTimeDependAssetPlayer<IBodyAnimationAsset>
    {
        private const int POSE_ANIM_CATEGORY_ID = 6;
        private const int CHARACTER_COUNT_FOR_POSITION_ADJUSTMENT = 2;

        private readonly IReadOnlyCollection<MovementType> _movementTypes;
        private readonly AnimatorMonitorProvider _animatorMonitorProvider;

        private AnimationClipOverrides _clipOverrides;
        private AnimatorOverrideController _overrideController;
        private ICharacterAsset[] _characters;
        private int _currentlyActiveCharacterCount;
        private CharacterController[] _controllers;
        private Transform _spawnPositionTransform;
        private readonly AnimationRestarter _animationRestarter;
        private bool _restartEnabled;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private MovementType MovementType
        {
            get
            {
                return Target.RepresentedModel.MovementTypeId.HasValue ? _movementTypes.First(x => x.Id == Target.RepresentedModel.MovementTypeId) : null;
            }
        }
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public BodyAnimationPlayer(IReadOnlyCollection<MovementType> movementTypes, IEventEditor eventEditor, AnimatorMonitorProvider animatorMonitorProvider)
        {
            _movementTypes = movementTypes;
            _animatorMonitorProvider = animatorMonitorProvider;
            _animationRestarter = new AnimationRestarter(eventEditor);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetCharacters(ICharacterAsset[] characters, CharacterController[] controllers)
        {
            if (IsPlaying)
            {
                UpdatePlayedCharacter(characters);
            }
            _characters = characters;
            _controllers = controllers;  
        }

        public void SetSpawnPositionTransform(Transform spawnPosition)
        {
            _spawnPositionTransform = spawnPosition;
        }

        public void SetCurrentlyActiveCharacterCount(int count)
        {
            _currentlyActiveCharacterCount = count;
        }

        public override void Simulate(float time)
        {
            time = (time + StartTime) % Target.BodyAnimation.length;
            var normalizedTime = time / Target.BodyAnimation.length;

            foreach (var character in _characters)
            {
                var animator = character.Animator;
                if (animator == null) continue;

                SetupAnimator(animator, character.GameObject);
                SetupAnimatorMonitor(character);
                animator.Play(Constants.CharacterEditor.START_BODY_ANIM_NAME, 0, normalizedTime);
            }

            ChangePlaybackSpeed(0, _characters);
        }

        public void SetAutoRestarting(bool enabled)
        {
            _restartEnabled = enabled;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _animationRestarter.StopRestarting();
            _animationRestarter.Cleanup();
           
            Target.UnloadStarted -= OnAssetUnloaded;
            
            StopMonitoring();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public override void SetTarget(IAsset target)
        {
            base.SetTarget(target);
            
            var asset = target as IBodyAnimationAsset;
            
            if (asset == null) return;
            
            asset.UnloadStarted += OnAssetUnloaded;
        }

        protected override void OnPlay()
        {
            var time = StartTime % Target.BodyAnimation.length;
            Prepare(_characters);
            PlayFromStartTime(_characters, time);

            if (_restartEnabled && _animationRestarter.SupportsRestarting(Target.RepresentedModel))
            {
                _animationRestarter.SetupRestarting(_characters);
            }

            _characters.ForEach(character =>
            {
                if (Target.RepresentedModel.BodyAnimationAndVfx == null) return;
                
                if (!_animatorMonitorProvider.TryGetMonitorByCharacterId(character.Id, out var monitor)) return;

                var animationId = Target.RepresentedModel.Id;
                
                monitor.StartMonitoring(animationId, StartTime, Target.BodyAnimation.length);
            });
        }

        protected override void OnPause()
        {
            ChangePlaybackSpeed(0, _characters);
        }

        protected override void OnResume()
        {
            ChangePlaybackSpeed(1, _characters);
        }

        protected override void OnStop()
        {
            foreach (var character in _characters)
            {
                var animator = character.Animator;
                if (animator != null) animator.StopPlayback();
            }
            
            StopMonitoring();

            if (_restartEnabled)
            {
                _animationRestarter.StopRestarting();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void ChangePlaybackSpeed(float speed, IEnumerable<ICharacterAsset> characters)
        {
            foreach (var character in characters)
            {
                var animator = character.Animator;
                if (animator == null) continue;

                animator.speed = speed;
            }
        }

        private static void PlayFromStartTime(IEnumerable<ICharacterAsset> characters, float startTime)
        {
            foreach (var character in characters)
            {
                character.Animator.PlayInFixedTime(Constants.CharacterEditor.START_BODY_ANIM_NAME, 0, startTime);
            }
        }

        private void UpdatePlayedCharacter(IEnumerable<ICharacterAsset> nextCharacters)
        {
            var newCharactersToPlay = nextCharacters.Where(x => _characters.All(asset => x.Id != asset.Id)).ToArray();
            if (newCharactersToPlay.Length == 0) return;
            
            Prepare(newCharactersToPlay);
            SyncWithCurrentPlaying(newCharactersToPlay);
        }

        private void SyncWithCurrentPlaying(ICharacterAsset[] newCharacters)
        {
            var oldCharacter = _characters.First(x => !newCharacters.Contains(x));

            foreach (var character in newCharacters)
            {
                oldCharacter.Animator.ApplyStateTo(character.Animator);
            }
        }
        
        private void AdjustPositionForAnimation(ICharacterAsset[] characters, CharacterController[] controllers)
        {
            if (Target.RepresentedModel.BodyAnimationCategoryId != POSE_ANIM_CATEGORY_ID ||
                _currentlyActiveCharacterCount != CHARACTER_COUNT_FOR_POSITION_ADJUSTMENT) return;

            foreach (var controller in controllers)
            {
                var character = characters.FirstOrDefault(x => x.Id == controller.CharacterId);
                if (character == null) continue;
                
                var characterTransform = character.GameObject.transform;
                characterTransform.eulerAngles = _spawnPositionTransform.eulerAngles;

                switch (controller.ControllerSequenceNumber)
                {
                   case 0:
                       characterTransform.position = _spawnPositionTransform.position + _spawnPositionTransform.right * 0.5f;
                       break;
                   case 1:
                       characterTransform.position = _spawnPositionTransform.position - _spawnPositionTransform.right * 0.5f;
                       break;
                }
            }
        }
        
        private void Prepare(ICharacterAsset[] characters)
        {
            foreach (var character in characters)
            {
                SetupAnimator(character.Animator, character.GameObject);
                SetupAnimatorMonitor(character);
            }

            AdjustPositionForAnimation(characters, _controllers);
            SetupCharacterHeelsIgnoring(characters);
            ChangePlaybackSpeed(1, characters);
        }

        private void SetupAnimator(Animator animator, GameObject characterGameObject)
        {
            if (_overrideController == null)
            {
                _overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                _clipOverrides = new AnimationClipOverrides(_overrideController.overridesCount);
                _overrideController.GetOverrides(_clipOverrides);
                _clipOverrides[Constants.CharacterEditor.START_BODY_ANIM_NAME] = Target.BodyAnimation;
                _overrideController.ApplyOverrides(_clipOverrides);
            }
            
            animator.runtimeAnimatorController = _overrideController;
            //work around for wrong displacement of character during changing animation
            var positionBefore = characterGameObject.transform.position;
            animator.Update(0.000001f);//force update
            characterGameObject.transform.position = positionBefore;
        }

        private void SetupAnimatorMonitor(ICharacterAsset character)
        {
            if (_animatorMonitorProvider.Contains(character.Id)) return;
            
            var animator = character.Animator;
            var monitor = animator.gameObject.AddComponent<AnimatorMonitor>();
            
            _animatorMonitorProvider.TryAddMonitor(character, monitor);
        }

        private void SetupCharacterHeelsIgnoring(ICharacterAsset[] characters)
        {
            foreach (var character in characters)
            {
                character.IgnoreHeightHeels = MovementType != null && !MovementType.DependsOnCharacterHeelsHeight;
            }
        }

        private void StopMonitoring()
        {
            if (_characters.IsNullOrEmpty()) return;
            
            foreach (var character in _characters)
            {
                if (_animatorMonitorProvider.TryGetMonitorByCharacterId(character.Id, out var monitor))
                {
                    monitor.StopMonitoring();
                }
            }
        }

        private void OnAssetUnloaded() => StopMonitoring();
    }
}
