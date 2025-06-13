using System;
using TMPro;
using UnityEngine.EventSystems;

namespace Common
{
    public class TextMeshProLinkHandler : TextMeshProLinksOpener
    {
        public event Action<string, string> HyperlinkHandled;

        protected override void HandleLink(TMP_LinkInfo linkInfo)
        {
            HyperlinkHandled?.Invoke(linkInfo.GetLinkID(), linkInfo.GetLinkText());
        }
    }
}