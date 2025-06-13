using Bridge.Models.ClientServer.Crews;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
    internal sealed class CrewMemberModel
    {
        public readonly long GroupId;
        public readonly string Nickname;
        public readonly bool IsOnline;
        public Texture2D Portrait { get; private set; }

        public CrewMemberModel(CrewMember crewMember)
        {
            GroupId = crewMember.Group.Id;
            Nickname = crewMember.Group.Nickname;
            IsOnline = crewMember.IsOnline;
        }

        public void SetPortraitTexture(Texture2D portrait)
        {
            Portrait = portrait;
        }
    }
}