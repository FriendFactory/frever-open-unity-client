using System;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    public static class CharacterControllerBodyAnimationExtensions
    {
        public static void SetBodyAnimation(this CharacterControllerBodyAnimation controller, BodyAnimationInfo bodyAnimation)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (bodyAnimation == null) throw new ArgumentNullException(nameof(bodyAnimation));
            
            controller.BodyAnimation = bodyAnimation;
            controller.BodyAnimationId = bodyAnimation.Id;
        }

        public static BodyAnimationInfo GetBodyAnimation(this CharacterControllerBodyAnimation controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            return controller.BodyAnimation;
        }
    }

    public static class BodyAnimationExtensions
    {
        public static bool IsMultiCharacter(this BodyAnimationInfo bodyAnimation)
        {
            return bodyAnimation.BodyAnimationGroupId.HasValue;
        }
    }
}