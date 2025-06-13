using Modules.CharacterManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    internal sealed class CharacterCarouselModel
    {
        public CharacterInfo[] Characters { get; private set; }
        public IReadOnlyDictionary<CharacterInfo, Texture2D> CharacterThumbnails => _characterThumbnails;

        public Action<CharacterInfo, Texture2D> CharacterThumbnailLoaded;

        private CharacterManager _characterManager;
        private readonly CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        private readonly Dictionary<CharacterInfo, Texture2D> _characterThumbnails = new();

        public CharacterCarouselModel(CharacterManager characterManager, CharacterThumbnailsDownloader characterThumbnailsDownloader, CharacterInfo[] targetCharacters)
        {
            _characterManager = characterManager;
            _characterThumbnailsDownloader = characterThumbnailsDownloader;
            Characters = targetCharacters;
            PrepareCharactersThumbnails();
        }

        public void Dispose()
        { 
            foreach (var kv in _characterThumbnails) 
            {
                UnityEngine.Object.Destroy(kv.Value);
            }
            _characterManager = null;
        }

        public void OnCharacterDeleted(CharacterInfo characterInfo)
        {
            UpdateCharacters(characterInfo.Id);
            _characterThumbnails.Remove(characterInfo);
        }

        private void UpdateCharacters(long removedCharacterId)
        {
            Characters = Characters.Where(x => x.Id != removedCharacterId).ToArray();
            foreach (var character in Characters)
            {
                if (_characterThumbnails.TryAdd(character, null))
                {
                    PrepareCharacterThumbnail(character);
                }
            }
        }

        public bool IsCharacterMain(CharacterInfo characterInfo)
        {
            return _characterManager.RaceMainCharacters.Values.Any(x=> x == characterInfo.Id);
        }

        private void PrepareCharactersThumbnails()
        {
            foreach (var character in Characters) 
            {
                _characterThumbnails.Add(character, null);
                PrepareCharacterThumbnail(character);
            }
        }

        private async void PrepareCharacterThumbnail(CharacterInfo character)
        {
            await _characterThumbnailsDownloader.GetCharacterThumbnail(character, Resolution._512x512, OnSuccess, OnThumbnailTextureFailedToDownload);

            void OnSuccess(Texture2D texture) 
            {
                _characterThumbnails[character] = texture;
                CharacterThumbnailLoaded?.Invoke(character, texture);
            }
        }

        private void OnThumbnailTextureFailedToDownload(string message)
        { 
            Debug.LogWarning(message);
        }
    }
}
    