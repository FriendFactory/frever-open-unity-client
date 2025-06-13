using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal interface IFormation
    {
         long Id { get; }
         void Run(ICharacterAsset[] characterAssets, Transform formationPoint);
    }
}