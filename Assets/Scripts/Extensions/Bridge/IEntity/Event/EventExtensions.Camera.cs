using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    /// <summary>
    ///     Camera related
    /// </summary>
    public static partial class EventExtensions
    {
        public static CameraFilterController GetCameraFilterController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CameraFilterController.FirstOrDefault();
        }

        public static void SetCameraFilterController(this Event ev, CameraFilterController controller)
        {
            ThrowExceptionIfEventIsNull(ev);
            if (ev.CameraFilterController == null)
            {
                ev.CameraFilterController = new List<CameraFilterController>();
            }
            else
            {
                ev.CameraFilterController.Clear();
            }

            if (controller == null) return;
            ev.CameraFilterController.Add(controller);
        }

        public static CameraFilterInfo GetCameraFilter(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraFilterController()?.CameraFilter;
        }
        
        public static CameraFilterVariantInfo GetCameraFilterVariant(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraFilterController()?.CameraFilterVariant;
        }

        public static void RemoveCameraFilter(this Event ev)
        {
            ev.CameraFilterController.Clear();
        }
        
        public static void SetCameraFilter(this Event ev, CameraFilterInfo cameraFilter, long variantId)
        {
            ThrowExceptionIfEventIsNull(ev);
            var cameraFilterController = ev.GetCameraFilterController();
            if (cameraFilterController == null)
            {
                cameraFilterController = new CameraFilterController();
                ev.CameraFilterController.Add(cameraFilterController);
            }
            cameraFilterController.SetCameraFilterVariant(cameraFilter, variantId);
        }
        
        public static CameraController GetCameraController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.CameraController.FirstOrDefault();
        }
        
        public static void SetCameraController(this Event ev, CameraController controller)
        {
            ThrowExceptionIfEventIsNull(ev);
            if (ev.CameraController == null)
            {
                ev.CameraController = new List<CameraController>();
            }
            else
            {
                ev.CameraController.Clear();
            }
            
            if(controller == null) return;
            ev.CameraController.Add(controller);
        }
        
        public static void SetCameraAnimation(this Event ev, CameraAnimationFullInfo cameraAnimation)
        {
            ThrowExceptionIfEventIsNull(ev);

            var controller = ev.GetCameraController();
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            controller.SetAnimation(cameraAnimation);
        }

        public static CameraAnimationFullInfo GetCameraAnimation(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraController()?.CameraAnimation;
        }
        
        public static long? GetCameraAnimationId(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraController()?.CameraAnimationId;
        }
        
        public static long GetCameraAnimationTemplateId(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraController().CameraAnimationTemplateId;
        }
        
        public static int GetCameraAnimationTemplateSpeed(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraController().TemplateSpeed;
        }

        public static int GetCameraAnimationNoiseIndex(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraController().CameraNoiseSettingsIndex;
        }
        
        public static int? GetCameraFilterValue(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraFilterController().CameraFilterValue;
        }
        
        public static bool HasCameraFilter(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetCameraFilterController() != null;
        }
    }
}