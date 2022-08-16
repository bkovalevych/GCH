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
    public class LoadPreviousVoicesHandler : AbstractTelegramHandler
    {
        private readonly IVoiceLabelSource _voiceSource;
        private readonly IUserSettingsTable _userSettingsTable;

        public LoadPreviousVoicesHandler(IWrappedTelegramClient client, IVoiceLabelSource voice, IUserSettingsTable userSettingsTable) : base(client)
        {
            _voiceSource = voice;
            _userSettingsTable = userSettingsTable; 
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var offset = int.Parse(upd.CallbackQuery.Data[Constants.CreateVoiceButtons.Previous.Length..]);
            var settings = await _userSettingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            Resources.Resources.Culture = new CultureInfo(settings.Language);
            
            var pagedResult = await _voiceSource.LoadAsync(offset);
            var fileName = ChatVoiceHelpers.GetFileName(upd.CallbackQuery.Message.ReplyMarkup.InlineKeyboard);

            var buttons = ChatVoiceHelpers.AddFooterButtons(pagedResult, fileName);

            var markup = new InlineKeyboardMarkup(buttons);

            _ = await ClientWrapper.Client.EditMessageReplyMarkupAsync(
                upd.CallbackQuery.Message.Chat.Id,
                upd.CallbackQuery.Message.MessageId,
                replyMarkup: markup,
                cancellationToken: cancellationToken);

        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.Previous);
        }
    }
}
