﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Magic.Net
{
    /// <summary>
    ///     A simple IOC
    /// </summary>
    public sealed class ServiceCollection : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceCreator> _services = new Dictionary<Type, ServiceCreator>();

        #region Implementation of IServiceProvider

        [CanBeNull]
        public object GetService(Type serviceType)
        {
            ServiceCreator creator;
            if (_services.TryGetValue(serviceType, out creator))
                return creator.Create();
            return null;
        }

        #endregion

        public void AddService<TService>(Func<TService> func)
        {
            _services.Add(typeof(TService), new ServiceFuncCreator<TService>(func));
        }

        [PublicAPI]
        public void AddService<TService>(TService instance)
        {
            _services.Add(typeof(TService), new StaticService<TService>(instance));
        }

        private sealed class StaticService<TService> : ServiceCreator
        {
            private readonly TService _instance;

            public StaticService(TService instance)
                : base(typeof(TService))
            {
                _instance = instance;
            }

            #region Overrides of ServiceCreator<TService>

            public override object Create()
            {
                return _instance;
            }

            #endregion
        }

        private sealed class ServiceFuncCreator<TService> : ServiceCreator
        {
            private readonly Func<TService> _func;

            public ServiceFuncCreator(Func<TService> func)
                : base(typeof(TService))
            {
                _func = func;
            }

            #region Overrides of ServiceCreator<TService>

            public override object Create()
            {
                return _func();
            }

            #endregion
        }

        private abstract class ServiceCreator
        {
            private readonly Type _serviceType;

            protected ServiceCreator(Type serviceType)
            {
                _serviceType = serviceType;
            }

            public abstract object Create();

            #region Overrides of Object

            public override bool Equals(object other)
            {
                // If parameter is null return false.
                // ReSharper disable once UseNullPropagation
                if (other == null)
                    return false;

                // If parameter cannot be cast to Point return false.
                ServiceCreator p = other as ServiceCreator;
                if (p == null)
                    return false;

                // Return true if the fields match:
                return _serviceType == p._serviceType;
            }

            public override int GetHashCode()
            {
                return _serviceType.GetHashCode() ^ 353;
            }

            #endregion
        }
    }
}