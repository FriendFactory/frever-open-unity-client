namespace Modules.Sound
{
    public interface ISoundManager
    {
        void Play(SoundType soundType, MixerChannel mixerChannel);
        void MuteChannel(bool mute, MixerChannel mixerChannel);
    }
}