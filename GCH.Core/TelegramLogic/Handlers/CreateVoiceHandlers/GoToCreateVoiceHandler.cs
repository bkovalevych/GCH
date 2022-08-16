using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class GoToCreateVoiceHandler : AbstractTelegramHandler
    {
        private readonly IUserVoicesContainer _userVoiceContainer;
        private readonly IVoiceLabelSource _voiceSource;
        private readonly IUserSettingsTable _userSettingsTable;

        public GoToCreateVoiceHandler(IWrappedTelegramClient client, 
            IUserVoicesContainer userVoiceContainer,
            IVoiceLabelSource voiceSource, IUserSettingsTable userSettingsTable) : base(client)
        {
            _userVoiceContainer = userVoiceContainer;
            _voiceSource = voiceSource;
            _userSettingsTable = userSettingsTable;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var settings = await _userSettingsTable.GetByChatId(upd.Message.Chat.Id);
            Resources.Resources.Culture = new CultureInfo(settings.Language);
            var pagedResult = await _voiceSource.LoadAsync();
            var buttons = ChatVoiceHelpers.AddFooterButtons(pagedResult, "");
            if (!string.IsNullOrEmpty(settings.LastVoiceId))
            {
                await _userVoiceContainer.BlobContainer.DeleteBlobIfExistsAsync(settings.LastVoiceId + ".ogg", 
                    cancellationToken: cancellationToken);
            }
            settings.LastVoiceId = "";
            await _userSettingsTable.SetSettings(settings);

            var markup = new InlineKeyboardMarkup(buttons);

            _ = await ClientWrapper.Client.SendTextMessageAsync(
                upd.Message.Chat.Id,
                $"{Resources.Resources.SendMeVoice}:",
                replyMarkup: markup,
                cancellationToken: cancellationToken);
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/create";
        }
    }
}
