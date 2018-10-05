using BTJ.SPO.FuncCommon;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using System;
using System.Diagnostics;

namespace BTJ.SPO.AutoApplyRetentionLabel
{
    public static class SPORetentionSetDefaultLabel
    {
        private static string _key = TelemetryConfiguration.Active.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
        private static TelemetryClient telemetry = new TelemetryClient() { InstrumentationKey = _key };

        private static string _authorityUri = TelemetryConfiguration.Active.InstrumentationKey = Environment.GetEnvironmentVariable("authorityUri", EnvironmentVariableTarget.Process);
        private static AuthenticationContext ac = new AuthenticationContext(_authorityUri, false);

        [FunctionName("SPORetention-SetDefaultLabel-func")]
        [Disable("disable-SPORetention-SetDefaultLabel-func")]
        public static void Run([QueueTrigger("%unlabeledDocsQueueName%", Connection = "retentionStorage")]UnlabeledDocument myQueueItem, [Queue("%failedToLabelQueueName%", Connection = "retentionStorage")]ICollector<UnlabeledDocument> failedDocData, TraceWriter log)
        {
            // null check to avoid issues with Azure Portal to local VS debugging latency resulting in empty item attempted to process
            if (myQueueItem != null)
            {
                log.Info($"C# Queue trigger function processed: {myQueueItem.DocPath}");

                SetComplianceTag(myQueueItem, failedDocData, log);
            }
        }

        public static void SetComplianceTag(UnlabeledDocument myQueueItem, ICollector<UnlabeledDocument> failedDocData, TraceWriter log)
        {
            Stopwatch sw = new Stopwatch();

            string resourceUri = null;

            var decodedWebUrl = System.Web.HttpUtility.UrlDecode(myQueueItem.WebUrl);
            var decodedPathUrl = System.Web.HttpUtility.UrlDecode(myQueueItem.DocPath);

            //temp fix to skip over dummy data stuck in system
            if (decodedPathUrl.Contains("dummyFilesToUpload"))
            {
                return;
            }


            if (decodedWebUrl.Contains("-my."))
            {
                resourceUri = Constants.ResourceMySiteUri;
            }
            else
            {
                resourceUri = Constants.ResourceUri;
            }

            //var ac = new AuthenticationContext(authorityUri, false);

            var cert = Utils.GetCertificate(Constants.AADAppCertificateThumbprint);
            ClientAssertionCertificate cac = new ClientAssertionCertificate(Constants.AADAppClientId, cert);

            var authResult = ac.AcquireTokenAsync(resourceUri, cac).Result;

            using (ClientContext cc = new ClientContext(decodedWebUrl))
            {
                cc.ExecutingWebRequest += (s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + authResult.AccessToken;
                };

                try
                {
                    var decodedDocPath = System.Web.HttpUtility.UrlDecode(myQueueItem.DocPath);
                    var listItem = cc.Web.GetListItem(decodedDocPath);
                    var parentList = listItem.ParentList;

                    cc.Load(listItem, li => li.ComplianceInfo);
                    cc.Load(parentList, pl => pl.BaseTemplate);

                    sw.Start();
                    cc.ExecuteQuery();
                    sw.Stop();
                    telemetry.TrackMetric("CSOM-GetComplianceInfo", sw.ElapsedMilliseconds);

                    // check for document library is a Preservation Hold Library
                    // not able to compare against List.ListTemplateType since not included in the enumeration, therefore using the current value of 1310
                    if (parentList.BaseTemplate == 1310)
                    {
                        log.Verbose("File is in a preservation hold library, skipping file: " + myQueueItem.DocPath);
                    }
                    else
                    {
                        listItem.SetComplianceTag(myQueueItem.RetentionLabel, false, false, false, false);

                        // use SystemUpdate() to avoid issues with published or checked out files
                        listItem.SystemUpdate();

                        log.Info("Applied label: " + decodedDocPath);

                        sw.Restart();
                        cc.ExecuteQuery();
                        sw.Stop();
                        telemetry.TrackMetric("CSOM-SetComplianceTag", sw.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Unable to apply compliance tag for: " + myQueueItem.DocPath, ex);
                    failedDocData.Add(myQueueItem);
                    throw ex;
                }
            }
        }
    }
}
