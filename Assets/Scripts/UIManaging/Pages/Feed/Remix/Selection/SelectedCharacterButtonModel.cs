using Bridge.Models.ClientServer.Assets;

namespace Modules.VideoStreaming.Remix.Selection
{
    internal class SelectedCharacterButtonModel
    {
        public int Index { get; set; }
        public CharacterInfo CharacterInfo { get; }
        public long Id => CharacterInfo.Id;

        public SelectedCharacterButtonModel(int index, CharacterInfo characterInfo)
        {
            Index = index;
            CharacterInfo = characterInfo;
        }
    }
}