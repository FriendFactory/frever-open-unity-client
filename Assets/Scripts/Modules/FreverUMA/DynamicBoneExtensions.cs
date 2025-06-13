using Bridge.Models.ClientServer.Assets;
using Extensions;
using Extensions.Wardrobe;

namespace Modules.FreverUMA
{
    public static class DynamicBoneExtensions
    {
        public static void ApplyPhysicsSettings(this DynamicBone dynamicBone, PhysicsSettings settings)
        {
            dynamicBone.m_Damping = settings.Damping;
            dynamicBone.m_DampingDistrib = settings.DampingDistrib?.ToAnimationCurve();
            dynamicBone.m_Elasticity = settings.Elasticity;
            dynamicBone.m_ElasticityDistrib = settings.ElasticityDistrib?.ToAnimationCurve();
            dynamicBone.m_Stiffness = settings.Stiffness;
            dynamicBone.m_StiffnessDistrib = settings.StiffnessDistrib?.ToAnimationCurve();
            dynamicBone.m_Inert = settings.Inert;
            dynamicBone.m_InertDistrib = settings.InertDistrib?.ToAnimationCurve();
            dynamicBone.m_Radius = settings.Radius;
            dynamicBone.m_RadiusDistrib = settings.RadiusDistrib?.ToAnimationCurve();
            dynamicBone.m_Gravity = settings.Gravity.ToUnityVector3();
            dynamicBone.m_Force = settings.Force.ToUnityVector3();
            dynamicBone.m_EndOffset = settings.EndOffset.ToUnityVector3();
            dynamicBone.UpdateParameters();
        }
    }
}