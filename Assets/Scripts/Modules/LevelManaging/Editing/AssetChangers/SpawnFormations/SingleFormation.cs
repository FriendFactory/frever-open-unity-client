using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class SingleFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.Single;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
        }
    }
}
