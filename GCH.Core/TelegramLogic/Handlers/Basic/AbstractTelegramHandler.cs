using GCH.Core.Interfaces.Tables;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using MediatR;
using System.Globalization;
using Telegram.Bot.Types.Enums;

namespace GCH.Core.TelegramLogic.Handlers.Basic
{
    public abstract class AbstractTelegramHandler : INotificationHandler<TelegramUpdateNotification>
    {
        protected IWrappedTelegramClient ClientWrapper { get; }
        protected IUserSettingsTable UserSettingsTable { get; }
        protected UserSettings UserSettings { get; set; }
        
        public AbstractTelegramHandler(IWrappedTelegramClient client, IUserSettingsTable userSettingsTable)
        {
            ClientWrapper = client;
            UserSettingsTable = userSettingsTable;
        }

        protected abstract bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken);
        protected abstract Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken);
        
        protected virtual long? GetChatId(TelegramUpdateNotification notification)
        {
            var upd = notification.Update;
            return upd.Type switch
            {
                UpdateType.CallbackQuery => upd.CallbackQuery.Message.Chat.Id,
                UpdateType.Message => upd.Message.Chat.Id,
                _ => null,
            };
        }

        public async Task Handle(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            if (When(notification, cancellationToken))
            {
                await InitSettingsAndResourceLocale(notification);
                await HandleThen(notification, cancellationToken);
            }
        }

        private async Task InitSettingsAndResourceLocale(TelegramUpdateNotification notification)
        {
            if (GetChatId(notification) is long id)
            {
                UserSettings = await UserSettingsTable.GetByChatId(id);
                Resources.Resources.Culture = new CultureInfo(UserSettings.Language);
            }
        }
    }
}
