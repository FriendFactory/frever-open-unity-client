using StansAssets.Foundation.Patterns;

namespace Modules.AccountVerification.Events
{
    public sealed class VerificationMethodUpdatedEvent: IEvent
    {
        public VerificationMethodUpdateResult Result { get; }

        public VerificationMethodUpdatedEvent(VerificationMethodUpdateResult result)
        {
            Result = result;
        }
    }
}