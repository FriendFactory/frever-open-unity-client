using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class DuoStoryFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.DuoStory;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position + formationPoint.right * 0.5f;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles - new Vector3(0, 90, 0);
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position - formationPoint.right * 0.5f;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles + new Vector3(0, 90, 0);
        }
    }
}
