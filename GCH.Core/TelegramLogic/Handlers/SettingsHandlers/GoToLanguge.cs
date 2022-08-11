using GCH.Core.Interfaces.Tables;
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
        private readonly IUserSettingsTable _settingsTable;

        public GoToLanguge(IWrappedTelegramClient client, IUserSettingsTable settingsTable) : base(client)
        {
            _settingsTable = settingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, 
            CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _settingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            var markup = new InlineKeyboardMarkup(new InlineKeyboardButton[][] 
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("en" == settings.Language? "en (selected)": "en")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageEn
                    },
                    new InlineKeyboardButton("ua" == settings.Language? "ua (selected)": "ua")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageUa
                    },
                    new InlineKeyboardButton("ru" == settings.Language? "ru (selected)": "ru")
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
