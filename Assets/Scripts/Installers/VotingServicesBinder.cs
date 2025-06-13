using UIManaging.Pages.VotingResult;
using Zenject;

namespace Installers
{
    internal static class VotingServicesBinder
    {
        public static void BindVotingServices(this DiContainer container)
        {
            container.BindInterfacesAndSelfTo<VotingBattleResultManager>().AsSingle();
        }
    }
}