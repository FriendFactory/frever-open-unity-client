using Abstract;
using Bridge.Models.ClientServer.Assets;
using Zenject;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    public class OpenUserSoundSettingsButton: BaseContextDataButton<UserSoundFullInfo>
    {
        [Inject] private MusicSelectionStateController _musicSelectionStateController;
        
        protected override void OnInitialized() { }

        protected override void OnUIInteracted()
        {
            var model = new SoundSettingsViewModel(ContextData, null);
            
            _musicSelectionStateController.FireAsync(MusicNavigationCommand.OpenSoundsSettings, model);
        }
    }
}