using Azure.Storage.Blobs;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Infrastructure.OggReader;
using LanguageExt;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.TelegramTriggerFunction
{
    public class CreateOrConcatCreatingVoiceFunction
    {
        private readonly IWrappedTelegramClient _client;
        private readonly IVoiceLabelSource _source;
        private readonly OggReaderService _oggReader;
        private ILogger _logger;
        private BlobContainerClient _blobVoicesContainerClient;
        private BlobContainerClient _blobCreatedContainerClient;

        public CreateOrConcatCreatingVoiceFunction(IWrappedTelegramClient client, IVoiceLabelSource source,
            OggReaderService oggReader)
        {
            _client = client;
            _source = source;
            _oggReader = oggReader;
        }

        [FunctionName("CreateOrConcatCreatingVoiceFunction")]
        public async Task Run(
            [QueueTrigger(queueName: "voices", Connection = "BlobConnectionString")] string rawMsg,
            [Blob(
            blobPath: "voices",
            access: FileAccess.ReadWrite,
            Connection = "BlobConnectionString")] BlobContainerClient blobVoicesContainerClient,
            [Blob(
            blobPath: "uservoices",
            access: FileAccess.ReadWrite,
            Connection = "BlobConnectionString")] BlobContainerClient blobCreatedContainerClient,
            ILogger logger)
        {
            _logger = logger;
            _blobVoicesContainerClient = blobVoicesContainerClient;
            _blobCreatedContainerClient = blobCreatedContainerClient;

            await (from msg in Parse(rawMsg)
                   from voice in GetVoiceToAdd(msg)
                   from duration in AddVoice(msg, voice)
                   select (msg, duration))
                   .Match(SucceessResponse, async (err) => { });
        }

        private async Task SucceessResponse((QueueMessageToAddVoice, TimeSpan) msgAndSize)
        {
            var (msg, duration) = msgAndSize;
            var paged = await _source.LoadAsync(
                int.Parse(msg.ChatState.TryGetValue("offset", out var offset) ? offset : "0"));
            var buttons = ChatVoiceHelpers.AddFooterButtons(paged, msg.FileName);
            var markup = new InlineKeyboardMarkup(buttons);
            await _client.Client.SendTextMessageAsync(
                msg.ChatId,
                $"Voice added. Duration - {duration:mm\\:ss}. Max duration is {Constants.MaxDuration:mm\\:ss}",
                replyMarkup: markup);
        }

        private async Task FailResponse(Exception err, QueueMessageToAddVoice msg)
        {
            _logger.LogInformation(err, "CreateOrConcatVoiceFunction.FailResponse. id = {0}, voice = {1}", msg.ChatId, msg.FileName);
            await _client.Client.SendTextMessageAsync(
                msg.ChatId,
                err.Message);
        }

        private TryAsync<QueueMessageToAddVoice> Parse(string rawMsg)
        {
            return new TryAsync<QueueMessageToAddVoice>(
                async () => JsonConvert.DeserializeObject<QueueMessageToAddVoice>(rawMsg));
        }

        private TryAsync<Stream> GetVoiceToAdd(QueueMessageToAddVoice msg)
        {
            return new TryAsync<Stream>(async () =>
            {
                var stream = new MemoryStream();
                if (!string.IsNullOrEmpty(msg.VoiceLabelName))
                {
                    var blob = _blobVoicesContainerClient
                        .GetBlobClient(msg.VoiceLabelName);
                    await blob.DownloadToAsync(stream);
                }
                else if (!string.IsNullOrEmpty(msg.ChatVoiceTelegramId))
                {
                    var t = await _client.Client
                        .GetFileAsync(msg.ChatVoiceTelegramId);
                    await _client.Client
                        .DownloadFileAsync(t.FilePath, stream);
                }
                stream.Position = 0;
                var duration = await _oggReader.GetDuration(stream);
                if (duration > Constants.MaxDuration)
                {
                    var err = new Exception($"Too long duration. Current is {duration:mm\\:ss}. Max is {Constants.MaxDuration:mm\\:ss}.");
                    await FailResponse(err, msg);
                    throw err;
                }
                return stream;
            });
        }

        private TryAsync<TimeSpan> AddVoice(QueueMessageToAddVoice msg, Stream voiceToAdd)
        {
            return new TryAsync<TimeSpan>(async () =>
            {
                var blobInstance = _blobCreatedContainerClient.GetBlobClient(msg.FileName + ".ogg");
                var exists = await blobInstance.ExistsAsync();
                var duration = TimeSpan.Zero;
                if (exists)
                {
                    using var exStream = new MemoryStream();
                    await blobInstance.DownloadToAsync(exStream);
                    exStream.Position = 0;
                    var fisrtDuration = await _oggReader.GetDuration(exStream);
                    var secondDuration = await _oggReader.GetDuration(voiceToAdd);
                    var sumDuration = fisrtDuration + secondDuration;
                    if (sumDuration > Constants.MaxDuration)
                    {
                        var err = new Exception($"Too long duration. Current is {duration:mm\\:ss}. Max is {Constants.MaxDuration:mm\\:ss}.");
                        await FailResponse(err, msg);
                        throw err;
                    }
                    var result = await _oggReader.ConcatStreams(exStream, voiceToAdd);
                    duration = await _oggReader.GetDuration(result);
                    await blobInstance.UploadAsync(result, overwrite: true);
                }
                else
                {
                    await blobInstance.UploadAsync(voiceToAdd);
                    duration = await _oggReader.GetDuration(voiceToAdd);
                }
                _logger.LogInformation("Voice was processed. Id {0}, Voice {1}, duration {2}", msg.ChatId, msg.FileName, duration);
                voiceToAdd.Dispose();
                return duration;
            });
        }

    }
}
