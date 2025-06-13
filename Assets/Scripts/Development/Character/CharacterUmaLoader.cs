#if UNITY_EDITOR
using System;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.FreverUMA.ViewManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Development.Character
{
    /// <summary>
    /// Use it if you need to test any character built with UMA
    /// </summary>
    internal sealed class CharacterUmaLoader: MonoBehaviour
    {
        [SerializeField] private long _characterId;

        [Inject] private IBridge _bridge;
        [Inject] private CharacterViewContainer _characterViewContainer;
        
        [Button]
        public async void Load()
        {
            var resp = await _bridge.GetCharactersAdminAccessLevel(new []{_characterId});
            if (resp.IsError || resp.Models.Length != 1)
            {
                Debug.LogError($"Failed to load characters. {resp.HttpStatusCode} : {resp.ErrorMessage}");
                return;
            }

            var character = resp.Models.First();
            character.BakedViews = Array.Empty<BakedView>();

            var view = await _characterViewContainer.GetView(character);
            view.GameObject.transform.position = transform.position;
            view.GameObject.transform.rotation = Quaternion.identity;
        }
        
        private static void SetLayerRecursively(GameObject go, int layerMask)
        {
            go.layer = layerMask;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layerMask);
            }
        }
    }
}
#endif