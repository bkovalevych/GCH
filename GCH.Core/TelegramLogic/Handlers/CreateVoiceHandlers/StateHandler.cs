using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;


namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class StateHandler : AbstractTelegramHandler
    {
        private IUserSettingsTable _userSettingsTable;

        public StateHandler(IWrappedTelegramClient client, IUserSettingsTable settingsTable) : base(client)
        {
            _userSettingsTable = settingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _userSettingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            int offset = int.Parse(
                ChatVoiceHelpers.GetOffset(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard));
            
            Resources.Resources.Culture = new CultureInfo(settings.Language);
            await ClientWrapper.Client.AnswerCallbackQueryAsync(upd.CallbackQuery.Id,
                string.Format(Resources.Resources.StateAlertMessage, offset + 1, offset + Constants.DefaultPageSize),
                true, cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.State);
        }
    }
}
