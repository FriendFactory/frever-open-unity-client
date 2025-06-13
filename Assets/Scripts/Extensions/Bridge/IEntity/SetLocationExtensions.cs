using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;

namespace Extensions
{
    public static class SetLocationExtensions
    {
        public static ICollection<CharacterSpawnPositionInfo> GetSpawnPositions(this SetLocationFullInfo setLocation)
        {
            if (setLocation == null) throw new InvalidOperationException(nameof(setLocation));
            return setLocation.CharacterSpawnPosition;
        }

        public static IEnumerable<CharacterSpawnPositionInfo> GetSpawnPositionsGroup(this SetLocationFullInfo setLocation, int groupIndex)
        {
            return setLocation.CharacterSpawnPosition.Where(x => x.SpawnPositionGroupId == groupIndex).OrderBy(x=>x.SpawnOrderIndex);
        }
        
        public static CharacterSpawnPositionInfo GetSpawnPosition(this SetLocationFullInfo setLocation, long id)
        {
            if (setLocation == null) throw new InvalidOperationException(nameof(setLocation));
            return setLocation.CharacterSpawnPosition.FirstOrDefault(x=>x.Id == id);
        }
        
        public static CharacterSpawnPositionInfo GetSpawnPosition(this SetLocationFullInfo setLocation, Guid guid)
        {
            if (setLocation == null) throw new InvalidOperationException(nameof(setLocation));
            return setLocation.CharacterSpawnPosition.FirstOrDefault(x=>x.UnityGuid == guid);
        }

        public static CharacterSpawnPositionInfo GetDefaultSpawnPosition(this SetLocationFullInfo setLocation)
        {
            if (setLocation == null) throw new InvalidOperationException(nameof(setLocation));
            var spawnPositions = setLocation.GetSpawnPositions();
            return spawnPositions.FirstOrDefault(spawnPosition => spawnPosition.IsDefault) ?? spawnPositions.First();
        }
    }
}