using Bridge.Models.ClientServer.Assets;

namespace UIManaging.Pages.Feed.Remix.Loaders
{
    internal sealed class DownloadedCharacterModel
    {
        public CharacterInfo CharacterInfo { get; }
        public CharacterCollectionType Category { get; }
        public long Id => CharacterInfo.Id;

        public DownloadedCharacterModel(CharacterInfo characterInfo, CharacterCollectionType category)
        {
            CharacterInfo = characterInfo;
            Category = category;
        }
    }
}