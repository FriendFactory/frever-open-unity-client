namespace Modules.CameraSystem.CameraAnimations.Players
{
    public enum PlayingType
    {
        Loop,
        OneTimeAndStop,
        OneTimeAndKeepPlayingLastFrame,
        BouncingLoop,
        OneTimeBouncingAndStop,//forward and then backward once
    }
}