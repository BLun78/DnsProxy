using System;

namespace DnsProxy.Models.Rules
{
    internal interface IRuleStrategy
    {
        Type GetStraegy();
    }
}