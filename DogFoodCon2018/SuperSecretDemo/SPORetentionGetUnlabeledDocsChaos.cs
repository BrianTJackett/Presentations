using BTJ.SPO.FuncCommon;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Search.Query;
using System;
using System.Web;

namespace BTJ.SPO.AutoApplyRetentionLabel
{
    public static class SPORetentionGetUnlabeledDocsChaos
    {
        [FunctionName("SPORetention-GetUnlabeledDocsChaos-func")]
        [Disable("disable-SPORetention-GetUnlabeledDocsChaos-func")]
        public static void Run([TimerTrigger("%chaosDocumentTimerSchedule%")]TimerInfo myTimer, [Queue("%unlabeledDocsQueueName%", Connection = "retentionStorage")]ICollector<UnlabeledDocument> queueData, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            GetUnlabeledDocsFromSearch(queueData, log);
        }

        public static void GetUnlabeledDocsFromSearch(ICollector<UnlabeledDocument> queueData, TraceWriter log)
        {
            var url = Environment.GetEnvironmentVariable("tenantRootUrl");
            var thumbprint = Environment.GetEnvironmentVariable("certificateThumbprint");
            var resourceUri = Environment.GetEnvironmentVariable("resourceUri");
            var authorityUri = Environment.GetEnvironmentVariable("authorityUri");
            var clientId = Environment.GetEnvironmentVariable("clientId");

            var ac = new AuthenticationContext(authorityUri, false);

            var cert = Utils.GetCertificate(thumbprint);

            ClientAssertionCertificate cac = new ClientAssertionCertificate(clientId, cert);

            var authResult = ac.AcquireTokenAsync(resourceUri, cac).Result;

            using (ClientContext cc = new ClientContext(url))
            {
                cc.ExecutingWebRequest += (s, e) =>
                {
                    e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + authResult.AccessToken;
                };

                ClientResult<ResultTableCollection> results = new ClientResult<ResultTableCollection>();
                KeywordQuery keywordQuery = new KeywordQuery(cc);
                string keywordQueryValue = Environment.GetEnvironmentVariable("keywordQueryForComplianceTagChaos").Replace(@"\", "");

                Random random = new Random();

                int startRow = 0;
                do
                {
                    keywordQuery.QueryText = keywordQueryValue;
                    keywordQuery.RowLimit = 500;
                    keywordQuery.StartRow = startRow;

                    SearchExecutor searchExec = new SearchExecutor(cc);
                    results = searchExec.ExecuteQuery(keywordQuery);
                    cc.ExecuteQuery();

                    if (results != null)
                    {
                        if (results.Value[0].RowCount > 0)
                        {
                            foreach (var row in results.Value[0].ResultRows)
                            {
                                var labelToApply = Environment.GetEnvironmentVariable("defaultRetentionLabelChaos");
                                var byteEncodedPath = HttpUtility.UrlEncode(row["Path"].ToString());
                                var byteEncodedWebUrl = HttpUtility.UrlEncode(row["SPWebUrl"].ToString());

                                log.Info(row["Path"].ToString() + " with label " + labelToApply);

                                //TODO decision: should we check for "read-only" mode per function or make it a global setting?
                                if (Environment.GetEnvironmentVariable("SendOutputToQueue").ToLower() == "true")
                                {
                                    if (random.Next(0, 10) % 10 == 0)
                                    {
                                        queueData.Add(new UnlabeledDocument(byteEncodedWebUrl, byteEncodedPath, labelToApply));
                                    }
                                }
                            }
                        }
                    }

                    startRow += results.Value[0].RowCount;
                } while (results.Value[0].TotalRows > results.Value[0].RowCount + startRow);
            }
        }
    }
}