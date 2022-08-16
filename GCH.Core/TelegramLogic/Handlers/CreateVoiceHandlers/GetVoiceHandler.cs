using Azure.Storage.Sas;
using GCH.Core.Interfaces.BlobContainers;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types;
using GCH.Core.Interfaces.Tables;
using System.Globalization;

namespace GCH.Core.TelegramLogic.Handlers.CreateVoiceHandlers
{
    public class GetVoiceHandler : AbstractTelegramHandler
    {
        private readonly IUserSettingsTable _userSettingsTable;
        private readonly IUserVoicesContainer _container;

        public GetVoiceHandler(IWrappedTelegramClient client, IUserVoicesContainer container, IUserSettingsTable userSettingsTable) : base(client)
        {
            _userSettingsTable = userSettingsTable;
            _container = container;
        }

        public override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;

            var blobName = upd.CallbackQuery.Data[Constants.CreateVoiceButtons.GetVoice.Length..] + ".ogg";
            var blob = _container.BlobContainer.GetBlobClient(blobName);
            var settings = await _userSettingsTable.GetByChatId(upd.CallbackQuery.Message.Chat.Id);
            Resources.Resources.Culture = new CultureInfo(settings.Language);
            if (await blob.ExistsAsync(cancellationToken: cancellationToken))
            {
                var uri = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddHours(1));
                await ClientWrapper.Client.SendVoiceAsync(
                    upd.CallbackQuery.Message.Chat.Id, 
                    new InputOnlineFile(uri),
                    caption: Resources.Resources.VoiceCaption,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await ClientWrapper.Client.DeleteMessageAsync(
                    upd.CallbackQuery.Message.Chat.Id,
                    upd.CallbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken);
                await ClientWrapper.Client.SendTextMessageAsync(
                    upd.CallbackQuery.Message.Chat.Id,
                    Resources.Resources.FileWasNotFound,
                    cancellationToken: cancellationToken);
            }
        }

        public override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.CallbackQuery
                && notification.Update.CallbackQuery.Data.StartsWith(Constants.CreateVoiceButtons.GetVoice);
        }
    }
}
