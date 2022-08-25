using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class GoToCreateVoiceHandler : AbstractTelegramHandler
    {
        private readonly IUserVoicesContainer _userVoiceContainer;
        private readonly IVoiceLabelSource _voiceSource;

        public GoToCreateVoiceHandler(IWrappedTelegramClient client,
            IUserVoicesContainer userVoiceContainer,
            IVoiceLabelSource voiceSource, IUserSettingsTable userSettingsTable)
            : base(client, userSettingsTable)
        {
            _userVoiceContainer = userVoiceContainer;
            _voiceSource = voiceSource;
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var pagedResult = await _voiceSource.LoadAsync();
            var buttons = ChatVoiceHelpers.AddFooterButtons(pagedResult, "");
            if (!string.IsNullOrEmpty(UserSettings.LastVoiceId))
            {
                await _userVoiceContainer.BlobContainer.DeleteBlobIfExistsAsync(UserSettings.LastVoiceId + ".ogg",
                    cancellationToken: cancellationToken);
            }
            UserSettings.LastVoiceId = "";
            await UserSettingsTable.SetSettings(UserSettings);

            var markup = new InlineKeyboardMarkup(buttons);

            _ = await ClientWrapper.Client.SendTextMessageAsync(
                upd.Message.Chat.Id,
                $"{Resources.Resources.SendMeVoice}:",
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/create";
        }
    }
}
