using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using GCH.Core.Models;
using Azure.Storage.Queues;
using System.Text;
using Newtonsoft.Json;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class VoiceFromUserReceivedHandler : AbstractTelegramHandler
    {
        private readonly QueueClient _queueClient;
        private readonly IUserSettingsTable _settingsTable;

        public VoiceFromUserReceivedHandler(
            IWrappedTelegramClient client,
            QueueClient queueClient,
            IUserSettingsTable userSettingsTable) : base(client)
        {
            _queueClient = queueClient;
            _settingsTable = userSettingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            if (upd.Message.Voice.Duration > 60)
            {
                await ClientWrapper.Client.SendTextMessageAsync(
                    upd.Message.Chat.Id,
                    "Voice too long. Bigger than 60sec");
                return;
            }
            var settings = await _settingsTable.GetByChatId(upd.Message.Chat.Id);
            var fileName = settings.LastVoiceId;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
                settings.LastVoiceId = fileName;
                await _settingsTable.SetSettings(settings);
            }
            
            var msg = new QueueMessageToAddVoice()
            {
                ChatId = upd.Message.Chat.Id,
                ChatVoiceTelegramId = upd.Message.Voice.FileId,
                FileName = fileName,
                ChatState = new Dictionary<string, string>()
            };
            await _queueClient.SendMessageAsync(Convert.ToBase64String(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg))), 
                cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                 && notification.Update.Message.Type == MessageType.Voice;
        }
    }
}
