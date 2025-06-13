using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.TimeManaging;
using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Playing.Core;
using UnityEngine;
using Zenject;
using static Common.Constants.FileDefaultPaths;

namespace Modules.FaceAndVoice.Face.Recording.Core
{
    internal sealed class FaceAnimRecorder : MonoBehaviour, IFaceAnimRecorder
    {
#if UNITY_ANDROID
        private float DELAY = 0.26f; //need to sync face anim + song, because of face streaming delay
        private float DELAY_LIPSYNC = 0.26f;
        private float DELAY_VOICE = 0.25f;
#else
        private float DELAY = 0.1f; //need to sync face anim + song, because of face streaming delay
#endif

        [Inject] private FaceAnimationConverter _faceAnimationConverter;
        [Inject] private FaceBlendShapeMap _faceBlendShapeMap;
        private SkinnedMeshRenderer _faceMesh;
        
        private AudioSource _syncSource;
        private ITimeSource _timeSource;
        private bool _useSync;
        private bool _useSyncAudio;
        private float _timeStamp;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public FaceAnimationClip AnimationClip { get; private set; }
        public bool IsRecording { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(SkinnedMeshRenderer faceMesh)
        {
            _faceMesh = faceMesh;
        }

        public void StartRecording(ITimeSource timeSource, AudioSource syncSource = null)
        {
            IsRecording = true;
            
            _syncSource = syncSource;
            _useSyncAudio = _useSync = _syncSource;
            _timeSource = timeSource;
            
            #if UNITY_ANDROID
            if (_useSync)
            {
                DELAY = DELAY_LIPSYNC;
            }
            else if(DELAY_VOICE > 0f)
            {
                _useSync = true;
                _useSyncAudio = false;
                DELAY = DELAY_VOICE;
            }
            #endif
            

            AnimationClip = new FaceAnimationClip();
        }

        public async Task StopRecordingAsync()
        {
            if (!IsRecording) return;

            if (_useSync)
                await Task.Delay((int) (DELAY * 1000));

            IsRecording = false;
            AnimationClip.Duration = _timeSource.ElapsedSeconds;

            AnimationClip.RelativePath = FACE_ANIMATION_PATH;
            AnimationClip.FullSavePath = Path.Combine(Application.persistentDataPath, FACE_ANIMATION_PATH);
            await SaveAnimationToTextFileAsync();
        }

        public void CancelRecording()
        {
            IsRecording = false;
        }

#if  ENABLE_ONSCREEN_TUNING
        void OnGUI()
        {
            int midFont = (int) ((Screen.height / 1334f * 3) * 8 * 0.7f);
            GUI.skin.label.fontSize = midFont;
            GUI.skin.button.fontSize = midFont;
            GUI.skin.textField.fontSize = midFont;

            int width = (int)(Screen.width * 0.20f);
            int y = 260;
            int height = (int)(Screen.height * 0.05f);
            int nextSpot = height + (int)(Screen.height * 0.003f);
            if(GUI.Button(new Rect(10,y,width,height), "Lip Sync PIP +"))
            {
                DELAY_LIPSYNC= DELAY_LIPSYNC + 0.01f;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "Lip Sync PIP -"))
            {
                DELAY_LIPSYNC= DELAY_LIPSYNC - 0.01f;
                if (DELAY_LIPSYNC < 0f)
                    DELAY_LIPSYNC = 0.0f;
            }
            y += nextSpot; 
            if(GUI.Button(new Rect(10,y,width,height), "Vox Delay PIP +"))
            {
                DELAY_VOICE= DELAY_VOICE + 0.01f;
            }
            y += nextSpot;
            if(GUI.Button(new Rect(10,y,width,height), "Vox Delay PIP -"))
            {
                DELAY_VOICE= DELAY_VOICE - 0.01f;
                if (DELAY_VOICE < 0f)
                    DELAY_VOICE = 0.0f;
            }
            y += nextSpot; 
            
            GUI.Label(new Rect(10, y, Screen.width * 0.9f, Screen.height * 0.4f),"PIP Delay: " + DELAY_LIPSYNC + "\n Voice Delay: " + DELAY_VOICE + "\n Delay: " + DELAY);
        }
#endif

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RecordFrame()
        {
            var blendShapeValues = GetFrameBlendShapeValues();
            if (blendShapeValues == null || blendShapeValues.Count == 0) return;
            
            var frame = new FaceAnimFrame(_timeStamp, blendShapeValues);
            AnimationClip.AddFrame(frame);
        }

        private Dictionary<BlendShape, float> GetFrameBlendShapeValues()
        {
            var output = new Dictionary<BlendShape, float>();
            for (var i = 0; i < _faceMesh.sharedMesh.blendShapeCount; i++)
            {
                var shapeName = _faceMesh.sharedMesh.GetBlendShapeName(i);
                if (!_faceBlendShapeMap.IsFaceBlendShape(shapeName)) continue;
                var shapeWeight = _faceMesh.GetBlendShapeWeight(i);

                var blendShape = _faceBlendShapeMap.GetBlendShapeByName(shapeName);
                output.Add(blendShape, shapeWeight);
            }

            return output;
        }

        private void LateUpdate()
        {
            if (!IsRecording) return;

            UpdateFrameTimeStamp();

            RecordFrame();
        }

        private void UpdateFrameTimeStamp()
        {
            _timeStamp = _useSyncAudio
                ? _syncSource.timeSamples / (float) _syncSource.clip.frequency - DELAY
                : _timeSource.ElapsedSeconds - DELAY;
        }

        private async Task SaveAnimationToTextFileAsync()
        {
            var faceAnimText = await _faceAnimationConverter.ConvertToStringAsync(AnimationClip.FaceAnimationData);
            await SaveFileAsync(faceAnimText);
        }

        private async Task SaveFileAsync(string faceAnimText)
        {
            if (string.IsNullOrEmpty(faceAnimText) || string.IsNullOrWhiteSpace(faceAnimText) ||
                string.IsNullOrEmpty(AnimationClip.FullSavePath))
                return;
            
            if (!Directory.Exists(Path.GetDirectoryName(AnimationClip.FullSavePath)))
            {
                var dir = Path.GetDirectoryName(AnimationClip.FullSavePath);
                Directory.CreateDirectory(dir);
            }

            //force running in non main thread
            await Task.Run(() =>
            {
                File.WriteAllText(AnimationClip.FullSavePath, faceAnimText);
            });
        }
    }
}