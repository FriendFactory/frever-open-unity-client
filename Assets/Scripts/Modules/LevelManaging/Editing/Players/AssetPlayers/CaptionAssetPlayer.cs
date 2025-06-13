using Extensions;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class CaptionAssetPlayer : AssetPlayerBase<ICaptionAsset>
    {
        public override void Simulate(float time)
        {
            OnPlay();
        }

        protected override void OnPlay()
        {
            var model = Target.RepresentedModel;
            var captionView = Target.CaptionView;
            captionView.Text = model.Text;
            captionView.SetNormalizedPosition(model.GetNormalizedPosition());
            captionView.SetColor(ColorExtension.HexToColor(model.TextColorRgb));
            captionView.SetAlignment(model.TextAlignment);
            captionView.SetActive(true);
            captionView.ForceRefresh();
        }

        protected override void OnPause()
        {
            Target.CaptionView.SetActive(false);
        }

        protected override void OnResume()
        {
            OnPlay();
        }

        protected override void OnStop()
        {
            Target.CaptionView.SetActive(false);
        }
    }
}