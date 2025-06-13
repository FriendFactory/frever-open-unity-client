using System;
using Common.ExceptionCatcher;
using Modules.SentryManaging;
//using Modules.SentryManaging;
using UIManaging.Pages.Common.ErrorsManagement;
using UnityEngine;
using Zenject;

namespace Installers
{
    internal static class ErrorTrackingBinder
    {
        private const string IGNORED_EXC_DATA_PATH = "ScriptableObjects/Ignored Exceptions";
        
        public static void BindErrorTracking(this DiContainer container)
        {
            container.Bind<SentryManager>().AsSingle();
            container.Bind<ErrorsManager>().AsSingle();
            container.BindInterfacesAndSelfTo<ExceptionCatcher>().FromNewComponentOnNewGameObject()
                     .WithGameObjectName(nameof(ExceptionCatcher)).AsSingle().NonLazy();
            container.BindInterfacesAndSelfTo<ExceptionsAnalyticsService>().AsSingle();
            container.BindIgnoredExceptionsData();
        }

        private static void BindIgnoredExceptionsData(this DiContainer container)
        {
            var ignoredExcData = Resources.Load<IgnoredExceptionsData>(IGNORED_EXC_DATA_PATH);
            if (ignoredExcData == null)
                throw new InvalidOperationException(
                    $"Couldn't find in resources {nameof(IgnoredExceptionsData)} at {IGNORED_EXC_DATA_PATH}");
            container.BindInterfacesAndSelfTo<IgnoredExceptionsData>().FromInstance(ignoredExcData).AsSingle();
        }
    }
}