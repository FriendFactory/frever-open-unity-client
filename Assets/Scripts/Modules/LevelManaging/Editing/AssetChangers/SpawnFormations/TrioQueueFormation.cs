using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.AssetChangers.SpawnFormations
{
    internal sealed class TrioQueueFormation : FormationBase
    {
        protected override FormationType FormationType => FormationType.TrioQueue;

        public override void Run(ICharacterAsset[] characterAssets, Transform formationPoint)
        {
            characterAssets[0].GameObject.transform.position = formationPoint.position;
            characterAssets[0].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[1].GameObject.transform.position = formationPoint.position - formationPoint.forward;
            characterAssets[1].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
                    
            characterAssets[2].GameObject.transform.position = formationPoint.position - formationPoint.forward * 2f;
            characterAssets[2].GameObject.transform.eulerAngles = formationPoint.eulerAngles;
        }
    }
}
