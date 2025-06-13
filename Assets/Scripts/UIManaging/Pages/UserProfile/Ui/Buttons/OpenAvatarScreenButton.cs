using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    public class OpenAvatarScreenButton : ButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.AvatarPage, new UmaAvatarArgs());
        }
    }
}
