using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Modules.LevelManaging.GifPreview.Core
{
    internal class LevelPreviewCapture : ILevelPreviewCapture
    {
        public LevelPreview LastCaptured { get; set; }
        public bool IsRunning { get; set; }

        private string _directory = "Assets/LevelPreviews";
        private string _fileName = "";

        private int _width = 386;
        private int _height = 768;

        private int _snapshotsMax;
        private float _frameRate;

        List<RenderTexture> _snapshots = new List<RenderTexture>();

        private float _lastSnapshot;
        private Coroutine _captureCoroutine;
        private bool  _previousGifDone = true;
         
        public void StartCapture(int snapshotsCount, int frameRate)
        {
            if(IsRunning)
                return;
            
            _frameRate = frameRate;
            _snapshotsMax += snapshotsCount;
            _lastSnapshot = Time.time;
            _captureCoroutine = CaptureTheGIF.Instance.StartCoroutine(CollectSnapShots());
            IsRunning = true;
        }

        public void StopCapture()
        {
            if(!IsRunning)
                return;
            
            IsRunning = false;
            if (_captureCoroutine != null)
            {
                CaptureTheGIF.Instance.StopCoroutine(_captureCoroutine);
            }
        }

        public IEnumerator BakeGif(Action onCompleted, Action onFailed)
        {
            if(!_previousGifDone)
                yield break;
                
            if (_snapshots.Count == 0)
            {
                onFailed?.Invoke();
                yield break;
            }

            _previousGifDone = false;
            
            _fileName = DateTime.Now.ToString("MM-dd-yyyy-HH_mm_ss");
            var file = CaptureTheGIF.MakeFile(_directory, _fileName);
            yield return CaptureTheGIF.Instance.MakeGif(_snapshots.ToList(), 1 / _frameRate, file, true);

            LastCaptured = new LevelPreview(Path.Combine(Path.GetFileName(file.Name)));
            _snapshots.Clear();
            _snapshotsMax = 0;
            onCompleted?.Invoke();
            _previousGifDone = true;
        }

        private IEnumerator CollectSnapShots()
        {
            IsRunning = true;

            while (IsRunning)
            {
                if (Time.time >= _lastSnapshot + 1f / _frameRate)
                {
                    _lastSnapshot = Time.time;
                    GifSnapshots();
                }

                yield return null;

                if (_snapshotsMax <= _snapshots.Count)
                {
                    StopCapture();
                }

            }
        }

        private void GifSnapshots()
        {
            _snapshots.Add(CaptureTheGIF.Snapshot(_width, _height));
        }

    }
}
