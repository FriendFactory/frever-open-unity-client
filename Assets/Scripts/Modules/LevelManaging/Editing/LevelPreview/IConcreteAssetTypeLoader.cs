using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using Models;
using IAsset = Modules.LevelManaging.Assets.IAsset;

namespace Modules.LevelManaging.Editing.LevelPreview
{
    /// <summary>
    /// Loads concrete asset type from event(BodyAnimation, VFX, Character etc)
    /// </summary>
    internal interface IConcreteAssetTypeLoader
    {
        event Action<IAsset[]> Finished;
        bool IsFinished { get; }
        bool HasAssetsToLoad { get; }
        DbModelType Type { get; }
        void Prepare(ICollection<Event> events);
        void Run();
        Task RunAsync();
        void Cancel();
    }
}