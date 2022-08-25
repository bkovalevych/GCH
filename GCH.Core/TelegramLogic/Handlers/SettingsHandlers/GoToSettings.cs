using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Res = GCH.Resources.Resources;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class GoToSettings : AbstractTelegramHandler
    {
        public GoToSettings(IWrappedTelegramClient client,
            IUserSettingsTable userSettingsTable) : base(client, userSettingsTable)
        {
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var languageBtn = new InlineKeyboardButton($"{Res.LanguageSelected}: {UserSettings.Language}")
            {
                CallbackData = Constants.SettingsButtons.Language
            };

            var markup = new InlineKeyboardMarkup(languageBtn);

            _ = await ClientWrapper.Client.SendTextMessageAsync(
                upd.Message.Chat.Id,
                Res.Settings,
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/settings";
        }
    }
}
