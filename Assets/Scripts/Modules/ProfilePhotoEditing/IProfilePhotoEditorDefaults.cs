using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Template;

namespace Modules.ProfilePhotoEditing
{
    public interface IProfilePhotoEditorDefaults
    {
        long BodyAnimationCategory { get; }
        TemplateInfo GetTemplate();
        Task<BodyAnimationInfo> GetBodyAnimationAsync();
    }
}