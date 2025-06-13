using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    /// <summary>
    ///     Vfx related
    /// </summary>
    public static partial class EventExtensions
    {
        public static VfxController GetVfxController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.VfxController?.FirstOrDefault();
        }

        public static void SetVfxController(this Event ev, VfxController controller)
        {
            ThrowExceptionIfEventIsNull(ev);

            if (ev.VfxController == null)
            {
                ev.VfxController = new List<VfxController>();
            }
            else
            {
                ev.VfxController.Clear();
            }

            if (controller == null) return;
            ev.VfxController.Add(controller);
        }

        public static VfxInfo GetVfx(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetVfxController()?.Vfx;
        }

        public static void SetVfx(this Event ev, VfxInfo vfx)
        {
            ThrowExceptionIfEventIsNull(ev);
            ev.GetVfxController().Vfx = vfx;
            ev.GetVfxController().VfxId = vfx.Id;
        }

        public static bool HasTheSameVfx(this Event target, Event compare)
        {
            ThrowExceptionIfEventIsNull(target);

            var targetVfx = target.GetVfx();
            if (targetVfx == null) return false;
            
            var compareVfx = compare?.GetVfx();
            if (compareVfx == null) return false;

            return compareVfx.Id == targetVfx.Id;
        }

        public static bool HasVfx(this Event ev)
        {
            return ev.GetVfxController() != null;
        }

        public static void RemoveVfx(this Event ev)
        {
            ev.VfxController.Clear();
        }
    }
}