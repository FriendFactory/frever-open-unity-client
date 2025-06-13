using Bridge.Models.ClientServer.Assets;
using System.Threading.Tasks;
using Bridge;
using Bridge.ClientServer.Assets.Wardrobes;
using Bridge.Results;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    internal sealed class MyWardrobeListRequest : WardrobeRequest
    {
        public MyWardrobeListRequest(IBridge bridge, MyWardrobeFilterModel filter) : base(bridge, filter)
        {
        }

        protected override Task<ArrayResult<WardrobeShortInfo>> DownloadWardrobes()
        {
            return Bridge.GetMyWardrobeList((MyWardrobeFilterModel)Filter, TokenSource.Token);
        }
    }
}