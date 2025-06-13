namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AudioPanel
{
    public class VoiceSlider : BaseAudioSlider
    {
        protected override int GetCurrentValue()
        {
            return LevelManager.VoiceVolume;
        }

        public override void ApplyCurrentValue()
        {
            LevelManager.SetVoiceVolume(GeSliderValue());
        }
    }
}
