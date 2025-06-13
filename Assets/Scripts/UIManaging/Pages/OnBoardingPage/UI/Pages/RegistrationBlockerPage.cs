using Common;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class RegistrationBlockerPage : MonoBehaviour
    {
        [SerializeField] private Button _tikTokButton;
        [SerializeField] private Button _instagramButton;
        [SerializeField] private Button _discordButton;
        [SerializeField] private Button _websiteButton;

        private void Awake()
        {
            _tikTokButton.onClick.AddListener(OnTikTokButtonClicked);
            _instagramButton.onClick.AddListener(OnInstagramButtonClicked);
            _discordButton.onClick.AddListener(OnDiscordButtonClicked);
            _websiteButton.onClick.AddListener(OnWebsiteButtonClicked);
        }

        private void OnTikTokButtonClicked()
        {
            Application.OpenURL(Constants.TIKTOK_LINK);
        }
        private void OnInstagramButtonClicked()
        {
            Application.OpenURL(Constants.INSTAGRAM_LINK);
        }
        private void OnDiscordButtonClicked()
        {
            Application.OpenURL(Constants.DISCORD_LINK);
        }

        private void OnWebsiteButtonClicked()
        {
            Application.OpenURL(Constants.WEBPAGE_LINK);
        }
        
    }
}
