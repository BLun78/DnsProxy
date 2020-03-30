using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DnsProxy.Plugin.Models.Rules;

namespace DnsProxy.Common.Models.Rules
{
    public class RuleFactory : IRuleFactory
    {
        private readonly Type[] _rules;

        public RuleFactory(Type[] rules)
        {
            _rules = rules;
        }

        public IRule Create(string ruleName, IRule rule)
        {
            foreach (var ruleType in _rules.Where(x => typeof(IRule).IsAssignableFrom(x)))
            {
                if ($"{ruleName}Rule".Equals(ruleType.Name, StringComparison.InvariantCultureIgnoreCase)
                    || ruleType.Name.Equals(ruleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (IRule)Activator.CreateInstance(ruleType, (Rule)rule);
                }
            }
            return default(IRule);
        }
    }
}
