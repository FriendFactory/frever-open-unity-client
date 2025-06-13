using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    public sealed class ThumbnailsHelper
    {
        private readonly IBridge _bridge;
        
        public ThumbnailsHelper(IBridge bridge)
        {
            _bridge = bridge;
        }
        
        public async Task<Dictionary<CharacterInfo, Sprite>> LoadThumbnailsAsync(ICollection<CharacterInfo> stylePresets, CancellationToken token = default)
        {
            var tasks = stylePresets
                .Select(stylePreset => LoadThumbnailForCharacterAsync(stylePreset, token))
                .ToArray();
            
            await Task.WhenAll(tasks);
            
            return tasks.Where(t => t.Result.Value) // Check whether Sprite is valid
                .ToDictionary(t => t.Result.Key, t => t.Result.Value);
        }
        
        private async Task<KeyValuePair<CharacterInfo, Sprite>> LoadThumbnailForCharacterAsync(CharacterInfo character, CancellationToken token)
        {
            var thumbnailFile = character.Files.First(x => x.Resolution == Resolution._512x512);
            var result = await _bridge.GetCharacterThumbnailAsync(character.Id, thumbnailFile, true, token);
            token.ThrowIfCancellationRequested();
            if (result.IsSuccess)
            {
                return CreateThumbnail(character, result.Object);
            }

            Debug.LogWarning(result.ErrorMessage);
            return default;
        }
        
        private KeyValuePair<CharacterInfo, Sprite> CreateThumbnail(CharacterInfo character, object obj)
        {
            if (obj is Texture2D thumbnailTexture)
            {
                var rect = new Rect(0f, 0f, thumbnailTexture.width, thumbnailTexture.height);
                var pivot = new Vector2(.5f, .5f);
                var thumbnailSprite = Sprite.Create(thumbnailTexture, rect, pivot);
                return new KeyValuePair<CharacterInfo, Sprite>(character, thumbnailSprite);
            }

            Debug.LogWarning("Wrong thumbnail format");
            return default;
        }

    }
}