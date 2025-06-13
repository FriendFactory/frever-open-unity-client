using System;
using Modules.FreverUMA;
using UnityEngine;
using UnityEngine.Serialization;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    [Serializable]
    public struct BlendShapeSetting {
        [FormerlySerializedAs("_bodyPart")]
        public UMABodyPart BodyPart;
        [FormerlySerializedAs("_sprite")]
        public Sprite Sprite;
    }
}