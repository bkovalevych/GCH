using GCH.Core.Interfaces.Sources;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class GoToCreateVoiceHandler : AbstractTelegramHandler
    {
        private readonly IVoiceLabelSource _voiceSource;

        public GoToCreateVoiceHandler(IWrappedTelegramClient client, IVoiceLabelSource voiceSource) : base(client)
        {
            _voiceSource = voiceSource;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var pagedResult = await _voiceSource.LoadAsync();
            var buttons = ChatVoiceHelpers.AddFooterButtons(pagedResult, "");

            var markup = new InlineKeyboardMarkup(buttons);

            _ = await ClientWrapper.Client.SendTextMessageAsync(
                upd.Message.Chat.Id,
                "Send me a voice or choose from existing:",
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/create";
        }
    }
}
