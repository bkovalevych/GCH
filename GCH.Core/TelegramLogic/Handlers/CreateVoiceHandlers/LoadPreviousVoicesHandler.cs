﻿using GCH.Core.Interfaces.Sources;
using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class LoadPreviousVoicesHandler : AbstractTelegramHandler
    {
        private readonly IVoiceLabelSource _voiceSource;

        public LoadPreviousVoicesHandler(IWrappedTelegramClient client,
            IVoiceLabelSource voice, IUserSettingsTable userSettingsTable) : base(client, userSettingsTable)
        {
            _voiceSource = voice;
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var offset = int.Parse(upd.CallbackQuery.Data[Constants.CreateVoiceButtons.Previous.Length..]);

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

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.Previous);
        }
    }
}
