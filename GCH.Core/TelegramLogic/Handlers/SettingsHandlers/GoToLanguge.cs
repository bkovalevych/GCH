using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using System.Globalization;
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
            Resources.Resources.Culture = new CultureInfo(settings.Language);
            var markup = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("en" == settings.Language? $"en ({Resources.Resources.Selected})": "en-US")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageEn
                    },
                    new InlineKeyboardButton("ua" == settings.Language? $"ua ({Resources.Resources.Selected})": "uk-UA")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageUa
                    },
                    new InlineKeyboardButton("ru" == settings.Language? $"ru ({Resources.Resources.Selected})": "ru-RU")
                    {
                        CallbackData = Constants.SettingsButtons.LanguageRu
                    }
                },
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton(Resources.Resources.BackToSettings)
                    {
                        CallbackData = Constants.SettingsButtons.Settings
                    }
                }
            }); ;

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
