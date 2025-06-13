using Bridge.Models.ClientServer.Crews;

namespace UIManaging.PopupSystem.Configurations
{
    public sealed class TransferOwnershipPopupConfiguration : PopupConfiguration
    {
        public long CrewId;
        public CrewMember[] Members;
        public long LocalGroupId;
        public TransferOwnershipPopupConfiguration(long crewId, CrewMember[] members, long localGroupId) : base(PopupType.TransferCrewOwnership, null)
        {
            CrewId = crewId;
            Members = members;
            LocalGroupId = localGroupId;
        }
    }
}