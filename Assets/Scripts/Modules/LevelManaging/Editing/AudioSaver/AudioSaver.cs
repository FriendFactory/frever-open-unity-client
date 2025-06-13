using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Modules.LevelManaging.Editing.EventCreation
{
    public class AudioSaver
    {
        private const int HEADER_SIZE = 44;
        private const string EXTENSION = ".wav";

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static void Save(string filename, AudioClip clip)
        {
            if (!filename.ToLower().EndsWith(EXTENSION))
            {
                filename += EXTENSION;
            }

            var filepath = filename;
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            var clipData = new ClipData {Samples = clip.samples, Channels = clip.channels};
            var dataFloat = new float[clip.samples * clip.channels];
            clip.GetData(dataFloat, 0);
            clipData.SamplesData = dataFloat;

            using (var fileStream = CreateEmpty(filepath))
            {
                var stream = new MemoryStream();
                ConvertAndWrite(stream, clipData);
                stream.WriteTo(fileStream);
                WriteHeader(fileStream, clip);
            }
        }

        public AudioClip TrimSilence(AudioClip clip, float min)
        {
            var samples = new float[clip.samples];
            clip.GetData(samples, 0);
            return TrimSilence(new List<float>(samples), min, clip.channels, clip.frequency);
        }

        public AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D = false, bool stream = false)
        {
            int i;

            for (i = 0; i < samples.Count; i++)
            {
                if (Mathf.Abs(samples[i]) > min)
                {
                    break;
                }
            }

            samples.RemoveRange(0, i);

            for (i = samples.Count - 1; i > 0; i--)
            {
                if (Mathf.Abs(samples[i]) > min)
                {
                    break;
                }
            }

            samples.RemoveRange(i, samples.Count - i);

            #pragma warning disable 618
            var clip = AudioClip.Create("TempClip", samples.Count, channels, hz, _3D, stream);
            #pragma warning restore 618

            clip.SetData(samples.ToArray(), 0);

            return clip;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static FileStream CreateEmpty(string filepath)
        {
            var fileStream = new FileStream(filepath, FileMode.Create);
            const byte emptyByte = new byte();

            for (var i = 0; i < HEADER_SIZE; i++) //preparing the header
            {
                fileStream.WriteByte(emptyByte);
            }

            return fileStream;
        }

        private static void ConvertAndWrite(MemoryStream memStream, ClipData clipData)
        {
            var samples = clipData.SamplesData;
            var intData = new short[samples.Length];
            var bytesData = new byte[samples.Length * 2];

            const float rescaleFactor = 32767; //to convert float to Int16

            for (var i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
            }

            Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
            memStream.Write(bytesData, 0, bytesData.Length);
        }

        private static void WriteHeader(FileStream fileStream, AudioClip clip)
        {
            var hz = clip.frequency;
            var channels = clip.channels;
            var samples = clip.samples;

            fileStream.Seek(0, SeekOrigin.Begin);

            var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            var chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            var subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            const ushort one = 1;

            var audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            var numChannels = BitConverter.GetBytes(channels);
            fileStream.Write(numChannels, 0, 2);

            var sampleRate = BitConverter.GetBytes(hz);
            fileStream.Write(sampleRate, 0, 4);

            var byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
            fileStream.Write(byteRate, 0, 4);

            var blockAlign = (ushort)(channels * 2);
            fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            const ushort bps = 16;

            var bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            var data = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(data, 0, 4);

            var subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            fileStream.Write(subChunk2, 0, 4);
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private struct ClipData
        {
            public int Samples;
            public int Channels;
            public float[] SamplesData;
        }
    }
}