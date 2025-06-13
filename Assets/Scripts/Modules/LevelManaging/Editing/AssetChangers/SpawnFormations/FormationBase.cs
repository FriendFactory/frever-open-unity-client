using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal abstract class FormationBase: IFormation
    {
        public long Id => FormationType.GetId();
        protected abstract FormationType FormationType { get; }
        public abstract void Run(ICharacterAsset[] characterAssets, Transform formationPoint);
    }
}