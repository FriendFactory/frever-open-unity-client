using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Template;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Zenject;

namespace Modules.ProfilePhotoEditing
{
    [UsedImplicitly]
    internal sealed class ProfilePhotoEditorDefaults : IProfilePhotoEditorDefaults
    {
        private const string DEFAULT_BODY_ANIMATION_NAME = "Fashion";
        
        private const long BODY_ANIMATION_CATEGORY_ID = 6;

        private readonly IBridge _bridge;
        private readonly IDataFetcher _dataFetcher;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public ProfilePhotoEditorDefaults(IBridge bridge, IDataFetcher dataFetcher)
        {
            _bridge = bridge;
            _dataFetcher = dataFetcher;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public long BodyAnimationCategory => BODY_ANIMATION_CATEGORY_ID;
        
        public TemplateInfo GetTemplate()
        {
            var template = new TemplateInfo()
            {
                Id = _dataFetcher.DefaultTemplateId
            };

            return template;
        }
        
        public async Task<BodyAnimationInfo> GetBodyAnimationAsync()
        {
            //todo: replace by using appropriate api FREV-10156
            var animationResult = await _bridge.GetBodyAnimationListAsync(null, 10, 10, raceId:0, filter: DEFAULT_BODY_ANIMATION_NAME);
            if (animationResult.IsSuccess)
            {
                return animationResult.Models.First(x=>x.Name == DEFAULT_BODY_ANIMATION_NAME);
            }
            throw new Exception($"Failed to load body animation. Reason:{animationResult.ErrorMessage}");
        }
    }
}