using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class IPItem: MonoBehaviour
    {
        private const Resolution AVATAR_DEFAULT_RESOLUTION = Resolution._64x64;
        private const Resolution CHARACTER_AVATAR_RESOLUTION = Resolution._128x128;
        private const Resolution ICON_RESOLUTION = Resolution._128x128;
        
        [SerializeField] private RawImage _logoImage;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Button _useButton;
        [SerializeField] private Button _createButton;
        [SerializeField] private AspectRatioFitter _logoAspectRatioFitter;

        [Inject] private IBridge _bridge;
        [Inject] private CharacterManager _characterManager;
        [Inject] private IMetadataProvider _metadataProvider;
        
        private CancellationTokenSource _cancellationSource;
        private Universe _universe;
        private bool _universeLogoSet = false;
        private bool _avatarImageSet = false;

        public event Action UseClicked;
        public event Action CreateClicked;

        private void Awake()
        {
            _useButton.onClick.AddListener(OnUseClicked);
            _createButton.onClick.AddListener(OnCreateClicked);
            _avatarImage.color = _avatarImageSet ? Color.white : Color.clear;
            _logoImage.color = _universeLogoSet ? Color.white : Color.clear;
        }

        private void OnDestroy()
        {
            if (_cancellationSource is null)
            {
                return;
            }
            _cancellationSource.Cancel();
            _cancellationSource = null;

            if (_logoImage.texture)
            {
                Destroy(_logoImage.texture);
            }
            
            if (_avatarImage.sprite)
            {
                Destroy(_avatarImage.sprite);
            }
        }

        public async void Init(Universe universe, Action onUseClicked, Action onCreateClicked)
        {
            _universe = universe;
            UseClicked = onUseClicked;
            CreateClicked = onCreateClicked;
            _avatarImageSet = _universeLogoSet = false;
            
            var raceIdPair = _characterManager.RaceMainCharacters.FirstOrDefault(c => _universe.Races.Any(r => r.RaceId == c.Key));
            if (raceIdPair.Equals(default(KeyValuePair<long, long>)))
            {
                _useButton.gameObject.SetActive(false);
                _createButton.gameObject.SetActive(true);
                await SetDefaultCharacterAvatar();
            }
            else
            {
                _useButton.gameObject.SetActive(true);
                _createButton.gameObject.SetActive(false);
                await SetCharacterAvatar(raceIdPair.Value);
            }

            SetUniverseIcon();
        }

        private async Task SetCharacterAvatar(long characterId)
        {
            var character = _characterManager.UserCharacters.First(character => character.Id == characterId);
            
            var texture = await GetTexture(character, CHARACTER_AVATAR_RESOLUTION);
            if (!texture)
            {
                Debug.LogError($"The texture for the character {character.Name} (ID={characterId}) wasn't found. The requested resolution is {CHARACTER_AVATAR_RESOLUTION}");
                return;
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            _avatarImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
            _avatarImage.color = Color.white;
            _avatarImageSet = true;
        }
        
        private async Task SetDefaultCharacterAvatar()
        {
            var texture = await GetTexture(_universe, AVATAR_DEFAULT_RESOLUTION);
            if (!texture)
            {
                Debug.LogError($"The texture for the universe {_universe.Name} (ID={_universe.Id}) wasn't found. The requested resolution is {AVATAR_DEFAULT_RESOLUTION}");
                return;
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            _avatarImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
            _avatarImage.color = Color.white;
            _avatarImageSet = true;
        }

        private async void SetUniverseIcon()
        {
            var texture = await GetTexture(_universe, ICON_RESOLUTION);
            if (!texture)
            {
                Debug.LogError($"The texture for the universe {_universe.Name} (ID={_universe.Id}) wasn't found. The requested resolution is {ICON_RESOLUTION}");
                return;
            }
            _logoImage.texture = texture;
            _logoAspectRatioFitter.aspectRatio = texture.width / (float)texture.height;
            _logoImage.color = Color.white;
            _universeLogoSet = true;
        }

        private async Task<Texture2D> GetTexture(IThumbnailOwner thumbnailOwner, Resolution resolution)
        {
            var thumbnailFileInfo = thumbnailOwner.Files?.FirstOrDefault(f => f.Resolution == resolution);
            if (thumbnailFileInfo is null)
            {
                Debug.LogError($"The thumbnail for the Character '{(thumbnailOwner as INamed).Name}' (ID={thumbnailOwner.Id} wasn't found");
                return null;
            }
            
            if (_bridge.HasCached(thumbnailOwner, thumbnailFileInfo))
            {
                var res = _bridge.GetThumbnailFromCacheImmediate(thumbnailOwner, resolution);
                if (res.IsSuccess)
                {
                    return res.Model;
                }
            }

            if (_cancellationSource is null)
            {
                _cancellationSource = new CancellationTokenSource();
            }

            var result = await _bridge.GetThumbnailAsync(thumbnailOwner, resolution, true, _cancellationSource.Token);
                
            _cancellationSource = null;
            return result.Object as Texture2D;
        }
        
        private void OnUseClicked()
        {
            UseClicked?.Invoke();
        }
        
        private void OnCreateClicked() {
            CreateClicked?.Invoke();
        }

    }
}