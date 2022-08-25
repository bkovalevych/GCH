using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class CancelCreationVoiceHandler : AbstractTelegramHandler
    {
        private readonly IUserVoicesContainer _userVoiceContainer;

        public CancelCreationVoiceHandler(IWrappedTelegramClient client,
            IUserSettingsTable settingsTable,
            IUserVoicesContainer userVoiceContainer) : base(client, settingsTable)
        {
            _userVoiceContainer = userVoiceContainer;
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            if (!string.IsNullOrEmpty(UserSettings.LastVoiceId))
            {
                await _userVoiceContainer.BlobContainer.DeleteBlobIfExistsAsync(UserSettings.LastVoiceId + ".ogg",
                    cancellationToken: cancellationToken);

                UserSettings.LastVoiceId = "";
                await UserSettingsTable.SetSettings(UserSettings);
            }
            await ClientWrapper.Client
                .AnswerCallbackQueryAsync(upd.CallbackQuery.Id, Resources.Resources.CancelAlert, showAlert: true);
            await ClientWrapper.Client.DeleteMessageAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data == Constants.CreateVoiceButtons.Cancel;
        }
    }
}
