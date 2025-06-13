using System;

namespace Modules.AudioControllers.Song
{
    public class SongSelectedCondition : SongCondition
    {
        private bool IsSongSelected { get; set; }

        public override bool CheckCondition()
        {
            return !IsSongSelected;
        }

        public override bool Subscribe()
        {
            return false;
        }

        private void OnSongSelected(object obj, EventArgs e)
        {

            IsSongSelected = true;
        }

        private void OnSongDeselected(object obj, EventArgs e)
        {

            IsSongSelected = false;
        }
    }
}