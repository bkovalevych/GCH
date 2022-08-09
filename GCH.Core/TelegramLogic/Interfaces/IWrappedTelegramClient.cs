using Telegram.Bot;
using Telegram.Bot.Types;

namespace GCH.Core.TelegramLogic.Interfaces
{
    public interface IWrappedTelegramClient
    {
        TelegramBotClient Client { get; }

        Task SetWebhookAsync(string newUrl = null);

        Task SendAnswer(string text, Update update);

        Task SendMusic(string uriVoice, Update update);

        Task SendMusic(Stream voice, Update update);
    }
}
