using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Management;

namespace BAG.IT.Core.Wmi
{
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class SccmScheduler : ISccmScheduler
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
    {
        private readonly ILogger<SccmScheduler> _logger;

        public SccmScheduler(ILogger<SccmScheduler> logger)
        {
            _logger = logger;
        }
        public void RunTriggers(params SccmTriggerId[] triggerIds)
        {
            foreach (var triggerId in triggerIds)
            {
                RunTrigger(triggerId);
            }
        }



        public bool RunTrigger(SccmTriggerId triggerId)
        {
            try
            {
                var s = $"00000000-0000-0000-0000-00{(int)triggerId:0000000000}";

                ManagementScope scope = new ManagementScope(@"\\.\root\ccm");
                ManagementBaseObject outMpParams;
                using (ManagementClass cls = new ManagementClass(scope.Path.Path, "sms_client", null))
                {
                    ManagementBaseObject inParams = cls.GetMethodParameters("TriggerSchedule");

                    inParams["sScheduleID"] = "{" + s + "}";

                    outMpParams = cls.InvokeMethod("TriggerSchedule", inParams, null);
                }

                if (outMpParams != null)
                {
                    var result = (int?)outMpParams["ReturnValue"];
                    return !result.HasValue || result == 0;
                }

                return false;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogWarning(e, e.Message);
                return false;
            }

        }

    }

    public interface ISccmScheduler
    {
        void RunTriggers(params SccmTriggerId[] triggerIds);
        bool RunTrigger(SccmTriggerId triggerId);
    }
}