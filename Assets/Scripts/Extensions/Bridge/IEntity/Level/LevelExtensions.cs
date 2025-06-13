using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Bridge.Models;
using Models;

namespace Extensions
{
    public static partial class LevelExtensions
    {
        private static readonly IReplaceAlgorithm<Level> IDS_ALGORITHM = new ReplaceIdsIterationAlgorithm();

        public static void ReplaceEmptyIds(this Level entity, Func<string, long> generator)
        {
            IDS_ALGORITHM.ReplaceId(entity, id => id == 0, generator);
        }

        public static Task ReplaceEmptyIdsAsync(this Level entity, Func<string, long> generator)
        {
            return Task.Run(() => ReplaceEmptyIds(entity, generator));
        }

        public static void ResetLocalIds(this Level entity)
        {
            IDS_ALGORITHM.ReplaceId(entity, id => id < 0,s => 0);
        }

        public static Task ResetLocalIdsAsync(this Level entity)
        {
            return Task.Run(() => ResetLocalIds(entity));
        }

        public static Event GetEvent(this Level level, long eventId)
        {
            return level.Event.FirstOrDefault(x => x.Id == eventId);
        }

        public static Event GetEventBefore(this Level level, Event target)
        {
            return level.GetOrderedEvents().TakeWhile(x => x.LevelSequence < target.LevelSequence).LastOrDefault();
        }
        
        public static Event[] GetEventsBefore(this Level level, int levelSequence)
        {
            return level.GetOrderedEvents().TakeWhile(x => x.LevelSequence < levelSequence).ToArray();
        }

        public static Event[] GetEventsBefore(this Level level, Event target)
        {
            return level.GetEventsBefore(target.LevelSequence).ToArray();
        }

        public static Event[] GetEventsAfter(this Level level, Event target)
        {
            return level.GetOrderedEvents().SkipWhile(x => x.LevelSequence <= target.LevelSequence).ToArray();
        }

        public static Event[] GetOrderedEvents(this Level level)
        {
            return level.Event.OrderBy(x => x.LevelSequence).ToArray();
        }

        public static long[] GetUsedTemplates(this Level level)
        {
            if (level == null) throw new ArgumentNullException(nameof(level));
            if (level.Event.IsNullOrEmpty()) return Array.Empty<long>();
            return level.Event.Where(x => x.TemplateId.HasValue).Select(x => x.TemplateId.Value).Distinct().ToArray();
        }

        public static bool IsEmpty(this Level level)
        {
            return level.Event.IsNullOrEmpty();
        }
        
        public static bool IsRemix(this Level level)
        {
            return level.RemixedFromLevelId != null;
        }
        
        public static bool UsesTemplateEvent(this Level level)
        {
            return level.Event != null
                && level.Event.Any(e => e.TemplateId.HasValue && e.TemplateId.Value > 0);
        }

        public static Event GetFirstEvent(this Level level)
        {
            return level.GetOrderedEvents().FirstOrDefault();
        }

        public static Event GetEventBySequenceNumber(this Level level, int sequenceNumber)
        {
            return level.Event.FirstOrDefault(x => x.LevelSequence == sequenceNumber);
        }

        public static Event GetLastEvent(this Level level)
        {
            return level.GetOrderedEvents().LastOrDefault();
        }

        public static void AddEvent(this Level level, Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            if (level.Event == null) level.Event = new List<Event>();
            level.Event.Add(ev);
        }
        
        public static Event GetTemplateEvent(this Level level)
        {
            return level.Event.First();
        }

        public static ICollection<Event> GetEventsWithRefreshedThumbnails(this Level level)
        {
            return level.Event.Where(x => x.Files.Any(f => f.State == FileState.ModifiedLocally)).ToArray();
        }

        public static float GetEventsDurationSec(this Level level)
        {
            if (level.IsEmpty()) return 0;
            return GetEventsDurationSecBeforeEvent(level, level.GetLastEvent().LevelSequence, true);
        }
        
        public static float GetEventsDurationSecBeforeEvent(this Level level, int sequenceNumber, bool include)
        {
            return level.Event.TakeWhile(x=> include && x.LevelSequence <= sequenceNumber || !include && x.LevelSequence < sequenceNumber )
                        .Aggregate(0f, (current, ev) => current + ev.Length).ToSeconds();
        }

        public static void ReplaceCharacters(this Level level, IReadOnlyDictionary<long, CharacterFullInfo> replacingData)
        {
            foreach (var controller in level.AllCharacterControllers())
            {
                var originalId = controller.CharacterId;
                if(!replacingData.ContainsKey(originalId)) continue;
                    
                var replaceCharacter = replacingData[originalId];
                controller.SetCharacter(replaceCharacter);
            }
        }

        public static void RemoveAllOutfits(this Level level)
        {
            foreach (var cc in level.AllCharacterControllers())
            {
                cc.SetOutfit(null);
            }   
        }

        public static void UnlinkTemplates(this Level level)
        {
            foreach (var ev in level.Event)
            {
                ev.TemplateId = null;
            }
        }

        public static long[] GetCharacterIds(this Level level)
        {
            return level.Event.SelectMany(x => x.GetUniqueCharacterIds()).Distinct().ToArray();
        }

        public static IEnumerable<CharacterController> AllCharacterControllers(this Level level)
        {
            return level.Event.SelectMany(x => x.CharacterController);
        }

        public static bool IsSavedOnServer(this Level level)
        {
            return level.Id > 0;
        }

        public static long[] GetUsedLicensedSongsIds(this Level level)
        {
            return level.Event.SelectMany(x => x.MusicController).Where(x => x.ExternalTrackId.HasValue)
                        .Select(x => x.ExternalTrackId.Value).ToArray();
        }

        public static IEnumerable<long> GetExternalSongIds(this Level level)
        {
            return level.Event.Select(x => x.GetMusicController()).Where(x => x != null)
                        .Where(x => x.ExternalTrackId.HasValue).Select(x => x.ExternalTrackId.Value).Distinct();
        }

        public static bool IsVideoMessageBased(this Level level)
        {
            return level.LevelTypeId == ServerConstants.LevelType.VIDEO_MESSAGE;
        }
    }
}