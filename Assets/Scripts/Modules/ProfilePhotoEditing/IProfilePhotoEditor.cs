using System.Threading.Tasks;
using Modules.PhotoBooth.Profile;
using UnityEngine;

namespace Modules.ProfilePhotoEditing
{
    public interface IProfilePhotoEditor
    {
        bool IsReady { get; }
        
        bool EnableCamera { set; }
        
        Task InitializeAsync(ProfilePhotoType photoType);
        Task<Texture2D> GetPhotoAsync();
        void Cleanup();
    }
}