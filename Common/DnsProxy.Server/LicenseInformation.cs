using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DnsProxy.Server
{
    public static class LicenseInformation
    {
        public static void GetLicense(ILogger logger)
        {
            logger.LogInformation($"Plugin: DnsProxy.Server");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation("     Copyright 2019 - 2020 Bjoern Lundstroem - (https://github.com/BLun78/DnsProxy)");
            logger.LogInformation("      ");
            logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            logger.LogInformation("     you may not use this file except in compliance with the License.");
            logger.LogInformation("     You may obtain a copy of the License at");
            logger.LogInformation("      ");
            logger.LogInformation("     \thttp://www.apache.org/licenses/LICENSE-2.0");
            logger.LogInformation("      ");
            logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            logger.LogInformation("     See the License for the specific language governing permissions and");
            logger.LogInformation("     limitations under the License.");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation(" Used libraries:");
            logger.LogInformation("----------------------------------------------------------------------------------------");
            logger.LogInformation("     ARSoft.Tools.Net - A .net Framework/ .net core DNS Library");
            logger.LogInformation("      ");
            logger.LogInformation("     Copyright 2017 Alexander Reinert - (https://github.com/alexreinert/ARSoft.Tools.Net)");
            logger.LogInformation("      ");
            logger.LogInformation("     Licensed under the Apache License, Version 2.0(the \"License\");");
            logger.LogInformation("     you may not use this file except in compliance with the License.");
            logger.LogInformation("     You may obtain a copy of the License at");
            logger.LogInformation("      ");
            logger.LogInformation("     \thttps://github.com/alexreinert/ARSoft.Tools.Net/blob/master/LICENSE");
            logger.LogInformation("      ");
            logger.LogInformation("     Unless required by applicable law or agreed to in writing, software");
            logger.LogInformation("     distributed under the License is distributed on an \"AS IS\" BASIS,");
            logger.LogInformation("     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.");
            logger.LogInformation("     See the License for the specific language governing permissions and");
            logger.LogInformation("     limitations under the License.");
        }
    }
}
