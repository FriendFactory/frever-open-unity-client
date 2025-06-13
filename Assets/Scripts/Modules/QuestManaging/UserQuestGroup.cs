using System.Collections.Generic;
using Bridge.Models.ClientServer.Onboarding;
using Bridge.Models.ClientServer.UserActivity;

namespace Modules.QuestManaging
{
    public struct UserQuestGroup
    {
        public OnboardingQuestGroup QuestGroup;
        public Dictionary<string, bool> QuestActive;
        public Dictionary<long, OnboardingQuestProgress> QuestProgress;
    }
}