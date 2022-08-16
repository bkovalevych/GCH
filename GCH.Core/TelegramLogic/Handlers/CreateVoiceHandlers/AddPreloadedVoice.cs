using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.FfmpegHelpers;
using GCH.Core.Interfaces.Tables;
using GCH.Core.LoggerWrapper;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class AddPreloadedVoice : AbstractTelegramHandler
    {
        private readonly IOggReaderService _oggReaderService;
        private readonly IUserSettingsTable _settingsTable;
        private readonly QueueClient _queueClient;
        private readonly IVoicesContainer _voicesContainer;
        private readonly LoggerWrapperService _loggerWrapper;

        private ILogger Logger { get => _loggerWrapper.Logger; }

        public AddPreloadedVoice(IWrappedTelegramClient client, QueueClient queueClient,
            IUserSettingsTable settingsTable, IVoicesContainer voicesContainer,
            IOggReaderService oggReaderService,
            LoggerWrapperService loggerWrapper) : base(client)
        {
            _oggReaderService = oggReaderService;
            _settingsTable = settingsTable;
            _queueClient = queueClient;
            _voicesContainer = voicesContainer;
            _loggerWrapper = loggerWrapper;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {

            await (from fileName in GetFileName(notification)
                   from properties in GetBlobProperties(notification)
                   from duration in GetDuration(notification, properties)
                   from _ in SendMessage(notification, fileName, duration)
                   select _)
                   .Match(_ => { },
                   ex =>
                   {
                       Logger.LogError(ex, "ChatId: {}, CallBack: {}, Message: {}",
                           notification.Update.CallbackQuery.Message.Chat.Id,
                           notification.Update.CallbackQuery.Data,
                           ex.Message);
                   });
        }

        private TryAsync<Unit> SendMessage(TelegramUpdateNotification notification, string fileName, TimeSpan duration)
        => async () =>
        {
            var upd = notification.Update;
            var msg = new QueueMessageToAddVoice()
            {
                ChatId = upd.CallbackQuery.Message.Chat.Id,
                VoiceLabelName = GetBlobName(notification),
                FileName = fileName,
                ChatState = ChatVoiceHelpers.GetState(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard),
                Duration = duration
            };
            await _queueClient.SendMessageAsync(Convert.ToBase64String(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg))));
            return Unit.Default;
        };

        private TryAsync<string> GetFileName(TelegramUpdateNotification notification)
        => async () =>
        {
            var upd = notification.Update;
            var fileName = ChatVoiceHelpers.GetFileName(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard);
            var settings = await _settingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
                settings.LastVoiceId = fileName;
                await _settingsTable.SetSettings(settings);
            }
            return fileName;
        };

        private string GetBlobName(TelegramUpdateNotification notification)
        {
            var upd = notification.Update;
            var blobName = upd.CallbackQuery.Data[Constants.CreateVoiceButtons.ContnetPrefix.Length..];
            return blobName;
        }

        private TryAsync<BlobProperties> GetBlobProperties(TelegramUpdateNotification notification) => async () =>
        {
            var blobName = GetBlobName(notification);
            var blob = _voicesContainer.BlobContainer.GetBlobClient(blobName);
            var props = await blob.GetPropertiesAsync();
            return props.Value;
        };

        private TryAsync<TimeSpan> GetDuration(TelegramUpdateNotification notification, BlobProperties props)
        => async () =>
        {
            var upd = notification.Update;
            var blobName = GetBlobName(notification);
            var blob = _voicesContainer.BlobContainer.GetBlobClient(blobName);
            if (!props.Metadata.ContainsKey("Duration"))
            {
                var temp = await _oggReaderService.GetDuration(
                    blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read,
                    DateTimeOffset.Now.AddMinutes(1)));
                props.Metadata["Duration"] = temp.ToString();
                await blob.SetMetadataAsync(props.Metadata);
            }
            var duration = TimeSpan.Parse(props.Metadata["Duration"]);
            return duration;
        };

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.ContnetPrefix);
        }
    }
}
