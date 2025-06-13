using System;
using Extensions;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Color = UnityEngine.Color;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    internal sealed class UmaAvatarView : MonoBehaviour
    {
        [SerializeField] private Text _displayText;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _chessboardTexture;
        [SerializeField] private Sprite _defaulBackgroundTexture;
        [SerializeField] private RawImage _image;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RawImage _loadingImage;
        [SerializeField] private GameObject _usingGameObject;
        [SerializeField] private GameObject _loadingSpinner;

        [Inject] private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        
        private bool _wasDestroyed;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public CharacterInfo Character { get; private set; }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Setup(CharacterInfo character, Action<CharacterInfo> onSelect) 
        {
            if (character == null)
            {
                OnThumbnailTextureFailedToDownload("Character is null");
                return;
            }
            
            SetThumbnailLoader();

            _characterThumbnailsDownloader.GetCharacterThumbnail(character, Resolution._512x512, OnThumbnailTextureDownloaded, OnThumbnailTextureFailedToDownload);
            
            Character = character;
            _displayText.text = character.Name;
            
            _button.onClick.AddListener(() => onSelect(character));
        }

        private void OnThumbnailTextureDownloaded(Texture2D texture)
        {
            if (_wasDestroyed) return;

            _loadingSpinner.SetActive(false);
            _image.color = Color.white;
            _image.texture = texture;
            _backgroundImage.gameObject.SetActive(true);
            _loadingImage.gameObject.SetActive(false);
        }
        
        private void OnThumbnailTextureFailedToDownload(string reason)
        {
            if (_wasDestroyed) return;
            
            SetThumbnailLoader();
        }

        private void SetThumbnailLoader()
        {
            _image.color = Color.white.SetAlpha(0f);
            _loadingSpinner.SetActive(true);
            _backgroundImage.gameObject.SetActive(false);
            _loadingImage.gameObject.SetActive(true);
        }
        
        public void SetCharacterUsing(bool value) 
        {
            _usingGameObject.SetActive(value);
        }

        private void OnDestroy()
        {
            _wasDestroyed = true;
            if (_image.texture != null)
            {
                Destroy(_image.texture);
                _image.texture = null;
            }
        }
    }
}
