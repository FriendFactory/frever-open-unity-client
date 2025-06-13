using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Playing.Core;

namespace Modules.LevelManaging.Assets
{
    public interface IFaceAnimationAsset: IAsset<FaceAnimationFullInfo>
    {
        int FirstFrameCue { get; }
        FaceAnimationClip FaceAnimationClip { get; }
    }

    internal sealed class FaceAnimationAsset : RepresentationAsset<FaceAnimationFullInfo>, IFaceAnimationAsset
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override DbModelType AssetType => DbModelType.FaceAnimation;

        public int FirstFrameCue =>
            //old animations has missed source audio activation cue used during recording, that's why we can use only
            //recorded animation time curve time despite of possible deviation in case of bad ARKit face tracking
            RepresentedModel.MusicStartCue ?? FaceAnimationClip.TimeCurveStartValue.ToMilli();

        public FaceAnimationClip FaceAnimationClip { get; private set; }
        

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(FaceAnimationFullInfo represent, FaceAnimationData faceAnimationData, float duration)
        {
            FaceAnimationClip = new FaceAnimationClip(faceAnimationData);
            FaceAnimationClip.Duration = duration;
            BasicInit(represent);
        }

        public override void CleanUp()
        {
            FaceAnimationClip.Clear();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }
    }
}

