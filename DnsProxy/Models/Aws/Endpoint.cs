using System;
using System.Collections.Generic;
using System.Text;
using Amazon.EC2.Model;

namespace DnsProxy.Models.Aws
{
    internal class Endpoint
    {
        public Endpoint(VpcEndpoint vpcEndpoint)
        {
            VpcEndpoint = vpcEndpoint;
        }

        public VpcEndpoint VpcEndpoint { get; set; }
        public List<NetworkInterface> NetworkInterfaces { get; set; }
    }
}
