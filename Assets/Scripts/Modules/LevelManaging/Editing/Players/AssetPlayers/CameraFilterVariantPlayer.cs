using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class CameraFilterVariantPlayer: AssetPlayerBase<ICameraFilterVariantAsset>
    {
        public override void Simulate(float time)
        {
            Target.SetActive(true);
        }

        protected override void OnPlay()
        {
            Target.SetActive(true);
        }

        protected override void OnPause()
        {
        }

        protected override void OnResume()
        {
            Target.SetActive(true);
        }

        protected override void OnStop()
        {
            Target.SetActive(false);
        }
    }
}