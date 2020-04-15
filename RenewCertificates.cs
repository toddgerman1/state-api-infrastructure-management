using System;
using System.IO;
using System.Threading.Tasks;
using Fathym;
using LCU.Presentation.State.ReqRes;
using LCU.StateAPI;
using LCU.StateAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LCU.State.API.NapkinIDE.InfrastructureManagement
{
    public class EnsureCertRenewal
    {
        #region API Methods
        [FunctionName("RenewCertificatesTimer")]
        public virtual async Task RunTimer([TimerTrigger("0 0 1 * * *", RunOnStartup = true)]TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation($"Ensuring Certificate Renewals via Timer: {DateTime.Now}");

            var instanceId = await runAction(starter, log);
        }

        [FunctionName("RenewCertificates")]
        public virtual async Task<IActionResult> RunAPI([HttpTrigger]HttpRequest req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation($"Ensuring Certificate Renewals via API");

            var instanceId = await runAction(starter, log);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        #endregion

        #region Helpers
        public virtual async Task<string> runAction(IDurableOrchestrationClient starter, ILogger log)
        {
            var entApiKey = Environment.GetEnvironmentVariable("LCU-ENTERPRISE-API-KEY");

            try
            {
                var instanceId = await starter.StartAction("RenewCertificatesOrchestration", new StateDetails()
                {
                    EnterpriseAPIKey = entApiKey
                }, new ExecuteActionRequest()
                {
                    Arguments = new
                    {
                        EnterpriseAPIKey = entApiKey
                    }.JSONConvert<MetadataModel>()
                }, log);

                return instanceId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
