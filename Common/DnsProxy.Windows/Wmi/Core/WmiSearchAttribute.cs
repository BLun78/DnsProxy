using System;

namespace BAG.IT.Core.Wmi.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WmiSearchAttribute : Attribute
    {
        public WmiSearchAttribute(string scope, string query)
        {
            Scope = scope;
            Query = query;

        }
        public string Scope { get; }
        public string Query { get; }
    }
}