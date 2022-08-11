using GCH.Core.TelegramLogic.Interfaces;
using GCH.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;

namespace GCH.Infrastructure.TelegramBot.Services
{
    public class WrappedTelegramClient : IWrappedTelegramClient
    {
        public TelegramBotClient Client { get; }
        private readonly TelegramBotSettings _settings;

        public WrappedTelegramClient(IOptions<TelegramBotSettings> settings)
        {
            _settings = settings.Value;
            Client = new TelegramBotClient(_settings.Token);
        }

        public async Task SetWebhookAsync(string newUrl = null)
        {
            if (!string.IsNullOrEmpty(newUrl))
            {
                Environment.SetEnvironmentVariable("TelegramBotWebHook", newUrl, EnvironmentVariableTarget.Process);
            }

            var result = await Client.GetWebhookInfoAsync();
            var url = Environment.GetEnvironmentVariable("TelegramBotWebHook", EnvironmentVariableTarget.Process);
            if (url != null && (result == null || string.IsNullOrEmpty (result.Url) || url != result.Url))
            {
                await Client.SetWebhookAsync(url);
            }
        }

        public async Task SendAnswer(string text, Update update)
        {
            if (update.Message?.Chat.Id != null)
            {
                await Client.SendTextMessageAsync(update.Message.Chat.Id, text);
            }
        }

        public async Task SendMusic(string uriVoice, Update update)
        {
            if (update.Message?.Chat.Id != null)
            {
                await Client.SendAudioAsync(update.Message.Chat.Id, new InputOnlineFile(uriVoice) 
                { 
                    FileName = "right voice"
                }, "From Gachi Bot");
            }
            if (update.InlineQuery?.Id != null)
            {
                
                var results = new List<InlineQueryResult>()
                {
                    new InlineQueryResultArticle("2", "Help", new InputTextMessageContent(
                        "Write your text using double quotes like \"boy next door\" and combine it with regular text (e. g. \"master\" is good boy). To get all awailable gachi voices write /getVoices in private chat"))
                };
                if (!string.IsNullOrEmpty(uriVoice))
                {
                    results.Insert(0, new InlineQueryResultAudio("1", uriVoice, "Send ♂RIGHT VERSION♂ Voice"));
                }
                await Client.AnswerInlineQueryAsync(update.InlineQuery.Id, results);
            }
        }

        public async Task SendMusic(Stream voice, Update update)
        {
            if (update.Message?.Chat.Id != null)
            {
                await Client.SendAudioAsync(update.Message.Chat.Id, new InputOnlineFile(voice), "From ♂@GachiBillyVoice_bot♂");
            }
        }
    }
}
