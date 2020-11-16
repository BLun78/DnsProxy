namespace BAG.IT.Core.Wmi
{

    //https://www.systemcenterdudes.com/configuration-manager-2012-client-command-list/
    /*
     *https://rid500.wordpress.com/2017/07/23/sccm-refresh-machine-policy-retrieval-evaluation-cycle-via-wmi/
     *
     *Trigger Codes:
{00000000-0000-0000-0000-000000000001} Hardware Inventory
{00000000-0000-0000-0000-000000000002} Software Inventory
{00000000-0000-0000-0000-000000000003} Discovery Inventory
{00000000-0000-0000-0000-000000000010} File Collection
{00000000-0000-0000-0000-000000000011} IDMIF Collection
{00000000-0000-0000-0000-000000000012} Client Machine Authentication
{00000000-0000-0000-0000-000000000021} Request Machine Assignments
{00000000-0000-0000-0000-000000000022} Evaluate Machine Policies
{00000000-0000-0000-0000-000000000023} Refresh Default MP Task
{00000000-0000-0000-0000-000000000024} LS (Location Service) Refresh Locations Task
{00000000-0000-0000-0000-000000000025} LS (Location Service) Timeout Refresh Task
{00000000-0000-0000-0000-000000000026} Policy Agent Request Assignment (User)
{00000000-0000-0000-0000-000000000027} Policy Agent Evaluate Assignment (User)
{00000000-0000-0000-0000-000000000031} Software Metering Generating Usage Report
{00000000-0000-0000-0000-000000000032} Source Update Message
{00000000-0000-0000-0000-000000000037} Clearing proxy settings cache
{00000000-0000-0000-0000-000000000040} Machine Policy Agent Cleanup
{00000000-0000-0000-0000-000000000041} User Policy Agent Cleanup
{00000000-0000-0000-0000-000000000042} Policy Agent Validate Machine Policy / Assignment
{00000000-0000-0000-0000-000000000043} Policy Agent Validate User Policy / Assignment
{00000000-0000-0000-0000-000000000051} Retrying/Refreshing certificates in AD on MP
{00000000-0000-0000-0000-000000000061} Peer DP Status reporting
{00000000-0000-0000-0000-000000000062} Peer DP Pending package check schedule
{00000000-0000-0000-0000-000000000063} SUM Updates install schedule
{00000000-0000-0000-0000-000000000071} NAP action
{00000000-0000-0000-0000-000000000101} Hardware Inventory Collection Cycle
{00000000-0000-0000-0000-000000000102} Software Inventory Collection Cycle
{00000000-0000-0000-0000-000000000103} Discovery Data Collection Cycle
{00000000-0000-0000-0000-000000000104} File Collection Cycle
{00000000-0000-0000-0000-000000000105} IDMIF Collection Cycle
{00000000-0000-0000-0000-000000000106} Software Metering Usage Report Cycle
{00000000-0000-0000-0000-000000000107} Windows Installer Source List Update Cycle
{00000000-0000-0000-0000-000000000108} Software Updates Assignments Evaluation Cycle
{00000000-0000-0000-0000-000000000109} Branch Distribution Point Maintenance Task
{00000000-0000-0000-0000-000000000110} DCM policy
{00000000-0000-0000-0000-000000000111} Send Unsent State Message
{00000000-0000-0000-0000-000000000112} State System policy cache cleanout
{00000000-0000-0000-0000-000000000113} Scan by Update Source
{00000000-0000-0000-0000-000000000114} Update Store Policy
{00000000-0000-0000-0000-000000000115} State system policy bulk send high
{00000000-0000-0000-0000-000000000116} State system policy bulk send low
{00000000-0000-0000-0000-000000000120} AMT Status Check Policy
{00000000-0000-0000-0000-000000000121} Application manager policy action
{00000000-0000-0000-0000-000000000122} Application manager user policy action
{00000000-0000-0000-0000-000000000123} Application manager global evaluation action
{00000000-0000-0000-0000-000000000131} Power management start summarizer
{00000000-0000-0000-0000-000000000221} Endpoint deployment reevaluate
{00000000-0000-0000-0000-000000000222} Endpoint AM policy reevaluate
{00000000-0000-0000-0000-000000000223} External event detection
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     * 
Manager Client Scan Trigger with WMI
You can also trigger agent from WMI command line if you don’t want to open the configuration manager properties.
Client Agent WMI Command
Application Deployment Evaluation Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000121}” /NOINTERACTIVE
Discovery Data Collection Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000003}” /NOINTERACTIVE
File Collection Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000010}” /NOINTERACTIVE
Hardware Inventory Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000001}” /NOINTERACTIVE
Machine Policy Retrieval Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000021}” /NOINTERACTIVE
Machine Policy Evaluation Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000022}” /NOINTERACTIVE
Software Inventory Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000002}” /NOINTERACTIVE
Software Metering Usage Report Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000031}” /NOINTERACTIVE
Software Updates Assignments Evaluation Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000108}” /NOINTERACTIVE
Software Update Scan Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000113}” /NOINTERACTIVE
State Message Refresh WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000111}” /NOINTERACTIVE
User Policy Retrieval Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000026}” /NOINTERACTIVE
User Policy Evaluation Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000027}” /NOINTERACTIVE
Windows Installers Source List Update Cycle WMIC /namespace:\\root\ccm path sms_client CALL TriggerSchedule “{00000000-0000-0000-0000-000000000032}” /NOINTERACTIVE
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     *
     * $ScheduleIDMappings = @{ 
                'MachinePolicy' = '{00000000-0000-0000-0000-000000000021}'; 
                'DiscoveryData' = '{00000000-0000-0000-0000-000000000003}'; 
                'ComplianceEvaluation' = '{00000000-0000-0000-0000-000000000071}'; 
                'AppDeployment' = '{00000000-0000-0000-0000-000000000121}'; 
                'HardwareInventory' = '{00000000-0000-0000-0000-000000000001}'; 
                'UpdateDeployment' = '{00000000-0000-0000-0000-000000000108}'; 
                'UpdateScan' = '{00000000-0000-0000-0000-000000000113}'; 
                'SoftwareInventory' = '{00000000-0000-0000-0000-000000000002}'; 

     */
    public enum SccmTriggerId
    {
        //MachinePolicy = "{00000000-0000-0000-0000-000000000021}", 
        //DiscoveryData = "{00000000-0000-0000-0000-000000000003}'; 
        //ComplianceEvaluation = "{00000000-0000-0000-0000-000000000071}'; 
        //AppDeployment = "{00000000-0000-0000-0000-000000000121}"; 
        //HardwareInventory = "{00000000-0000-0000-0000-000000000001}"; 
        //UpdateDeployment = "{00000000-0000-0000-0000-000000000108}"; 
        //UpdateScan = "{00000000-0000-0000-0000-000000000113}"; 
        //SoftwareInventory = "{00000000-0000-0000-0000-000000000002}";

        MachinePolicy = 21,
        DiscoveryData = 3,
        ComplianceEvaluation = 71,
        AppDeployment = 121,
        HardwareInventory = 1,
        UpdateDeployment = 108,
        UpdateScan = 113,
        SoftwareInventory = 2,




        ApplicationDeploymentEvaluationCycle = 121,
        DiscoveryDataCollectionCycle = 3,
        FileCollectionCycle = 10,
        HardwareInventoryCycle = 1,
        MachinePolicyRetrievalCycle = 21,
        MachinePolicyEvaluationCycle = 22,
        SoftwareInventoryCycle = 2,
        SoftwareMeteringUsageReportCycle = 31,
        SoftwareUpdatesAssignmentsEvaluationCycle = 108,
        SoftwareUpdateScanCycle = 113,
        StateMessageRefresh = 111,
        UserPolicyRetrievalCycle = 26,
        UserPolicyEvaluationCycle = 27,
        WindowsInstallersSourceListUpdateCycle = 32,
    }
}