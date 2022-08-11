using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using GCH.Core.Interfaces.Tables;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class CancelCreationVoiceHandler : AbstractTelegramHandler
    {
        private readonly IUserSettingsTable _settingsTable;

        public CancelCreationVoiceHandler(IWrappedTelegramClient client, 
            IUserSettingsTable settingsTable) : base(client)
        {
            _settingsTable = settingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _settingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            if (!string.IsNullOrEmpty(settings.LastVoiceId))
            {
                settings.LastVoiceId = "";
                await _settingsTable.SetSettings(settings);
            }
            await ClientWrapper.Client.DeleteMessageAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId, 
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data == Constants.CreateVoiceButtons.Cancel;
        }
    }
}
