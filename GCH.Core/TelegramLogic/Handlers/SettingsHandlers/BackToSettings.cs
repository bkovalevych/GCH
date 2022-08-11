using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using GCH.Core.Interfaces.Tables;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class BackToSettings : AbstractTelegramHandler
    {
        private readonly IUserSettingsTable _userSettingstable;

        public BackToSettings(IWrappedTelegramClient client, IUserSettingsTable settingsTable) : base(client)
        {
            _userSettingstable = settingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _userSettingstable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            if (notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageRu
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageEn
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageUa)
            {
                settings.Language = upd.CallbackQuery.Data["language/".Length..];
                await _userSettingstable.SetSettings(settings);
            }

            var languageBtn = new InlineKeyboardButton($"Language: {settings.Language}") 
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
                && (notification.Update.CallbackQuery.Data == Constants.SettingsButtons.Settings
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageRu
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageEn
                || notification.Update.CallbackQuery.Data == Constants.SettingsButtons.LanguageUa);
        }
    }
}
