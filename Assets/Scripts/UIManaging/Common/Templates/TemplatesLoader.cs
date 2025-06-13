using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Template;
using Bridge.Results;
using Modules.Amplitude;
using JetBrains.Annotations;
using Zenject;
using TaskExtensions = Extensions.TaskExtensions;

namespace UIManaging.Common.Templates
{
    [UsedImplicitly]
    public sealed class TemplatesLoader
    {
        public static readonly TemplateCategory PERSONAL_CATEGORY = new TemplateCategory { Id = -1L, Name = "For me" };
        public static readonly TemplateSubCategory ALL_SUB_CATEGORY = new TemplateSubCategory() { Id = -1L, Name = "All" };

        private readonly IBridge _bridge;
        private readonly AmplitudeManager _amplitudeManager;
        private readonly List<TemplateInfo> _personalizedTemplatesCache = new List<TemplateInfo>();
        private bool _isLoadingPersonalTemplates;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public TemplatesLoader(IBridge bridge, AmplitudeManager amplitudeManager)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<TemplateInfo[]> DownloadTemplates(
            int skip, int take, int? characterCount,
            string nameFilter,
            CancellationToken cancellationToken = default)
        {
            ArrayResult<TemplateInfo> result;

            if (string.IsNullOrWhiteSpace(nameFilter))
            {
                result = await _bridge.GetPersonalEventTemplates(take, skip, _amplitudeManager.MlExperimentVariantsHeader, 
                                                                 cancellationToken);
            }
            else
            {
                var categoryId = PERSONAL_CATEGORY.Id;
                var subCategoryId = ALL_SUB_CATEGORY.Id;
                var filter = nameFilter.Equals(string.Empty) ? null : nameFilter;

                result = await _bridge.GetEventTemplates
                (
                    take, skip,
                    categoryId, subCategoryId,
                    characterCount, filter,
                    cancellationToken
                );
            }

            if (result.IsSuccess)
            {
                return result.Models;
            }

            if (result.IsError)
            {
                Debug.LogError($"Failed to download templates. Reason: {result.ErrorMessage}");
            }

            return null;
        }

        public async Task<TemplateInfo[]> GetPersonalisedTemplates(int skip, int take,
            CancellationToken cancellationToken = default)
        {
            if (_isLoadingPersonalTemplates)
            {
                await TaskExtensions.DelayWithoutThrowingCancellingException(30, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) return null;
            
            var hasAllInCache = _personalizedTemplatesCache.Count >= skip + take;
            if (hasAllInCache)
            {
                return _personalizedTemplatesCache.Skip(skip).Take(take).ToArray();
            }

            var isFirstPageRequest = skip == 0;
            if (isFirstPageRequest && _personalizedTemplatesCache.Any())
            {
                return _personalizedTemplatesCache.ToArray();
            }
            
            var output = new List<TemplateInfo>();
            if (_personalizedTemplatesCache.Count > skip)
            {
                output.AddRange(_personalizedTemplatesCache.Skip(skip));
            }

            skip += output.Count;
            take -= output.Count;

            _isLoadingPersonalTemplates = true;
            
            var result = await _bridge.GetPersonalEventTemplates
            (
                take, skip, _amplitudeManager.MlExperimentVariantsHeader,
                cancellationToken
            );

            if (result.IsSuccess)
            {
                _personalizedTemplatesCache.AddRange(result.Models);
                _isLoadingPersonalTemplates = false;
                return result.Models;
            }

            _isLoadingPersonalTemplates = false;
            if (result.IsError)
            {
                if (result.ErrorMessage.Contains(":403")) return null;
                
                Debug.LogError($"Failed to download templates. Reason: {result.ErrorMessage}");
            }

            return null;
        }

        public async void FetchPersonalisedTemplates(int count)
        {
            await GetPersonalisedTemplates(0, count);
        }
    }
}