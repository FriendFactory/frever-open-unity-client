using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;

namespace Extensions
{
    public static class CharacterSpawnPositionInfoExtensions
    {
        public static bool HasGroup(this CharacterSpawnPositionInfo characterSpawnPositionInfo)
        {
            return characterSpawnPositionInfo.SpawnPositionGroupId.HasValue;
        }
        
        public static int GetGroupId(this CharacterSpawnPositionInfo characterSpawnPositionInfo)
        {
            if (!characterSpawnPositionInfo.HasGroup())
            {
                throw new InvalidOperationException(
                    $"Character spawn position {characterSpawnPositionInfo.Name} has no group");
            }
            return characterSpawnPositionInfo.SpawnPositionGroupId.Value;
        }

        public static bool HasDefaultBodyAnimation(this CharacterSpawnPositionInfo characterSpawnPositionInfo)
        {
            return characterSpawnPositionInfo.DefaultBodyAnimationId.HasValue;
        }

        public static long[] GetAllSupportedMovementTypes(this CharacterSpawnPositionInfo spawnPosition)
        {
            if (spawnPosition.SecondaryMovementTypeIds.IsNullOrEmpty())
            {
                return spawnPosition.MovementTypeId.HasValue ? new[] { spawnPosition.MovementTypeId.Value } : Array.Empty<long>();
            }

            return spawnPosition.SecondaryMovementTypeIds.Append(spawnPosition.MovementTypeId.Value).ToArray();
        }
    }
}