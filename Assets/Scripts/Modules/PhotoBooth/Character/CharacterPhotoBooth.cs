using System.Threading.Tasks;
using Common;
using Modules.CameraCapturing;
using UnityEngine;

namespace Modules.PhotoBooth.Character
{
    [RequireComponent(typeof(CharacterPhotoBoothPresetProvider))]
    public class CharacterPhotoBooth: MonoBehaviour
    {
        [SerializeField] protected Camera _camera;
        [SerializeField] private Transform _light;

        private IPhotoBoothPresetProvider<BodyDisplayMode, CharacterPhotoBoothPreset> _presetProvider;
        
        public int CullingMask
        {
            get => Camera.cullingMask;
            set => Camera.cullingMask = value;
        }

        private Camera Camera => _camera ??= GetComponent<Camera>();

        private void Awake()
        {
            _presetProvider = GetComponentInChildren<IPhotoBoothPresetProvider<BodyDisplayMode, CharacterPhotoBoothPreset>>();
        }
        
        public async Task<Texture2D> GetPhotoAsync(long raceId, BodyDisplayMode mode, Vector3 characterPosition, Quaternion characterRotation, bool changeRenderScale)
        {
            if (_presetProvider == null ||
                !_presetProvider.TryGetPreset(raceId, mode, out var preset))
            {
                Debug.LogError($"[{GetType().Name}] Preset for race {raceId}, mode {mode} not found");
                return null;
            }

            if (!_camera)
            {
                Debug.LogError($"[{GetType().Name}] Camera is not set");
                return null;
            }

            transform.position = characterPosition;
            if (_light)
            {
                _light.rotation = transform.rotation = characterRotation * Quaternion.Euler(Vector3.up * 180);
            }

            ApplyPresetSettings(preset);
            
            _camera.gameObject.SetActive(true);
            
            var photo = await CameraCapture.CaptureIntoTextureAsync(_camera, preset.resolution, changeRenderScale, Constants.ThumbnailSettings.CHARACTER_PHOTO_BOOTH_RENDER_SCALE);
            
            _camera.gameObject.SetActive(false);
            if (_light)
            {
                _light.rotation = Quaternion.identity;
            }

            return photo;
        }
        
        private void ApplyPresetSettings(CharacterPhotoBoothPreset preset)
        {
            _camera.fieldOfView = preset.fieldOfView;
            _camera.transform.SetPositionAndRotation(preset.location.position, preset.location.rotation);
        }
    }
}