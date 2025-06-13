namespace Modules.AudioOutputManaging
{
     /// <summary>
     /// This module should control audio output on devices(speakers vs earphone vs headphones)
     /// </summary>
     public interface IDeviceAudioOutputControl
     {
          void Run();
          void Stop();
     }
}
