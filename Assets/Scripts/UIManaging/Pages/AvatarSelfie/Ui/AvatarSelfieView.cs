using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Services.SelfieAvatar;
using DG.Tweening;
using Navigation.Args;
using Navigation.Core;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;
using Gender = Bridge.Models.ClientServer.StartPack.Metadata.Gender;

namespace UIManaging.Pages.AvatarSelfie.Ui
{
    internal sealed class AvatarSelfieView : MonoBehaviour
    {
        private static readonly Vector2Int PHOTO_TARGET_RESOLUTION = new Vector2Int(1080,1440);//iPhone11 photo resolution good handled by ML server

        private static Dictionary<string, int> _genderIdentifierToPredictionGenderId = new()
        {
            { "female", 0 },
            { "male", 1 },
            { "non-binary", 2 }
        };
        
        [SerializeField]private SelfieCaptureButton _selfieCaptureButton;
        [SerializeField]private Button _captureButton;
        [SerializeField]private RawImage _viewport;
        [SerializeField] private Text _genderText;
        [SerializeField] private Image _placeHolderImage;
#if UNITY_EDITOR
        [SerializeField]
        private Vector2Int BaseImageSize = new Vector2Int(2436, 1125);
        
#endif

        [Inject] private ARCameraManager _cameraManager;
        [Inject] private ARFaceManager _faceManager;
        
        private Action<SelfieAvatarResult> _onImageProcessed;
        private PageManager _manager;
        private PopupManager _popupManager;
        private Texture2D _rotatedTexture;
        private Texture2D _cameraOutputTexture;
        private ARFace _trackedFace;
        private IBridge _bridge;
        private float _distance;
        private Gender _gender;
        private readonly float _loadingFadeDuration = 0.5f;
        private Color _startPlaceholderColor;

        public void Init(Action<SelfieAvatarResult> onImageProcessed, PopupManager popupManager, IBridge bridge)
        {
            _popupManager = popupManager;
            _bridge = bridge;
            _onImageProcessed = onImageProcessed;
            _captureButton.onClick.RemoveAllListeners();
            _captureButton.onClick.AddListener(CaptureImage);
        }
        
        public void SetGender(Gender gender)
        {
            _gender = gender;
            _genderText.text = _gender.ToString();
        }

        private void OnEnable()
        {
            StartCapturing();
            _startPlaceholderColor = _placeHolderImage.color;
        }

        private void OnDisable()
        {
            StopCapturing();
            _placeHolderImage.color = _startPlaceholderColor;
            _placeHolderImage.gameObject.SetActive(true);
        }

        private async void CaptureImage()
        {
            StopCapturing();
            var popupConfig = new InformationPopupConfiguration
            {
                Title = "Creating character",
                PopupType = PopupType.Loading
            };
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(PopupType.Loading);

            byte[] imageBytes;
            if (HasValidSize(_cameraOutputTexture))
            {
                imageBytes = _cameraOutputTexture.EncodeToPNG();
            }
            else
            {
                var resized = ResizeImageToServerRequirement();
                imageBytes = resized.EncodeToPNG();
                Destroy(resized);
            }

            var gender = GetSelfie2AvatarGenderIdentifier(_gender.Identifier);
            var resp = await _bridge.GetSelfieJsonAsync(imageBytes, gender, _distance);
            var json = resp.SelfieJson;
            json.predictions.gender = _genderIdentifierToPredictionGenderId[_gender.Identifier];
            _popupManager.ClosePopupByType(PopupType.Loading);
            _onImageProcessed?.Invoke(resp);
        }

        /// <summary>
        /// Temporary fix until ML server works fine only with photos from iPhone11
        /// </summary>

        private bool HasValidSize(Texture2D texture)
        {
            return texture.width == PHOTO_TARGET_RESOLUTION.x && texture.height == PHOTO_TARGET_RESOLUTION.y;
        }
        
        private Texture2D ResizeImageToServerRequirement()
        {
            var resized = new Texture2D(PHOTO_TARGET_RESOLUTION.x, PHOTO_TARGET_RESOLUTION.y, _cameraOutputTexture.format, false);
            RotateTexture();
            CopyPixelsToTheCenter(_rotatedTexture, resized);
            return resized;
        }

        private void CopyPixelsToTheCenter(Texture2D source, Texture2D dest)
        {
            var startPosX = (PHOTO_TARGET_RESOLUTION.x - source.width) / 2;
            var startPosY = (PHOTO_TARGET_RESOLUTION.y - source.height) / 2;
            dest.SetPixels(startPosX, startPosY, source.width, source.height, source.GetPixels());
            dest.Apply();
        }

        public void StartCapturing()
        {
#if !UNITY_EDITOR
            _cameraManager.frameReceived += OnCameraFrameReceived;
            _cameraManager.frameReceived += DisablePlaceHolder;
            _faceManager.facesChanged += OnFaceChanged;
#else
            var fakeSelfie =
                UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Editor/FakeAssets/fake_selfie.jpg",
                    typeof(Texture2D))  as Texture2D;
            _cameraOutputTexture = fakeSelfie;
            _viewport.texture = _cameraOutputTexture;
            _rotatedTexture = new Texture2D(_cameraOutputTexture.height, _cameraOutputTexture.width, _cameraOutputTexture.format, false);
            AdaptCameraViewPortSize();
            
            IEnumerator DisablePlaceHolderDelayed()
            {
                yield return new WaitForSeconds(1.5f);
                DisablePlaceHolder(default);
            }
            
            StartCoroutine(DisablePlaceHolderDelayed());
#endif
        }

        private void StopCapturing()
        {
            _cameraManager.frameReceived -= OnCameraFrameReceived;
            _faceManager.facesChanged -= OnFaceChanged;
        }

        private void DisablePlaceHolder(ARCameraFrameEventArgs eventArgs)
        {
#if !UNITY_EDITOR
            if (!_cameraManager.TryAcquireLatestCpuImage(out _)) return;
#endif
            _cameraManager.frameReceived -= DisablePlaceHolder;
            _placeHolderImage.DOFade(0f, _loadingFadeDuration)
                .OnComplete(() =>
                {
                    _placeHolderImage.gameObject.SetActive(false);
                });
        }
        
        private void AdaptCameraViewPortSize()
        {
            var photoSize = new Vector2(_cameraOutputTexture.width, _cameraOutputTexture.height);
            var canvas = _viewport.transform.root;
            var screenRectSize = canvas.GetComponent<RectTransform>().sizeDelta;

            var photoRatio = photoSize.x / photoSize.y;
            var screenRectRatio = screenRectSize.x / screenRectSize.y;
            
            float multiplier;
            if (photoRatio > screenRectRatio)
                multiplier = screenRectSize.y / photoSize.x;
            else
                multiplier = screenRectSize.x / photoSize.y;

            _viewport.rectTransform.sizeDelta = photoSize * multiplier;
            _viewport.rectTransform.localRotation = Quaternion.Euler(0f,0f,90f);
        }

        private void OnFaceChanged(ARFacesChangedEventArgs eventArgs)
        {
            if (_trackedFace == null || _trackedFace.trackingState != TrackingState.Tracking)
            {
                _trackedFace = eventArgs.updated.FirstOrDefault(face => face.trackingState == TrackingState.Tracking);
            }
            
            _selfieCaptureButton.SetState(_trackedFace != null);
        }

        private unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (!_cameraManager.TryAcquireLatestCpuImage(out var image)) return;

            if (_trackedFace != null)
                _distance = Vector3.Distance(_faceManager.gameObject.transform.position, _trackedFace.transform.position) * 100f;

            // Choose an RGBA format.
            // See XRCameraImage.FormatSupported for a complete list of supported formats.
            var format = TextureFormat.RGBA32;

            if (_cameraOutputTexture == null || _cameraOutputTexture.width != image.width || _cameraOutputTexture.height != image.height)
            {
                _cameraOutputTexture = new Texture2D(image.width, image.height, format, false);
                _rotatedTexture = new Texture2D(image.height, image.width, format, false);
                AdaptCameraViewPortSize();
            }
            
            // Convert the image to format, flipping the image across the Y axis.
            // We can also get a sub rectangle, but we'll get the full image here.
            var conversionParams = new XRCpuImage.ConversionParams(image, format, XRCpuImage.Transformation.MirrorY);

            // Texture2D allows us write directly to the raw texture data
            // This allows us to do the conversion in-place without making any copies.
            var rawTextureData = _cameraOutputTexture.GetRawTextureData<byte>();
            try
            {
                image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
            }
            finally
            {
                // We must dispose of the XRCameraImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }
            
            _cameraOutputTexture.Apply();

            // Set the RawImage's texture so we can visualize it.
            _viewport.texture = _cameraOutputTexture;
        }

        private void RotateTexture()
        {
            var pixels = _cameraOutputTexture.GetPixels32(0);
            var width = _cameraOutputTexture.width;
            var height = _cameraOutputTexture.height;
            var ret = new Color32[width * height];
 
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    var rotated = (i + 1) * height - j - 1;
                    var original = j * width + i;
                    ret[rotated] = pixels[original];
                }
            }
            _rotatedTexture.SetPixels32(ret);
            _rotatedTexture.Apply();
        }

        private void OnDestroy()
        {
            //prevent exception on destroying fake photo in Editor mode
            #if !UNITY_EDITOR
            if(_cameraOutputTexture != null) Destroy(_cameraOutputTexture);
            #endif
            if(_rotatedTexture != null) Destroy(_rotatedTexture);
        }

        private static string GetSelfie2AvatarGenderIdentifier(string genderIdentifier)
        {
            return genderIdentifier switch
            {
                "male" => "Male",
                "female" => "Female",
                "non-binary" => "NonBinary",
                _ => genderIdentifier
            };
        }
    }
}