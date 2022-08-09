using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class BackToSettings : AbstractTelegramHandler
    {
        public BackToSettings(IWrappedTelegramClient client) : base(client)
        {
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var languageBtn = new InlineKeyboardButton("Language") 
            { 
                CallbackData = Constants.SettingsButtons.Language
            };
            
            var markup = new InlineKeyboardMarkup(languageBtn);

            _ = await ClientWrapper.Client.EditMessageTextAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId,
                "Choose settings to change:", 
                replyMarkup: markup, 
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data == Constants.SettingsButtons.Settings;
        }
    }
}
