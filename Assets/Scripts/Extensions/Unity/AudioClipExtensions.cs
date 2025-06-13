using System;
using UnityEngine;

namespace Extensions
{
    public static class AudioClipExtensions
    {
        public static AudioClip ExtractPiece(this AudioClip clip, float startFromSec, float endSec)
        {
            var sourceSampleData = new float[clip.samples * clip.channels];
            clip.GetData(sourceSampleData, 0);
            var sampleStartIndex = (int)(startFromSec * clip.frequency);
            var sampleEndIndex = (int)(endSec * clip.frequency);
            var newClipSampleData = new float[sampleEndIndex - sampleStartIndex];
            
            Array.Copy(sourceSampleData, sampleStartIndex, newClipSampleData, 0, newClipSampleData.Length);
            return CreateAudioClip(clip, newClipSampleData);
        }

        private static AudioClip CreateAudioClip(AudioClip sourceClip, float[] newClipSampleData)
        {
           var audioClip = AudioClip.Create(sourceClip.name, newClipSampleData.Length,
               sourceClip.channels,
               sourceClip.frequency, false); 
           audioClip.SetData(newClipSampleData, 0);
           return audioClip;
        }
    }
}