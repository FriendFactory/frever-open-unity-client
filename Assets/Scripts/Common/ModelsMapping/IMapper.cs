using Bridge.Models.ClientServer.Level;
using Bridge.Models.ClientServer.Level.Full;
using JetBrains.Annotations;
using Models;

namespace Common.ModelsMapping
{
    public interface IMapper
    {
        Level Map(LevelFullData levelFullData);
        Level Map(LevelShortInfo dto);
        LevelFullInfo Map(Level level);
    }

    [UsedImplicitly]
    internal sealed class Mapper: IMapper
    {
        private readonly LevelToLevelFullInfoMapper _levelToLevelFullInfoMapper;
        private readonly LevelFullDataToLevelMapper _levelFullDataToLevelMapper;

        public Mapper(LevelToLevelFullInfoMapper levelToLevelFullInfoMapper,
                      LevelFullDataToLevelMapper levelFullDataToLevelMapper)
        {
            _levelToLevelFullInfoMapper = levelToLevelFullInfoMapper;
            _levelFullDataToLevelMapper = levelFullDataToLevelMapper;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public Level Map(LevelFullData levelFullData)
        {
            return _levelFullDataToLevelMapper.Map(levelFullData);
        }

        public Level Map(LevelShortInfo dto)
        {
            var output = new Level
            {
                Id = dto.Id, 
                CreatedTime = dto.CreatedTime
            };
            output.Event.Add(new Event
            {
                Id = dto.FirstEventId,
                Files = dto.FirstEventFiles
            });
            return output;
        }

        public LevelFullInfo Map(Level level)
        {
            return _levelToLevelFullInfoMapper.Map(level);
        }
    }
}