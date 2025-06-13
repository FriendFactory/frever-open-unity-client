using Bridge.Models.ClientServer.Assets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Extensions
{
    public static class CharacterExtensions
    {
        public static CharacterInfo ToCharacterInfo(this CharacterFullInfo original)
        {
            return new CharacterInfo
            { 
                Id = original.Id,
                Access = original.Access,
                Name = original.Name,
                GenderId = original.GenderId,
                GroupId = original.GroupId,
                Files = original.Files
            };
        }

        public static Dictionary<long, CharacterFullInfo> ReplaceByFullInfo(
            this IReadOnlyDictionary<long, CharacterInfo> characters, ICollection<CharacterFullInfo> fullInfos)
        {
            var output = new Dictionary<long, CharacterFullInfo>();
            foreach (var pair in characters)
            {
                output[pair.Key] = fullInfos.First(x => x.Id == pair.Value.Id);
            }
            return output;
        }
    }
}