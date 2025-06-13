using Bridge.Models.ClientServer.Level.Full;
using UnityEngine;
using Vector3Dto = Bridge.Models.ClientServer.Assets.Vector3Dto;

namespace Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ToUnityVector3(this Vector3Dto vector3Dto)
        {
            return new Vector3
            {
                x = vector3Dto.X,
                y = vector3Dto.Y,
                z = vector3Dto.Z
            };
        }
        
        public static Vector2Dto ToVector2Dto(this Vector2 vector)
        {
            return new Vector2Dto
            {
                X = vector.x,
                Y = vector.y
            };
        }

        public static Vector2 Abs(this Vector2 vector2)
        {
            return new Vector2(Mathf.Abs(vector2.x), Mathf.Abs(vector2.y));
        }

        public static Vector2Int ToVector2Int(this Vector2 vector2)
        {
            return new Vector2Int((int)vector2.x, (int)vector2.y);
        }
    }
}