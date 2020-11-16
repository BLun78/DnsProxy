using JetBrains.Annotations;
using LightInject;
using System;

namespace BAG.IT.Core.Wmi
{

    public class DependencyRegistration : ICompositionRoot
    {
        public void Compose([NotNull] IServiceRegistry serviceRegistry)
        {
            if (serviceRegistry == null) throw new ArgumentNullException(nameof(serviceRegistry));

#pragma warning disable CA2000 // Dispose objects before losing scope


            serviceRegistry.Register<ICCM_InstalledComponent, CCM_InstalledComponent>(new PerRequestLifeTime());
            serviceRegistry.Register<ICCM_InstalledComponentList, CCM_InstalledComponentList>(new PerRequestLifeTime());
            serviceRegistry.Register<IMsftdaConnectionStatus, MsftdaConnectionStatus>(new PerRequestLifeTime());
            serviceRegistry.Register<INetworkAdapterItem, NetworkAdapterItem>(new PerRequestLifeTime());
            serviceRegistry.Register<INetworkAdapterList, NetworkAdapterList>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmClientInfo, SccmClientInfo>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmClient, SccmClient>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmInventoryActionStatusHwInv, SccmInventoryActionStatusHwInv>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmInventoryActionStatusSwInv, SccmInventoryActionStatusSwInv>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmInventoryActionStatusHeartbeatInv, SccmInventoryActionStatusHeartbeatInv>(new PerRequestLifeTime());
            serviceRegistry.Register<ISccmScheduler, SccmScheduler>(new PerContainerLifetime());
            serviceRegistry.Register<ISmsAuthority, SmsAuthority>(new PerRequestLifeTime());
            serviceRegistry.Register<ISmsClient, SmsClient>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32Bios, Win32Bios>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32ComputerSystem, Win32ComputerSystem>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32ComputerSystemProduct, Win32ComputerSystemProduct>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32NetworkAdapterConfigurationItem, Win32NetworkAdapterConfigurationItem>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32NetworkAdapterConfigurationList, Win32NetworkAdapterConfigurationList>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32OperatingSystem, Win32OperatingSystem>(new PerRequestLifeTime());
            serviceRegistry.Register<IWin32Processor, Win32Processor>(new PerRequestLifeTime());


#pragma warning restore CA2000 // Dispose objects before losing scope
        }

    }
}