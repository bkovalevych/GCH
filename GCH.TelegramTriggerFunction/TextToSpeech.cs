using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using GCH.Core.LoggerWrapper;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using GCH.Core.WordProcessing.Models;
using GCH.Core.WordProcessing.Requests.ProcessText;
using GCH.Infrastructure.TextToSpeech;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GCH.TelegramTriggerFunction
{
    public class TextToSpeech
    {
        private readonly IWrappedTelegramClient _client;
        private readonly IPublisher _publisher;
        private readonly LoggerWrapperService _logger;
        public TextToSpeech(
            IWrappedTelegramClient client,
            IPublisher publisher,
            LoggerWrapperService loggerWrapperService)
        {
            _logger = loggerWrapperService;
            _client = client;
            _publisher = publisher;    
        }

        [FunctionName("TextToSpeech")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob(
            blobPath: "voices",
            access: FileAccess.ReadWrite,
            Connection = "BlobConnectionString")] BlobContainerClient blobContainerClient,
            ILogger log)
        {
            _logger.Logger = log;
            try
            {
                var rawString = new StreamReader(req.Body).ReadToEnd();
                var update = JsonConvert.DeserializeObject<Update>(rawString);

                var text = update.InlineQuery?.Query ?? update.Message?.Text;
                await _publisher.Publish(new TelegramUpdateNotification()
                {
                    Update = update
                });
            } 
            catch (Exception ex)
            {
                log.LogError(ex, "unhandled ex. Message: {}", ex.Message);
            }
            return new OkResult();
        }

        private static async Task<Result<Stream>> GetVoice(string text)
        {
            var handler = new ProcessTextHandler();
            var request = new ProcessTextRequest()
            {
                Text = text
            };
            var itemsResult = await handler.Handle(request, default);
            var mappeditems = await itemsResult.MapAsync(ComposeItems);
            return mappeditems;
        }

        private static void Init(BlobContainerClient blobContainerClient)
        {
            var service = new TextToSpeechService("3d12c15443664fe589308947cdba7bbe", "westeurope");
            SetGetLabelFunction(blobContainerClient);
            SetGetTextFunction(service);
        }

        private static async Task<Stream> ComposeItems(List<IUnit> units)
        {
            var rawOutput = new MemoryStream();
            foreach (var unit in units)
            {
                var stream = await unit.Compose();
                var reader = new Mp3FileReader(stream);
                if ((rawOutput.Position == 0) && (reader.Id3v2Tag != null))
                {
                    rawOutput.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    rawOutput.Write(frame.RawData, 0, frame.RawData.Length);
                }
                stream.Dispose();
            }
            rawOutput.Position = 0;
            return rawOutput;
        }

        private static IActionResult HandleError(Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }

        private static void SetGetLabelFunction(BlobContainerClient blobContainerClient)
        {
            GCHLabelUnit.ComposeFunction = (unit) => blobContainerClient
            .GetBlobClient(unit.ShortName + ".mp3")
            .OpenReadAsync();
        }

        private static void SetGetTextFunction(TextToSpeechService service)
        {
            TextUnit.ComposeFunction = (unit) => service.FromText(unit.Text);
        }
    }
}
