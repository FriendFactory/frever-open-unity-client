using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.AssetChangers;
using Modules.LevelManaging.Editing.Templates;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing
{
    /// <summary>
    /// Loads and apply default body animation for target 
    /// </summary>
    [UsedImplicitly]
    internal sealed class DefaultBodyAnimationForSpawnPositionLoader
    {
        private readonly IBridge _bridge;
        private readonly BodyAnimationChanger _bodyAnimationChanger;
        private readonly IDataFetcher _dataFetcher;
        private readonly ITemplateProvider _templateProvider;

        private long DefaultTemplateId => _dataFetcher.DefaultTemplateId;

        public DefaultBodyAnimationForSpawnPositionLoader(IBridge bridge, BodyAnimationChanger bodyAnimationChanger, IDataFetcher dataFetcher, ITemplateProvider templateProvider)
        {
            _bridge = bridge;
            _bodyAnimationChanger = bodyAnimationChanger;
            _dataFetcher = dataFetcher;
            _templateProvider = templateProvider;
        }

        public async Task ApplyDefaultBodyAnimationForAllCharacters(Event ev)
        {
            var bodyAnimation = await GetDefaultBodyAnimation(ev);

            var loadArgs = new[]
            {
                new BodyAnimLoadArgs
                {
                    CharacterController = ev.CharacterController,
                    BodyAnimation = bodyAnimation
                }
            };
            await _bodyAnimationChanger.Run(ev, loadArgs);
        }

        private async Task<BodyAnimationInfo> GetDefaultBodyAnimation(Event ev)
        {
            BodyAnimationInfo bodyAnimation;
            var defaultBodyAnimId = ev.GetTargetSpawnPosition().DefaultBodyAnimationId;

            if (defaultBodyAnimId.HasValue)
            {
                bodyAnimation = await GetBodyAnimationModel(defaultBodyAnimId.Value);
            }
            else
            {
                var defaultTemplate = await _templateProvider.GetTemplateEvent(DefaultTemplateId);
                bodyAnimation = defaultTemplate.GetTargetCharacterController().GetBodyAnimation();
            }

            return bodyAnimation;
        }

        private async Task<BodyAnimationInfo> GetBodyAnimationModel(long id)
        {
            var response = await _bridge.GetBodyAnimationAsync(id);
            if (response.IsError)
            {
                throw new Exception($"Failed to load default body animation. Reason: {response.ErrorMessage}");
            }

            return response.Model;
        }
    }
}