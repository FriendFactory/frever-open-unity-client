using System.Linq;
using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;

namespace Modules.LevelManaging.Editing.Players.PlayersSetup
{
    internal sealed class VfxPlayerSetup: GenericSetup<IVfxAsset, VfxAssetPlayer>
    {
        protected override void SetupPlayer(VfxAssetPlayer player, Event ev)
        {
            var activationCue = ev.GetVfxController().ActivationCue.ToKilo();

            if (player.TargetAsset is IVfxAsset vfxAsset && vfxAsset.RepresentedModel.BodyAnimationAndVfx is { } vfxBundle)
            {
                var bodyAnimController = ev.GetCharacterBodyAnimationControllersWithAnimationId(vfxBundle.BodyAnimation.Id).FirstOrDefault();
                
                if (bodyAnimController == null) return;
                
                activationCue = bodyAnimController.ActivationCue.ToKilo();
            }

            player.SetStartTime(activationCue);
        }
    }
}