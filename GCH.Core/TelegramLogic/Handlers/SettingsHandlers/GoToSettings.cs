using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using GCH.Core.Interfaces.Tables;

namespace GCH.Core.TelegramLogic.Handlers.SettingsHandlers
{
    public class GoToSettings : AbstractTelegramHandler
    {
        private readonly IUserSettingsTable _userSettingstable;

        public GoToSettings(IWrappedTelegramClient client, 
            IUserSettingsTable userSettingsTable) : base(client)
        {
            _userSettingstable = userSettingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _userSettingstable.GetByChatId(upd.Message.Chat.Id);
            var languageBtn = new InlineKeyboardButton($"Language: {settings.Language}") 
            { 
                CallbackData = Constants.SettingsButtons.Language
            };
            
            var markup = new InlineKeyboardMarkup(languageBtn);

            _ = await ClientWrapper.Client.SendTextMessageAsync(
                upd.Message.Chat.Id, 
                "Choose settings to change:", 
                replyMarkup: markup, 
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message 
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/settings";
        }
    }
}
