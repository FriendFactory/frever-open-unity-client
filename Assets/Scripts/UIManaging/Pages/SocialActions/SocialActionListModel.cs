using System.Collections.Generic;
using System.Linq;
using Modules.SocialActions;

namespace UIManaging.Pages.SocialActions
{
    public sealed class SocialActionListModel
    {
        public List<SocialActionCardModel> CardModels { get; set; }

        public void Remove(long actionId)
        {
            if(CardModels.All(m => m.ActionId != actionId)) return;
            
            var model = CardModels.Find(m => m.ActionId == actionId);
            CardModels.Remove(model);
        }
    }
}