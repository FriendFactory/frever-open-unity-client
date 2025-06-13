namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AudioPanel
{
    internal sealed class MusicSlider : BaseAudioSlider
    {
        protected override int GetCurrentValue()
        {
            return LevelManager.MusicVolume;
        }

        public override void ApplyCurrentValue()
        {
            LevelManager.SetMusicVolume(GeSliderValue());
        }
    }
}
