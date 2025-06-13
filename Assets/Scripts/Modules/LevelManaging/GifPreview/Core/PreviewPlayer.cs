using OldMoatGames;
using UnityEngine.UI;

namespace Modules.LevelManaging.GifPreview.Core
{
    public class PreviewPlayer
    {
        private AnimatedGifPlayer _gifPlayer;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public PreviewPlayer(string fileName, AnimatedGifPlayer gifPlayer)
        {
            _gifPlayer = gifPlayer;
            _gifPlayer.FileName = fileName;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Play()
        {
            _gifPlayer.OnReady += DisplayGif;
            _gifPlayer.Loop = true;
            _gifPlayer.Init();
        }

        public void Stop()
        {
            _gifPlayer.Pause();
            _gifPlayer.OnReady -= DisplayGif;
        }

        public void Play(LevelPreview preview, AnimatedGifPlayer gifPlayer)
        {
            _gifPlayer = gifPlayer;
            _gifPlayer.FileName = preview.FileName;
            _gifPlayer.OnReady += DisplayGif;
            _gifPlayer.Init();
        }

        public void SetPath(GifPath path)
        {
            _gifPlayer.Path = path;
        }

        public void SetFileName(string fileName)
        {
            _gifPlayer.FileName = fileName;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DisplayGif()
        {
            _gifPlayer.GetComponent<RawImage>().enabled = true;
            _gifPlayer.Play();
        }
    }
}
