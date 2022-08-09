using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class GoToLanguge : AbstractTelegramHandler
    {
        public GoToLanguge(IWrappedTelegramClient client) : base(client)
        {
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, 
            CancellationToken cancellationToken)
        {
            var upd = notification.Update;

            var markup = new InlineKeyboardMarkup(new InlineKeyboardButton[][] 
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("En")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageEn
                    },
                    new InlineKeyboardButton("Ua")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageUa
                    },
                    new InlineKeyboardButton("Ru")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageRu
                    }
                },
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("Back to settings")
                    {
                        CallbackData = Constants.SettingsButtons.Settings
                    }
                }
            });

            _ = await ClientWrapper.Client.EditMessageTextAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId,
                "Choose language",
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data == Constants.SettingsButtons.Language;
        }
    }
}
