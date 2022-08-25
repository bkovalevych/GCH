using Azure.Data.Tables;
using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.FfmpegHelpers;
using GCH.Core.Models;
using GCH.Infrastructure.Voices;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace GCH.TelegramTriggerFunction
{
    public class AddMem
    {
        private readonly IVoicesContainer _voiceContainer;
        private readonly TableClient _voiceLabels;
        private readonly IOggReaderService _oggService;

        public AddMem(IVoicesContainer voicesContainer, TableClient voiceLabels, IOggReaderService oggService)
        {
            _voiceContainer = voicesContainer;
            _voiceLabels = voiceLabels;
            _oggService = oggService;
        }

        [FunctionName("AddMem")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var reqFormData = await req.ReadFormAsync();

            var blobName = reqFormData["blobName"][0];
            var label = reqFormData["label"][0];
            var file = reqFormData.Files["data"];
            var t = await AddLabel(blobName, label)
                .Bind(it => AddBlob(blobName, file));
            var result = t.Match<Unit, IActionResult>(
                unit => { return new OkObjectResult("This HTTP triggered function executed successfully."); },
                err => { return new BadRequestObjectResult(err.Message); });
            return result;
        }

        private TryAsync<Unit> AddLabel(string blobName, string label) => async () =>
        {
            var pages = _voiceLabels.QueryAsync<VoiceLabelTableEntity>();
            var max = 0;
            var blobFileName = $"app_{blobName}.ogg";
            await foreach (var voice in pages)
            {
                max = Math.Max(max, voice.Id);
                if (blobFileName == voice.Blob)
                {
                    var ex = new ArgumentException("Label is not unique");
                    return new Result<Unit>(ex);
                }
            }

            var id = max + 1;
            var voiceLabel = new VoiceLabelTableEntity()
            {
                Id = id,
                Blob = blobFileName,
                RowKey = id.ToString(),
                PartitionKey = "ogg",
                Description = label,
                Name = label
            };
            await _voiceLabels.AddEntityAsync(voiceLabel);
            return Unit.Default;
        };

        private TryAsync<Unit> AddBlob(string blobName, IFormFile file) => async () =>
        {
            using var inpStrm = file.OpenReadStream();
            using var outputStream = await _oggService.ProcessVoiceMem(inpStrm);
            await _voiceContainer.BlobContainer.UploadBlobAsync($"app_{blobName}.ogg", outputStream);
            return Unit.Default;
        };
    }
}
