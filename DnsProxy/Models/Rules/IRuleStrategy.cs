using System;
using System.Collections.Generic;
using System.Text;

namespace DnsProxy.Models.Rules
{
    internal interface IRuleStrategy
    {
        Type GetStraegy();
    }
}
