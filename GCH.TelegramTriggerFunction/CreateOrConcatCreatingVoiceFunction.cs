using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Interfaces.Tables;
using GCH.Core.LoggerWrapper;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Infrastructure.OggReader;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
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
        private readonly IUserSettingsTable _userSettingsTable;
        private readonly LoggerWrapperService _loggerWrapper;
        private BlobContainerClient _blobVoicesContainerClient;
        private BlobContainerClient _blobCreatedContainerClient;
        private QueueMessageToAddVoice _currentMessage;

        public CreateOrConcatCreatingVoiceFunction(IWrappedTelegramClient client, IVoiceLabelSource source,
            OggReaderService oggReader, LoggerWrapperService loggerWrapper, IUserSettingsTable settingsTable)
        {
            _userSettingsTable = settingsTable;
            _loggerWrapper = loggerWrapper;
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
            _loggerWrapper.Logger = logger;
            _blobVoicesContainerClient = blobVoicesContainerClient;
            _blobCreatedContainerClient = blobCreatedContainerClient;

            await (from msg in Parse(rawMsg)
                   from voiceUrl in GetVoiceToAdd(msg)
                   from duration in AddVoice(msg, voiceUrl)
                   select (msg, duration))
                   .Match(
                SucceessResponse,
                (err) => FailResponse(err, _currentMessage));
        }

        private async Task SucceessResponse((QueueMessageToAddVoice, TimeSpan) msgAndSize)
        {
            var (msg, duration) = msgAndSize;
            var paged = await _source.LoadAsync(
                int.Parse(msg.ChatState.TryGetValue("offset", out var offset) ? offset : "0"));
            var settings = await _userSettingsTable.GetByChatId(msg.ChatId);
            Resources.Resources.Culture = new CultureInfo(settings.Language);

            var buttons = ChatVoiceHelpers.AddFooterButtons(paged, msg.FileName);
            var markup = new InlineKeyboardMarkup(buttons);
            await _client.Client.SendTextMessageAsync(
                msg.ChatId,
                string.Format(Resources.Resources.AfterVoiceAddedMessage, 
                duration, 
                Constants.MaxDuration),
                replyMarkup: markup);
        }

        private async Task FailResponse(Exception err, QueueMessageToAddVoice msg)
        {
            _loggerWrapper.Logger.LogError(err, "CreateOrConcatVoiceFunction.FailResponse. id = {}, voice = {}, message = {}",
                msg.ChatId, msg.FileName, err.Message);
            await _client.Client.SendTextMessageAsync(
                msg.ChatId,
                err.Message);
        }

        private TryAsync<QueueMessageToAddVoice> Parse(string rawMsg) => async () =>
        {
            var msg = JsonConvert.DeserializeObject<QueueMessageToAddVoice>(rawMsg);
            _currentMessage = msg;
            if (msg.Duration > Constants.MaxDuration)
            {
                var err = new Exception(@$"Too long voice. Current is {msg.Duration:mm\:ss\ff}. Max is {Constants.MaxDuration:mm\:ss\ff}.");
                return new Result<QueueMessageToAddVoice>(err);
            }
            return msg;
        };


        private TryAsync<Stream> GetVoiceToAdd(QueueMessageToAddVoice msg) => async () =>
        {
            var memStr = new MemoryStream();
            if (!string.IsNullOrEmpty(msg.VoiceLabelName))
            {
                var blob = _blobVoicesContainerClient
                    .GetBlobClient(msg.VoiceLabelName);
                await blob.DownloadToAsync(memStr);
            }
            else if (!string.IsNullOrEmpty(msg.ChatVoiceTelegramId))
            {
                var t = await _client.Client
                    .GetFileAsync(msg.ChatVoiceTelegramId);
                await _client.Client.DownloadFileAsync(t.FilePath, memStr);
            }
            memStr.Position = 0;
            return memStr;
        };


        private TryAsync<TimeSpan> AddVoice(QueueMessageToAddVoice msg, Stream voiceToAdd) => async () =>
        {
            var blobInstance = _blobCreatedContainerClient.GetBlobClient(msg.FileName + ".ogg");
            var exists = await blobInstance.ExistsAsync();
            var sumDuration = msg.Duration;
            BlobProperties properties;
            if (exists)
            {
                properties = await blobInstance.GetPropertiesAsync();
                if (!properties.Metadata.ContainsKey("Duration"))
                {
                    properties.Metadata["Duration"] = (await _oggReader.GetDuration(blobInstance.GenerateSasUri(
                        BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(1)))).ToString();
                    await blobInstance.SetMetadataAsync(properties.Metadata);
                }
                var firstDuration = TimeSpan.Parse(properties.Metadata["Duration"]);
                sumDuration += firstDuration;
                if (sumDuration > Constants.MaxDuration)
                {
                    return new Result<TimeSpan>(new Exception($"Too long duration. Current is {firstDuration:mm\\:ss\\:ff} " +
                        $"and you try to add {msg.Duration:mm\\:ss\\:ff} which is longer than {Constants.MaxDuration:mm\\:ss\\:ff}"));
                }

                using var firstStream = new MemoryStream();
                await blobInstance.DownloadToAsync(firstStream);
                firstStream.Position = 0;
                using var result = await _oggReader.Concat(firstStream, voiceToAdd);
                await blobInstance.UploadAsync(result, overwrite: true);
            }
            else
            {
                await blobInstance.UploadAsync(voiceToAdd);
                properties = await blobInstance.GetPropertiesAsync();
                properties.Metadata["Duration"] = sumDuration.ToString();
            }
            await blobInstance.SetMetadataAsync(properties.Metadata);

            _loggerWrapper.Logger.LogInformation("Voice was processed. Id {}, Voice {}, duration {}",
                msg.ChatId, msg.FileName, sumDuration);

            return sumDuration;
        };
    }
}
