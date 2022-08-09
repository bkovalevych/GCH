using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using MediatR;

namespace GCH.Core.TelegramLogic.Handlers.Basic
{
    public abstract class AbstractTelegramHandler : INotificationHandler<TelegramUpdateNotification>
    {
        protected IWrappedTelegramClient ClientWrapper { get; }

        public AbstractTelegramHandler(IWrappedTelegramClient client)
        {
            ClientWrapper = client;
        }

        public abstract bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken);
        public abstract Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken);
        
        public async Task Handle(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            if (When(notification, cancellationToken))
            {
                await HandleThen(notification, cancellationToken);
            }
        }
    }
}
