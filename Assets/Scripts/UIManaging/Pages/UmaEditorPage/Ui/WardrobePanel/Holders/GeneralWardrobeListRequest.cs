using Bridge.Models.ClientServer.Assets;
using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Results;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{

    internal sealed class GeneralWardrobeListRequest : WardrobeRequest
    {
        public GeneralWardrobeListRequest(IBridge bridge, WardrobeFilter filter) : base(bridge, filter)
        {
        }

        protected override Task<ArrayResult<WardrobeShortInfo>> DownloadWardrobes()
        {
            return Bridge.GetWardrobeList((WardrobeFilter)Filter, TokenSource.Token);
        }
    }
}
