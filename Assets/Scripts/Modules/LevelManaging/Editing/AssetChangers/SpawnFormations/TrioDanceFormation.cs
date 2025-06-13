using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class TrioDanceFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.TrioDance;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position + formationPoint.right - formationPoint.forward;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[2].GameObject.transform.position = formationPoint.position - formationPoint.right - formationPoint.forward;
            characterAssets[2].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
        }
    }
}
