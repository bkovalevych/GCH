using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;


namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class StateHandler : AbstractTelegramHandler
    {
        public StateHandler(IWrappedTelegramClient client, IUserSettingsTable settingsTable)
            : base(client, settingsTable)
        {
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            int offset = int.Parse(
                ChatVoiceHelpers.GetOffset(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard));

            await ClientWrapper.Client.AnswerCallbackQueryAsync(upd.CallbackQuery.Id,
                string.Format(Resources.Resources.StateAlertMessage, offset + 1, offset + Constants.DefaultPageSize),
                true, cancellationToken: cancellationToken);
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.State);
        }
    }
}
