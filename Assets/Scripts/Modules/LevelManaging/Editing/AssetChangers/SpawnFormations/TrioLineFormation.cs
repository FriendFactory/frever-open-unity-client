using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class TrioLineFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.TrioLine;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position + formationPoint.right;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[2].GameObject.transform.position = formationPoint.position - formationPoint.right;
            characterAssets[2].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
        }
    }
}
