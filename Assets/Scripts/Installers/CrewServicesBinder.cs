using System;
using Bridge.Models.ClientServer;
using Modules.Crew;
using UIManaging.PopupSystem.Popups.ChatSettings;
using Zenject;

namespace Installers
{
    internal static class CrewServicesBinder
    {
        public static void BindCrewServices(this DiContainer container)
        {
            container.Bind<CrewService>().AsSingle();
            container.BindFactory<GroupShortInfo, Action<bool>, ChatMemberModel, ChatMemberModel.Factory>().AsSingle();
        }
    }
}