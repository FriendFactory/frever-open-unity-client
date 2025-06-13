using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using JetBrains.Annotations;
using UnityEngine;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Extensions;
using Modules.AssetsStoraging.Core;
using Object = UnityEngine.Object;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public interface IMovementTypeThumbnailsProvider
    {
        void FetchMovementTypeThumbnails();
        
        Sprite GetThumbnail(long movementTypeId);

        void Cleanup();
    }
    
    [UsedImplicitly]
    internal sealed class MovementTypeThumbnailsProvider: IMovementTypeThumbnailsProvider
    {
        private readonly IBridge _bridge;
        private readonly IDataFetcher _dataFetcher;
        private readonly Dictionary<long, Sprite> _loadedTextures = new();

        private IEnumerable<MovementType> MovementTypes => _dataFetcher.MetadataStartPack.MovementTypes;

        public MovementTypeThumbnailsProvider(IBridge bridge, IDataFetcher dataFetcher)
        {
            _bridge = bridge;
            _dataFetcher = dataFetcher;
        }

        public async void FetchMovementTypeThumbnails()
        {
            foreach (var movementType in MovementTypes.Where(x=> !x.Files.IsNullOrEmpty()))
            {
                if (_loadedTextures.ContainsKey(movementType.Id) || _bridge.HasCached(movementType, movementType.Files.First()))
                {
                    continue;
                }
                    
                await _bridge.FetchThumbnailAsync(movementType, Resolution._128x128);
            }
        }

        public Sprite GetThumbnail(long movementTypeId)
        {
            if (_loadedTextures.ContainsKey(movementTypeId))
            {
                return _loadedTextures[movementTypeId];
            }

            var model = MovementTypes.First(x => x.Id == movementTypeId);
            if (model.Files.IsNullOrEmpty()) return null;
            const Resolution res = Resolution._128x128;
            if (!_bridge.HasCached(model, model.Files.First(x=>x.Resolution == res)))
            {
                return null;
            }
            var textureResult = _bridge.GetThumbnailFromCacheImmediate(model, res);
            var texture = textureResult.Model;
            if (texture == null) return null;
            
            var sprite = texture.ToSprite();
            _loadedTextures[movementTypeId] = sprite;
            return sprite;
        }

        public void Cleanup()
        {
            foreach (var sprite in _loadedTextures.Values)
            {
                Object.Destroy(sprite.texture);
                Object.Destroy(sprite);
            }
            _loadedTextures.Clear();
        }
    }
}