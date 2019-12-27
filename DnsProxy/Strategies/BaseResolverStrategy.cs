﻿#region Apache License-2.0

// Copyright 2019 Bjoern Lundstroem
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using DnsProxy.Common;
using DnsProxy.Models.Rules;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Strategies
{
    internal abstract class BaseResolverStrategy<TRule> : IDisposable, IOrder, IDnsResolverStrategy<TRule>,
        IDnsResolverStrategy
        where TRule : IRule
    {
        protected readonly ILogger<BaseResolverStrategy<TRule>> Logger;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _timeoutCts;
        protected TRule Rule;

        protected BaseResolverStrategy(ILogger<BaseResolverStrategy<TRule>> logger)
        {
            Logger = logger;
        }

        public int Order { get; protected set; }

        public abstract Task<List<DnsRecordBase>> ResolveAsync(DnsQuestion dnsQuestion, CancellationToken cancellationToken);

        public abstract Models.Strategies GetStrategy();
        public abstract void OnRuleChanged();

        public void SetRule(IRule rule)
        {
            Rule = (TRule)rule;
        }
        public void SetRule(TRule rule)
        {
            Rule = rule;
        }

        public bool MatchPattern(DnsQuestion dnsQuestion)
        {
            string pattern;
            if (!string.IsNullOrWhiteSpace(Rule.DomainNamePattern))
            {
                pattern = Rule.DomainNamePattern;

            }
            else if (!string.IsNullOrWhiteSpace(Rule.DomainName))
            {
                pattern = $"^{Rule.DomainName.Replace(".", @"\.", StringComparison.InvariantCulture)}$";
            }
            else
            {
                throw new NotSupportedException($"On Attribute {nameof(Rule.DomainName)} or {nameof(Rule.DomainNamePattern)} must be set!");
            }

            var match = Regex.Match(dnsQuestion.Name.ToString(), pattern);
            return match.Success;
        }

        protected CancellationToken CreateCancellationToken(CancellationToken cancellationToken)
        {
            _timeoutCts = new CancellationTokenSource(Rule.QueryTimeout);
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _timeoutCts.Token);
            return _cts.Token;
        }

        protected void DisposeCancellationToken()
        {
            _timeoutCts?.Dispose();
            _cts?.Dispose();
        }

        #region IDisposable Support

        protected bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseResolverStrategy()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}