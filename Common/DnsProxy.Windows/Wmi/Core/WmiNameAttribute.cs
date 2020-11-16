using System;

namespace BAG.IT.Core.Wmi.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class WmiNameAttribute : Attribute
    {
        public WmiNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}