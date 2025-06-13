namespace UIManaging.Pages.LevelEditor.Ui.SushiBarComponents
{
    public interface ICameraSwitchingHistory
    {
        void Save(long setLocationId, long cameraIndex);
        long GetCamera(long setLocationIndex);
    }
}
