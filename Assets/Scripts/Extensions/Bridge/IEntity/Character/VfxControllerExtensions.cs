using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    public static class VfxControllerExtensions
    {
        public static void SetVfx(this VfxController controller, VfxInfo vfx)
        {
            controller.Vfx = vfx;
            controller.VfxId = vfx.Id;
        }
    }
}