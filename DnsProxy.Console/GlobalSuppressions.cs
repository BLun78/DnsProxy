#region Apache License-2.0
// Copyright 2020 Bjoern Lundstroem
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

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'CheckAwsVpc' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:DnsProxy.Console.Program.CheckAwsVpc~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'Task Program.CheckAwsVpc()' passes a literal string as parameter 'message' of a call to 'void LoggerExtensions.LogInformation(ILogger logger, string message, params object[] args)'. Retrieve the following string(s) from a resource table instead: \"No AWS config found!\".", Justification = "<Pending>", Scope = "member", Target = "~M:DnsProxy.Console.Program.CheckAwsVpc~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'CheckForAwsMfaAsync' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:DnsProxy.Console.Program.CheckForAwsMfaAsync~System.Threading.Tasks.Task")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'void Program.CreateHeader()' passes a literal string as parameter 'value' of a call to 'void Console.WriteLine(string value)'. Retrieve the following string(s) from a resource table instead: \"starts up BLun.de DNS Proxy ...\".", Justification = "<Pending>", Scope = "member", Target = "~M:DnsProxy.Console.Program.CreateHeader")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'Task<int> Program.Main(string[] args)' passes a literal string as parameter 'message' of a call to 'void LoggerExtensions.LogInformation(ILogger logger, string message, params object[] args)'. Retrieve the following string(s) from a resource table instead: \"stop {DefaultTitle}\".", Justification = "<Pending>", Scope = "member", Target = "~M:DnsProxy.Console.Program.Main(System.String[])~System.Threading.Tasks.Task{System.Int32}")]