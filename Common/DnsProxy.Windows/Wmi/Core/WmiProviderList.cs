using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;


namespace BAG.IT.Core.Wmi.Core
{
    public interface IWmiProviderList<T> : IWmiProvider where T : IWmiProviderListItem
    {
        List<T> Items { get; }
    }

    public abstract class WmiProviderList<T> : IWmiProviderList<T> where T : IWmiProviderListItem
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WmiProviderList<T>> _logger;
        public List<T> Items { get; } = new List<T>();

        private readonly string _query;
        private readonly string _scope;

        protected WmiProviderList(IServiceProvider serviceProvider, ILogger<WmiProviderList<T>> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            var dnAttribute = GetType().GetCustomAttributes(
                typeof(WmiSearchAttribute), true
            ).OfType<WmiSearchAttribute>().First();

            this._query = dnAttribute.Query;
            this._scope = dnAttribute.Scope;
        }



        public bool Exists()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, _query))
                {
                    searcher.Get();
                }

                return true;

            }
            catch (ManagementException e)
            {
                _logger.LogWarning(e, e.Message);
                return false;

            }
        }

        public void ReadData()
        {
            try
            {
                Items.Clear();
                using ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher(_scope, _query);

                var queryList = searcher.Get().Cast<ManagementObject>();

                foreach (var queryObj in queryList)
                {
                    T instance = _serviceProvider.GetService<T>();
                    instance.SetData(queryObj);
                    Items.Add(instance);

                }

            }
            catch (ManagementException e)
            {
                _logger.LogError(e, e.Message);
            }
        }

    }
}