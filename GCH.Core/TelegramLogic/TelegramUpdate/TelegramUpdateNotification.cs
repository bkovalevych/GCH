using MediatR;
using Telegram.Bot.Types;

namespace GCH.Core.TelegramLogic.TelegramUpdate
{
    public class TelegramUpdateNotification : INotification
    {
        public Update Update { get; set; } 
    }
}
