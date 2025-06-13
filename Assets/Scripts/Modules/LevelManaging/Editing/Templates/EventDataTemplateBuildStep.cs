namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class EventDataTemplateBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            Destination.CharacterSpawnPositionId = Template.CharacterSpawnPositionId;
            Destination.CharacterSpawnPositionFormationId = Template.CharacterSpawnPositionFormationId;
            Destination.CharacterSpawnPositionFormation = Template.CharacterSpawnPositionFormation;
            Destination.TargetCharacterSequenceNumber = Template.TargetCharacterSequenceNumber;
        }
    }
}