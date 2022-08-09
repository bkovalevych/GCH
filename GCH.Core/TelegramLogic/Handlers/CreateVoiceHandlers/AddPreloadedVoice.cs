using Azure.Storage.Queues;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class AddPreloadedVoice : AbstractTelegramHandler
    {
        private readonly QueueClient _queueClient;

        public AddPreloadedVoice(IWrappedTelegramClient client, QueueClient queueClient) : base(client)
        {
            _queueClient = queueClient;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var fileName = ChatVoiceHelpers.GetFileName(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = Guid.NewGuid().ToString();
            }
            var msg = new QueueMessageToAddVoice()
            {
                ChatId = upd.CallbackQuery.Message.Chat.Id,
                VoiceLabelName = upd.CallbackQuery.Data[Constants.CreateVoiceButtons.ContnetPrefix.Length..],
                FileName = fileName,
                ChatState = ChatVoiceHelpers.GetState(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard)
            };
            await _queueClient.SendMessageAsync(Convert.ToBase64String(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg))), cancellationToken);
        }

        

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.ContnetPrefix);
        }
    }
}
