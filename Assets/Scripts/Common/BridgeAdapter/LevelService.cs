using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Results;
using Common.ModelsMapping;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Common.BridgeAdapter
{
    public interface ILevelService
    {
        Task<LevelResult> GetLevelAsync(long id, CancellationToken token = default);
        void FetchLevelForVideoMessage();
        Task<LevelResult> GetLevelForVideoMessageAsync(CancellationToken token = default);
        Task<LevelResult> GetShuffledLevelAsync(long id, CancellationToken token = default);
        Task<LevelsResult> GetLevelDraftsAsync(int take, int skip,  CancellationToken token = default);
        
        Task<LevelResult> SaveLevelAsync(Level level);
        Task<Result> DeleteLevelAsync(long levelId);
    }

    public sealed class LevelResult: ResultBase
    {
        public Level Level;
    }

    public sealed class LevelsResult : ResultBase
    {
        public Level[] Levels;
    }
    
    [UsedImplicitly]
    internal sealed class LevelService: ILevelService
    {
        private readonly IBridge _bridge;
        private readonly IMapper _mapper;

        private Level _cachedLevelForVideoMessage;

        public LevelService(IBridge bridge, IMapper mapper)
        {
            _bridge = bridge;
            _mapper = mapper;
        }

        public async Task<LevelResult> GetLevelAsync(long id, CancellationToken token = default)
        {
            var result = await _bridge.GetLevel(id, token);
            return ConvertToLevelResult(result);
        }

        public async void FetchLevelForVideoMessage()
        {
            await GetLevelForVideoMessageAsync();
        }

        public async Task<LevelResult> GetLevelForVideoMessageAsync(CancellationToken token = default)
        {
            if (_cachedLevelForVideoMessage != null)
            {
                return new LevelResult
                {
                    IsSuccess = true,
                    Level = await _cachedLevelForVideoMessage.CloneAsync()
                };
            }
            var result = await _bridge.GetLevelTemplateForVideoMessage(token);
            if (result.IsSuccess)
            {
                _cachedLevelForVideoMessage = _mapper.Map(result.Model);
            }
            return ConvertToLevelResult(result);
        }

        public async Task<LevelResult> GetShuffledLevelAsync(long id, CancellationToken token = default)
        {
            var result = await _bridge.GetShuffledLevel(id, token);
            return ConvertToLevelResult(result);
        }

        public async Task<LevelsResult> GetLevelDraftsAsync(int take, int skip, CancellationToken token = default)
        {
            var result = await _bridge.GetLevelDrafts(take, skip, token);
           
            if (result.IsRequestCanceled)
            {
                return new LevelsResult { IsCancelled = true };
            }
            if (result.IsError)
            {
                return new LevelsResult { ErrorMessage = result.ErrorMessage };
            }
            
            return new LevelsResult
            {
                IsSuccess = true,
                Levels = result.Models.Select(_mapper.Map).ToArray()
            };
        }

        public async Task<LevelResult> SaveLevelAsync(Level level)
        {
            var dto = _mapper.Map(level);
            var resp = await _bridge.SaveLevel(dto);
            if (resp.IsError)
            {
                return new LevelResult { ErrorMessage = resp.ErrorMessage };
            }

            return new LevelResult
            {
                IsSuccess = true,
                Level = _mapper.Map(resp.Model)
            };
        }

        public Task<Result> DeleteLevelAsync(long levelId)
        {
            return _bridge.DeleteLevel(levelId);
        }

        private LevelResult ConvertToLevelResult(Result<LevelFullData> bridgeResponse)
        {
            if (bridgeResponse.IsRequestCanceled)
            {
                return new LevelResult { IsCancelled = true };
            }
            if (bridgeResponse.IsError)
            {
                return new LevelResult { ErrorMessage = bridgeResponse.ErrorMessage };
            }

            var mappedLevel = _mapper.Map(bridgeResponse.Model);
            FixCaptionSize(mappedLevel);
            return new LevelResult
            {
                IsSuccess = true,
                Level = mappedLevel
            };
        }

        //workaround to support 1.8 API previous client version, which stores Font in non decimal way
        //todo: drop on 1.9 when we start using float in DTO models ,instead of int multiplied by 1000
        private void FixCaptionSize(Level level)
        {
            var captions = level.Event.Where(x => !x.Caption.IsNullOrEmpty()).SelectMany(x => x.Caption);
            foreach (var caption in captions)
            {
                if (caption.FontSize < 1000)
                {
                    caption.FontSize *= 1000;
                }
            }
        }
    }
}