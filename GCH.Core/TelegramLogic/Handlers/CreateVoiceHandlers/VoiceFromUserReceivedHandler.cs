using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Queues;
using GCH.Core.Interfaces.Tables;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class VoiceFromUserReceivedHandler : AbstractTelegramHandler
    {
        private readonly EventGridPublisherClient _publisher;

        public VoiceFromUserReceivedHandler(
            IWrappedTelegramClient client,
            IConfiguration configuration,
            IUserSettingsTable userSettingsTable)
            : base(client, userSettingsTable)
        {
            _publisher = new EventGridPublisherClient(
                new Uri(configuration["EventGridEndpoint"]),
                new AzureKeyCredential(configuration["EventGridCreds"]));
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            if (upd.Message.Voice.Duration > Constants.MaxDuration.TotalSeconds)
            {
                await ClientWrapper.Client.SendTextMessageAsync(
                    upd.Message.Chat.Id,
                    @$"Voice too long. Bigger than {Constants.MaxDuration:mm\:ss\:ff}",
                    cancellationToken: cancellationToken);
                return;
            }

            var fileName = UserSettings.LastVoiceId;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
                UserSettings.LastVoiceId = fileName;
                await UserSettingsTable.SetSettings(UserSettings);
            }

            var msg = new QueueMessageToAddVoice()
            {
                ChatId = upd.Message.Chat.Id,
                ChatVoiceTelegramId = upd.Message.Voice.FileId,
                FileName = fileName,
                ChatState = new Dictionary<string, string>(),
                Duration = TimeSpan.FromSeconds(upd.Message.Voice.Duration)
            };
            
            var eventInstance = new EventGridEvent("voices", "added", "v1", msg);
            await _publisher.SendEventAsync(eventInstance);

            await ClientWrapper.Client.SendTextMessageAsync(upd.Message.Chat.Id,
                string.Format(Resources.Resources.VoiceAddedAlert, "user Voice"),
                cancellationToken: cancellationToken);
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                 && notification.Update.Message.Type == MessageType.Voice;
        }
    }
}
