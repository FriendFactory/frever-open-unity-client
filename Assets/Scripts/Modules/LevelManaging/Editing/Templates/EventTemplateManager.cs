using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal interface IEventTemplateManager
    {
        Event LastMadeEvent { get; }
        Event CreateFreshEventBasedOnTemplate(Event template, ReplaceCharacterData[] characters);
    }
    
    internal sealed class EventTemplateManager: IEventTemplateManager
    {
        private readonly IReadOnlyCollection<IEventBuildStep> _buildSteps;

        public Event LastMadeEvent { get; private set; }
        
        public EventTemplateManager(IEventBuildStep[] buildSteps)
        {
            _buildSteps = buildSteps;
        }

        public Event CreateFreshEventBasedOnTemplate(Event template, ReplaceCharacterData[] characters)
        {
            characters = GetPreparedReplaceCharacterData(template, characters);
            
            ValidateApplying(template, characters);

            var buildArgs = new BuildStepArgs
            {
                Template = template,
                ReplaceCharactersData = characters,
                TargetEvent = new Event()
            };
            
            foreach (var buildStep in _buildSteps)
            {
                buildStep.Run(buildArgs);
            }

            LastMadeEvent = buildArgs.TargetEvent;
            return LastMadeEvent;
        }

        private void ValidateApplying(Event template, ReplaceCharacterData[] replaceCharacterData)
        {
            var uniqueCharacters = replaceCharacterData.Select(x => x.ReplaceByCharacter).DistinctBy(x => x.Id);
            if (template.GetUniqueCharactersCount() != uniqueCharacters.Count())
                throw new UniqueCharacterMismatchTemplateValidationException();
        }

        private ReplaceCharacterData[] GetPreparedReplaceCharacterData(Event template, ReplaceCharacterData[] characters)
        {
            if (template.TargetCharacterSequenceNumber <= 0) return characters;
            
            // in some cases order of replacement and event characters are not matched
            var orderedCharacters = template.CharacterController
                        .Select(tc => characters.First(rc => tc.CharacterId == rc.OriginCharacterId))
                        .ToArray();

            var firstReplaceData = orderedCharacters[0];
            var targetReplaceData = orderedCharacters[template.TargetCharacterSequenceNumber];

            var firstReplaceCharacter = firstReplaceData.ReplaceByCharacter;
            var targetReplaceCharacter = targetReplaceData.ReplaceByCharacter;

            var firstReplaceDataOverride = new ReplaceCharacterData(firstReplaceData.OriginCharacterId, targetReplaceCharacter, targetReplaceData.ChangedOutfit);
            var targetReplaceDataOverride = new ReplaceCharacterData(targetReplaceData.OriginCharacterId, firstReplaceCharacter, firstReplaceData.ChangedOutfit);

            orderedCharacters[0] = firstReplaceDataOverride;
            orderedCharacters[template.TargetCharacterSequenceNumber] = targetReplaceDataOverride;

            return orderedCharacters;
        }
    }

    internal sealed class UniqueCharacterMismatchTemplateValidationException:InvalidOperationException
    {
        public UniqueCharacterMismatchTemplateValidationException():base("Template and destination events must have the same unique characters count")
        {
        }
    }
}