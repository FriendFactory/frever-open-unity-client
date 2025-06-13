using System.Collections;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Common;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    /// <summary>
    /// Restarts locomotion body animations on initial position after constant interval
    /// </summary>
    internal sealed partial class BodyAnimationPlayer
    {
        private sealed class AnimationRestarter
        {
            private const float RESTARTING_INTERVAL = 2.5f;
            private const long LOCOMOTION_MOVEMENT_TYPE_ID = 1;
            private readonly IEventEditor _eventEditor;
            
            private Coroutine _runningCoroutine;
            private ICharacterAsset[] _characters;
            private readonly Dictionary<long, Vector3> _initialCharacterPositions = new Dictionary<long, Vector3>();
            private readonly Dictionary<long, Quaternion> _initialCharacterRotations = new Dictionary<long, Quaternion>();

            //---------------------------------------------------------------------
            // Properties
            //---------------------------------------------------------------------
            
            private bool IsRunning => _runningCoroutine != null;

            //---------------------------------------------------------------------
            // Ctors
            //---------------------------------------------------------------------
            
            public AnimationRestarter(IEventEditor eventEditor)
            {
                _eventEditor = eventEditor;
                _eventEditor.SpawnFormationChanged += OnSpawnFormationChanged;
            }

            //---------------------------------------------------------------------
            // Public
            //---------------------------------------------------------------------
            
            public bool SupportsRestarting(BodyAnimationInfo bodyAnimationInfo)
            {
                return bodyAnimationInfo.MovementTypeId == LOCOMOTION_MOVEMENT_TYPE_ID;
            }

            public void SetupRestarting(ICharacterAsset[] characters)
            {
                if (IsRunning)
                {
                    StopRestarting();
                }
                
                _characters = characters;
                StoreInitialPositions();
                RunAutoRestart();
            }

            public void StopRestarting()
            {
                if (!IsRunning) return;
                StopAutoRestart();
            }

            public void Cleanup()
            {
                _eventEditor.SpawnFormationChanged -= OnSpawnFormationChanged;
            }

            //---------------------------------------------------------------------
            // Helpers
            //---------------------------------------------------------------------
            
            private void StoreInitialPositions()
            {
                _initialCharacterPositions.Clear();
                _initialCharacterRotations.Clear();

                foreach (var characterAsset in _characters)
                {
                    var transform = characterAsset.GameObject.transform;
                    _initialCharacterPositions[characterAsset.Id] = transform.localPosition;
                    _initialCharacterRotations[characterAsset.Id] = transform.localRotation;
                }
            }
            
            private void RunAutoRestart()
            {
                _runningCoroutine = CoroutineSource.Instance.StartCoroutine(Update());
            }
            
            private void StopAutoRestart()
            {
                CoroutineSource.Instance.StopCoroutine(_runningCoroutine);
            }
            
            private IEnumerator Update()
            {
                while (true)
                {
                    yield return new WaitForSeconds(RESTARTING_INTERVAL);
                    RestartCharacter();
                }
            }

            private void RestartCharacter()
            {
                PutCharactersOnInitialPosition();
            }

            private void PutCharactersOnInitialPosition()
            {
                foreach (var character in _characters)
                {
                    var transform = character.GameObject.transform;
                    transform.localPosition = _initialCharacterPositions[character.Id];
                    transform.localRotation = _initialCharacterRotations[character.Id];
                }
            }
            
            private void OnSpawnFormationChanged(long? spawnFormationId)
            {
                if (!IsRunning) return;
                StoreInitialPositions();
                StopRestarting();
                RunAutoRestart();
            }
        }
    }
}