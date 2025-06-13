using UnityEngine;

namespace Modules.CameraSystem.Extensions.CinemachineExtensions
{
    public static class CinemachineFreeLookUtils 
    {
        /// <summary>
        /// CinemachineFreeLook dutch is a value which represent camera rotation over Z axis(forward axes)
        /// and it has range from -180 to 180, so 181 euler is equal -179 dutch.
        /// To calculate dutch based on Z euler angle we need to convert euler angle Z with infinitive values range 
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float EulerAngleZToDutch(float z)
        {
            var normalizedAngle = z % 360;
            
            if (normalizedAngle == 0) return 0;

            if (Mathf.Abs(normalizedAngle - 180f) < 0.0000001f)
            {
                return 180 * Mathf.Sign(z);
            }

            var amountOf180Cycles = (int) (z / 180);
            var isEvenNumber = amountOf180Cycles % 2 == 0;
            if (isEvenNumber)
            {
                return z % 180;
            }

            return Mathf.Abs(z % 180) - 180;//after 180, value goes in -180 -> 0 direction
        }
    }
}
