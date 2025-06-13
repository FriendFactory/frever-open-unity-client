using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;
using Zenject;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers.AnimatorTracking
{
    [UsedImplicitly]
    public sealed class AnimatorMonitorProvider
    {
        private readonly Dictionary<long, AnimatorMonitor> _animatorMonitorMap = new();
        
        public bool TryAddMonitor(ICharacterAsset character, AnimatorMonitor animatorMonitor)
        {
            if (!_animatorMonitorMap.TryAdd(character.Id, animatorMonitor)) return false;
            
            character.View.Destroyed += OnCharacterViewDestroyed;
            character.Updated += OnCharacterUpdated;

            return true;

            void OnCharacterViewDestroyed(CharacterView characterView)
            {
                characterView.Destroyed -= OnCharacterViewDestroyed;

                RemoveMonitor(characterView.CharacterId);
            }

            void OnCharacterUpdated()
            {
                character.Updated -= OnCharacterUpdated;

                RemoveMonitor(character.Id);
            }
        }

        public bool RemoveMonitor(long characterId)
        {
            return _animatorMonitorMap.Remove(characterId);
        }

        public bool Contains(long id)
        {
            return _animatorMonitorMap.ContainsKey(id);
        }

        public bool TryGetMonitor(long animationId, out AnimatorMonitor animatorMonitor)
        {
            animatorMonitor = _animatorMonitorMap.Values.FirstOrDefault(monitor => monitor.AnimationId == animationId);
            
            return  animatorMonitor != null;
        }

        public bool TryGetMonitorByCharacterId(long characterId, out AnimatorMonitor animatorMonitor)
        {
            return _animatorMonitorMap.TryGetValue(characterId, out animatorMonitor);
        }
    }
}