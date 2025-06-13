using System;
using UnityEngine;

namespace Modules.PhotoBooth.Character
{
    [Serializable]
    public class CharacterPhotoBoothPreset
    {
        public Vector2Int resolution = new Vector2Int(128, 128);
        public float fieldOfView = 45;
        public Transform location;
    }
}