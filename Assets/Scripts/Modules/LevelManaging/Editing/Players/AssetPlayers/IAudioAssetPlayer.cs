namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public interface IAudioAssetPlayer: IAssetPlayer, ITimeDependAssetPlayer
    {
        void SetVolume(float volume);
    }
}