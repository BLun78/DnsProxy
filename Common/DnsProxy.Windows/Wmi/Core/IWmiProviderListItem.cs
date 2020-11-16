using System.Collections.Generic;
using System.Management;

namespace BAG.IT.Core.Wmi.Core
{
    public interface IWmiProviderListItem
    {
        public IDictionary<string, object> Data { get; }
        public void SetData(ManagementObject queryObj);
    }
}