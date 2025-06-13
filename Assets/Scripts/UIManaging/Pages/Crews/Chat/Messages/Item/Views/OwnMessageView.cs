using Common.Hyperlinks;

namespace UIManaging.Pages.Crews
{
    internal class OwnMessageView : UserMessageView
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        protected override string MentionStyle => HyperlinkParser.MENTION_STYLE_DEFAULT;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void UpdateUsername()
        {
        }
    }
}