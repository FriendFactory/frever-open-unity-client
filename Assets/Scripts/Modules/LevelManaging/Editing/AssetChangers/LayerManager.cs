using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    public interface ILayerManager
    {
        LayerMask GetCharacterLayer(int characterLayerIndex);
        LayerMask GetCharacterLayers();
    }
    
    [UsedImplicitly]
    internal sealed class LayerManager: ILayerManager
    {
        private const string CHARACTER_MASK_NAME_PREFIX = "Character";
        private LayerMask? _characterAllMask;

        public LayerMask GetCharacterLayer(int characterLayerIndex)
        {
            return LayerMask.NameToLayer($"{CHARACTER_MASK_NAME_PREFIX}{characterLayerIndex}");
        }

        public LayerMask GetCharacterLayers()
        {
            if (!_characterAllMask.HasValue)
            {
                var layerNames = new List<string>();
                for (var i = 0; i < Common.Constants.CHARACTERS_IN_EVENT_MAX; i++)
                {
                    layerNames.Add($"{CHARACTER_MASK_NAME_PREFIX}{i}");
                }
                _characterAllMask = LayerMask.GetMask(layerNames.ToArray());
            }

            return _characterAllMask.Value;
        }
    }
}