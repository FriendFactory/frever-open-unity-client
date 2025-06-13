using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class DuoForwardFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.DuoForward;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position + formationPoint.right * 0.5f;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position - formationPoint.right * 0.5f;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
        }
    }
}
