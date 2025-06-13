using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;

namespace Extensions
{
    public static class EditorMetadataExtensions
    {
        public static BodyAnimationInfo[] GetAllDressUpAnimations(this EditorMetadata metadata)
        {
            var mainCharacterAnim = metadata.CharacterEditingAnimation;
            var backgroundCharactersAnim = metadata.BackgroundCharacterAnimations;
            return backgroundCharactersAnim.Append(mainCharacterAnim).DistinctBy(x=>x.Id).ToArray();
        }
    }
}