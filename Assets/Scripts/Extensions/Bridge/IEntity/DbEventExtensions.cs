using Models;

namespace Extensions
{
    public static class DbEventExtensions
    {
        public static CharacterController GetTargetCharacterController(this Event @event)
        {
            var sequenceNumber = @event.TargetCharacterSequenceNumber;
            return sequenceNumber < 0 ? @event.GetFirstCharacterController() : @event.GetCharacterController(sequenceNumber);
        }
    }
}
