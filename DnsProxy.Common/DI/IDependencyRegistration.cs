using DnsProxy.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DnsProxy.Common.DI
{
    public interface IDependencyRegistration : IOrder
    {
        void Register(IServiceCollection serviceCollection);
    }
}
