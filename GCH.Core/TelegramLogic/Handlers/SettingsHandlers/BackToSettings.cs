using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class BackToSettings : AbstractTelegramHandler
    {
        public BackToSettings(IWrappedTelegramClient client, IUserSettingsTable settingsTable)
            : base(client, settingsTable)
        {
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            if (notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageRu
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageEn
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageUa)
            {
                UserSettings.Language = upd.CallbackQuery.Data["language/".Length..];
                await UserSettingsTable.SetSettings(UserSettings);
            }
            var languageBtn = new InlineKeyboardButton($"{Resources.Resources.LanguageSelected}: {UserSettings.Language}")
            {
                CallbackData = Constants.SettingsButtons.Language
            };

            var markup = new InlineKeyboardMarkup(languageBtn);

            _ = await ClientWrapper.Client.EditMessageTextAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId,
                $"{Resources.Resources.ChooseSettingsToChange}:",
                replyMarkup: markup,
                cancellationToken: cancellationToken); ;
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && (notification.Update.CallbackQuery.Data == Constants.SettingsButtons.Settings
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageRu
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageEn
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageUa);
        }
    }
}
