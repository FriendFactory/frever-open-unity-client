using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    public sealed class SongSelectedPreviewManager : MonoBehaviour
    {
        [SerializeField] private Button _playPreviewButton;
        [SerializeField] private Button _stopPreviewButton;
        
        private ILevelManager _levelManager;
        private IAssetPlayersProvider _playersProvider;
        private IAudioAssetPlayer _audioAssetPlayer;
        private bool _isPlaying;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, IAssetPlayersProvider playersProvider)
        {
            _levelManager = levelManager;
            _playersProvider = playersProvider;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _playPreviewButton.onClick.AddListener(PlayPreview);
            _stopPreviewButton.onClick.AddListener(StopPreview);

            _playPreviewButton.gameObject.SetActive(true);
            _stopPreviewButton.gameObject.SetActive(false);

            _levelManager.EventDeleted += StopPreview;
        }

        private void Update()
        {
            if (_audioAssetPlayer == null || !_isPlaying) return;

            if (_isPlaying && !_audioAssetPlayer.IsPlaying)
            {
                StopPreview();
            }
        }

        private void OnDestroy()
        {
            StopPreview();

            _playPreviewButton.onClick.RemoveListener(PlayPreview);
            _stopPreviewButton.onClick.RemoveListener(StopPreview);

            _levelManager.EventDeleted -= StopPreview;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void StopPreview()
        {
            if (!_isPlaying) return;

            _audioAssetPlayer.Stop();

            _playPreviewButton.gameObject.SetActive(true);
            _stopPreviewButton.gameObject.SetActive(false);
            _isPlaying = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void PlayPreview()
        {
            var songAsset = _levelManager.GetCurrentAudioAsset();
            var musicStartTime = _levelManager.GetMusicActivationCue().ToSeconds();
            var musicVolume = _levelManager.GetMusicVolume();

            _audioAssetPlayer = _playersProvider.CreateAssetPlayer(songAsset) as IAudioAssetPlayer;
            _audioAssetPlayer?.SetStartTime(musicStartTime);
            _audioAssetPlayer?.SetVolume(musicVolume);
            _audioAssetPlayer?.Play();
            
            _playPreviewButton.gameObject.SetActive(false);
            _stopPreviewButton.gameObject.SetActive(true);
            _isPlaying = true;
        }
    }
}
