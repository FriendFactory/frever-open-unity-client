using Bridge.Models.ClientServer.Assets;

namespace Modules.LevelManaging.Editing.Templates
{
    public struct ReplaceCharacterData
    {
        public readonly long OriginCharacterId;
        public readonly CharacterFullInfo ReplaceByCharacter;
        public readonly OutfitFullInfo ChangedOutfit;

        public ReplaceCharacterData(long originCharacterId, CharacterFullInfo replaceByCharacter, OutfitFullInfo outfit = null)
        {
            OriginCharacterId = originCharacterId;
            ReplaceByCharacter = replaceByCharacter;
            ChangedOutfit = outfit;
        }
    }
}