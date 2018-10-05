using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTJ.SPO.AutoApplyRetentionLabel
{
    internal class Constants
    {
        //const string AzureServicesAuthConnectionString = "AzureServicesAuthConnectionString";
        //const string AzureWebJobsDashboard = "AzureWebJobsDashboard";
        //const string AzureWebJobsStorage = "AzureWebJobsStorage";
        //const string AzureWebJobsDisableHomepage = "AzureWebJobsDisableHomepage";
        //const string RetentionStorage = "retentionStorage";
        //const string ExternalStorageAccountLabel = "ExternalStorageAccount";

        //const string TenantRootUrlLabel = "tenantRootUrl";
        //const string TenantAdminUrlLabel = "tenantAdminUrl";
        //const string ResourceUriLabel = "resourceUri";
        //const string ResourceMySiteUriLabel = "resourceMySiteUri";
        //const string AuthorityUriLabel = "authorityUri";

        //const string DefaultRetentionLabelLabel = "defaultRetentionLabel";
        //const string KeywordQueryForNoComplianceTagLabel = "keywordQueryForNoComplianceTag";

        //const string DefaultRetentionLabelChaosLabel = "defaultRetentionLabelChaos";
        //const string KeywordQueryForComplianceTagChaosLabel = "keywordQueryForComplianceTagChaos";

        //const string ClientIdLabel = "clientId";
        //const string CertificateThumbprintLabel = "certificateThumbprint";

        //const string UnlabeledDocsQueueNameLabel = "unlabeledDocsQueueName";
        //const string FailedToLabelQueueNameLabel = "failedToLabelQueueName";

        //const string UnlabeledDocumentTimerSchedule = "unlabeledDocumentTimerSchedule";
        //const string ChaosDocumentTimerSchedule = "chaosDocumentTimerSchedule";
        //const string QueueMonitorTimerSchedule = "QueueMonitorTimerSchedule";

        //const string AppInsightsInstrumentationKeyLabel = "APPINSIGHTS_INSTRUMENTATIONKEY";

        //const string DisableSPORetentionGetUnlabeledDocsFunc = "disable-SPORetention-GetUnlabeledDocs-func";
        //const string DisableSPORetentionGetUnlabeledDocsChaosFunc = "disable-SPORetention-GetUnlabeledDocsChaos-func";
        //const string DisableSPORetentionSetDefaultLabelFunc = "disable-SPORetention-SetDefaultLabel-func";


        //internal static string ExternalStorageAccount = Environment.GetEnvironmentVariable("ExternalStorageAccount");

        internal static string AADAppClientId = Environment.GetEnvironmentVariable("clientId");
        internal static string AADAppCertificateThumbprint = Environment.GetEnvironmentVariable("certificateThumbprint");

        internal static string TenantRootUrl = Environment.GetEnvironmentVariable("tenantRootUrl");
        internal static string ResourceUri = Environment.GetEnvironmentVariable("resourceUri");
        internal static string ResourceMySiteUri = Environment.GetEnvironmentVariable("resourceMySiteUri");
        //internal static string AuthorityUri = Environment.GetEnvironmentVariable("authorityUri");

        internal static string KeywordQueryForComplianceTag = (Environment.GetEnvironmentVariable("keywordQueryForComplianceTag").Replace(@"\", ""));
        internal static string KeywordQueryForComplianceTagChaos = (Environment.GetEnvironmentVariable("keywordQueryForComplianceTagChaos").Replace(@"\", ""));

        //internal static string AppInsightsInstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

        internal static string DefaultRetentionLabel = Environment.GetEnvironmentVariable("defaultRetentionLabel");
        internal static string DefaultRetentionLabelChaos = Environment.GetEnvironmentVariable("defaultRetentionLabelChaos");
    }
}
