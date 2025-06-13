using UnityEngine;
using System;
using Navigation.Core;

namespace TipsManagment
{
    [CreateAssetMenu(fileName = "TipPreset.asset", menuName = "Friend Factory/Tutorial/Tip Preset", order = 4)]
    public class TipPreset : ScriptableObject
    {
        public PageId Page;
        [SerializeReference]
        public GameObject TipPrefab;
        
        public string TargetElementName;
        public bool UseTarget;
        public int ItemIndex;
        public RelativePosition ForcePosition;
        public Vector2 Position;
        public TipSettings Settings = new TipSettings();
    }

    [Serializable]
    public class TipSettings
    {
        public TipId Id;
        [Multiline]
        public string HintText;
        public int HintSequenceOrder;
        public int TriggerWaitTime;
        public int Duration;
        public int PromptAgain;
    }

    public enum TipId
    {
        OnboardingCharacterSelection = 101,
        OnboardingCharacterEditor = 102,
        OnboardingCharacterResult = 103,
        OnboardingCharacterSwitch = 104,
        OnboardingTasks = 105,
        OnboardingSceneryChoice = 106,
        OnboardingMovementChoice = 107,
        OnboardingFeed = 108,
        OnboardingPublish = 109,
        OnboardingAnimationChoice = 110,
        OnboardingGenderSelection = 111,
        OnboardingWelcomeGift = 112,
        OnboardingFeedSwipe = 120,
        LevelEditorFaceTracking = 202,
        LevelEditorMicrophoneToggle = 203,
        QuestLikeVideos = 301,
        QuestRemixVideo = 302,
        QuestJoinStyleChallenge = 303,
        QuestCreateVideo = 304,
        QuestEditProfile = 305,
        QuestClaimSeasonRewards = 306,
        QuestClaimSeasonLikes = 307,
        QuestCreatorScore = 308,
        QuestSecondFrever = 309,
        QuestJoinCrew = 310,
    }
}