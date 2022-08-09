using GCH.Core.TelegramLogic.Interfaces;
using GCH.Infrastructure.TelegramBot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace GCH.TelegramTriggerFunction
{
    public class SetWebHook
    {
        private readonly IWrappedTelegramClient _client;

        public SetWebHook(IWrappedTelegramClient client)
        {
            _client = client;
        }

        [FunctionName("SetWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var url = new StreamReader(req.Body).ReadToEnd();
            var text = "url was not specified";
            if (!string.IsNullOrEmpty(url))
            {
                await _client.SetWebhookAsync(url);
                text = $"url has changed to {url}";
                log.LogInformation(text);
            }
            return new OkObjectResult(text);
        }
    }
}
