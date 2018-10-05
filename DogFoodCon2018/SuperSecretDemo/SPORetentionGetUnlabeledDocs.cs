using BTJ.SPO.FuncCommon;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Diagnostics;
using System.Web;

namespace BTJ.SPO.AutoApplyRetentionLabel
{
    public static class SPORetentionGetUnlabeledDocs
    {
        private static string _key = TelemetryConfiguration.Active.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
        private static TelemetryClient telemetry = new TelemetryClient() { InstrumentationKey = _key };

        private static string _authorityUri = TelemetryConfiguration.Active.InstrumentationKey = Environment.GetEnvironmentVariable("authorityUri", EnvironmentVariableTarget.Process);
        private static AuthenticationContext ac = new AuthenticationContext(_authorityUri, false);

        [FunctionName("SPORetention-GetUnlabeledDocs-func")]
        [Disable("disable-SPORetention-GetUnlabeledDocs-func")]
        public static void Run([TimerTrigger("%unlabeledDocumentTimerSchedule%")]TimerInfo myTimer, [Queue("%unlabeledDocsQueueName%", Connection = "retentionStorage")]ICollector<UnlabeledDocument> queueData, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            GetUnlabeledDocsFromSearch(queueData, log);
        }

        public static void GetUnlabeledDocsFromSearch(ICollector<UnlabeledDocument> queueData, TraceWriter log)
        {
            Stopwatch sw = new Stopwatch();

            //var ac = new AuthenticationContext(Constants.AuthorityUri, false);
            var cert = Utils.GetCertificate(Constants.AADAppCertificateThumbprint);

            ClientAssertionCertificate cac = new ClientAssertionCertificate(Constants.AADAppClientId, cert);

            var authResult = ac.AcquireTokenAsync(Constants.ResourceUri, cac).Result;

            using (ClientContext cc = new ClientContext(Constants.TenantRootUrl))
            {
                cc.ExecutingWebRequest += (s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + authResult.AccessToken;
                };

                ClientResult<ResultTableCollection> results = new ClientResult<ResultTableCollection>();
                KeywordQuery keywordQuery = new KeywordQuery(cc);
                
                int startRow = 0;
                var defaultRetentionLabel = Environment.GetEnvironmentVariable("defaultRetentionLabel");

                sw.Start();
                do
                {
                    keywordQuery.StartRow = startRow;
                    keywordQuery.QueryText = Constants.KeywordQueryForComplianceTag;
                    keywordQuery.RowLimit = 500;
                    keywordQuery.TrimDuplicates = false;

                    SearchExecutor searchExec = new SearchExecutor(cc);
                    results = searchExec.ExecuteQuery(keywordQuery);
                    cc.ExecuteQuery();

                    if (results != null)
                    {
                        if (results.Value[0].RowCount > 0)
                        {
                            foreach (var row in results.Value[0].ResultRows)
                            {
                                var path = row["Path"].ToString();
                                var byteEncodedPath = HttpUtility.UrlEncode(row["Path"].ToString());
                                var byteEncodedWebUrl = HttpUtility.UrlEncode(row["SPWebUrl"].ToString());

                                log.Info("Unlabeled document at: " + path);

                                queueData.Add(new UnlabeledDocument(byteEncodedWebUrl, byteEncodedPath, Constants.DefaultRetentionLabel));
                            }
                        }
                    }

                    startRow += results.Value[0].RowCount;
                } while (results.Value[0].TotalRowsIncludingDuplicates > startRow);
                sw.Stop();
                telemetry.TrackMetric("CSOM-GetUnlabeledDocs", sw.ElapsedMilliseconds);
            }
        }
    }
}