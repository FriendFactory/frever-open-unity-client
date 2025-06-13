using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Modules.FreverUMA
{
    internal sealed class CharacterCommandsFactory : IFactory
    {
        private readonly DiContainer _container;

        public CharacterCommandsFactory(DiContainer container)
        {
            _container = container;
        }

        public T Create<T>(params object[] args) where T : UserCommand
        {
            var command = _container.Instantiate<T>(args);
            return command;
        }
    }
}