using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class CancelCreationVoiceHandler : AbstractTelegramHandler
    {
        public CancelCreationVoiceHandler(IWrappedTelegramClient client) : base(client)
        {
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            
            await ClientWrapper.Client.DeleteMessageAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId, 
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data == Constants.CreateVoiceButtons.Cancel;
        }
    }
}
