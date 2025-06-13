using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Models;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal class BodyAnimationChangingContext
    {
        public Event TargetEvent { get; set; }
        public ICollection<BodyAnimLoadArgs> RequestedChanges { get; set; }
    }
    
    internal class LoadingResult
    {
        public ICollection<IBodyAnimationAsset> LoadedAnimations;
        public Dictionary<BodyAnimationInfo, string> FailedLoadings;
        public bool Cancelled;
    }

    internal enum AlgorithmType
    {
        SingleCharacterAnimation,
        MultipleCharacterAnimation
    }
}