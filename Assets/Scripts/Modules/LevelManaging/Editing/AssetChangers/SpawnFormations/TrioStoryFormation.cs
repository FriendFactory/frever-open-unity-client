using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class TrioStoryFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.TrioStory;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position - formationPoint.forward;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position + formationPoint.right;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles - new Vector3(0, 90, 0);
                    
            characterAssets[2].GameObject.transform.position = formationPoint.position - formationPoint.right;
            characterAssets[2].GameObject.transform.eulerAngles = formationPoint.eulerAngles + new Vector3(0, 90, 0);
        }
    }
}
