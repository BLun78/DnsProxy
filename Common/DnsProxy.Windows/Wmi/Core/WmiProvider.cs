using Microsoft.Extensions.Logging;
using System.Linq;
using System.Management;

namespace BAG.IT.Core.Wmi.Core
{
    public abstract class WmiProvider : WmiProviderListItem, IWmiProvider
    {
        private readonly ILogger<WmiProvider> _logger;
        private readonly string _query;
        private readonly string _scope;

        protected WmiProvider(ILogger<WmiProvider> logger)
        {
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
            catch (ManagementException)
            {
                return false;

            }
        }


        public void ReadData()
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(_scope, _query);
                foreach (var item in searcher.Get())
                {
                    var queryObj = (ManagementObject)item;
                    SetData(queryObj);
                }
            }
            catch (ManagementException e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}