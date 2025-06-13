using Common;
using UIManaging.Core;
using VoxelBusters.EssentialKit;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal class InviteButton : ButtonBase 
    {
        private const string EMOJI_UNICODE = "\U0001F31F"; //Star emoji. 
        private const string INVITE_MESSAGE = "Come and hang out with me on Frever!" + EMOJI_UNICODE + "\n" + Constants.APP_STORE_URL;
        
        protected override void OnClick()
        {
            var shareSheet = ShareSheet.CreateInstance();
            shareSheet.AddText(INVITE_MESSAGE);
            
            shareSheet.Show();
        }
    }
}
