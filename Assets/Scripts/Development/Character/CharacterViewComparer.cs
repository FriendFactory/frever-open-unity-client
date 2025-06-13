#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Bridge;
using Modules.FreverUMA;
using UnityEngine;
using Zenject;
using Modules.FreverUMA.ViewManagement;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace Development.Character
{
    /// <summary>
    /// Allows spawning the character in two ways (UMA and baked mesh) near each other for visual comparison
    /// </summary>
    internal sealed class CharacterViewComparer : MonoBehaviour
    {
        [SerializeField] private float _distanceInterval = 0.8f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private string _textIds;
        [SerializeField] private long[] _characterIds;

        [Inject] private CharacterViewContainer _characterViewContainer;
        [Inject] private IBridge _bridge;
        [Inject] private AvatarHelper _avatarHelper;

        [Button]
        private void ParseIdsFromString()
        {
            if (_textIds == null) return;
            _characterIds = ExtractNumbers(_textIds).ToArray();
        }
        
        [Button]
        private async void LoadTargetCharacters()
        {
            if (_characterIds.IsNullOrEmpty()) return;

            var resp = await _bridge.GetCharactersAdminAccessLevel(_characterIds);
            if (resp.IsError)
            {
                Debug.LogError($"Failed to load characters. {resp.HttpStatusCode} : {resp.ErrorMessage}");
                return;
            }

            var characterModels = resp.Models;
            
            var pos = transform.position;
            for (int i = 0; i < characterModels.Length; i++)
            {
                var characterModel = characterModels[i];
                if (characterModel.BakedViews.IsNullOrEmpty())
                {
                    Debug.LogError($"Character {characterModel.Id} does not have baked view");
                    continue;
                }

                _characterViewContainer.SetOptimizeMemory(false);
                var bakedView = await _characterViewContainer.GetView(characterModel);
                pos += Vector3.left * _distanceInterval;
                bakedView.GameObject.transform.position = pos;
                bakedView.GameObject.transform.rotation = Quaternion.identity;
                var layer = Mathf.RoundToInt(Mathf.Log(_layerMask.value, 2));
                SetLayerRecursively(bakedView.GameObject, layer);
                //change character id data on view for preventing getting cached value when spawning UMA based
                bakedView.GetType().GetField(nameof(bakedView.CharacterId), BindingFlags.Instance|BindingFlags.Public).SetValue(bakedView, -bakedView.CharacterId);

                characterModel.BakedViews = null;
                var umaView = await _characterViewContainer.GetView(characterModel);
                pos += Vector3.left * _distanceInterval;
                umaView.GameObject.transform.position = pos;
                umaView.GameObject.transform.rotation = Quaternion.identity;
                SetLayerRecursively(umaView.GameObject, layer);
                _avatarHelper.UnloadAllUmaBundles();
                await Task.Delay(1000);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layerMask)
        {
            go.layer = layerMask;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layerMask);
            }
        }
        
        private static List<long> ExtractNumbers(string text)
        {
            // Split the text by commas or spaces
            var delimiters = new[] { ',', ' ', '\n' };
            var numberStrings = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            // Convert the split strings to long numbers and store them in a list
            var numbers = new List<long>();
            foreach (var numberString in numberStrings)
            {
                if (long.TryParse(numberString, out var number))
                {
                    numbers.Add(number);
                }
            }

            return numbers;
        }
    }
}
#endif
