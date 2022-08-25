using GCH.Core.Interfaces.Tables;
using GCH.Core.TelegramLogic.Handlers.Basic;
using GCH.Core.TelegramLogic.Interfaces;
using GCH.Core.TelegramLogic.TelegramUpdate;
using System.Globalization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Res = GCH.Resources.Resources;

namespace GCH.Core.TelegramLogic.Handlers.StartHandlers
{
    public class StartHandler : AbstractTelegramHandler
    {
        private readonly List<CultureInfo> _supportedCultures;

        public StartHandler(IWrappedTelegramClient client, IUserSettingsTable settingsTable) 
            : base(client, settingsTable)
        {
            _supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("en-US"),
                new CultureInfo("uk-UA"),
                new CultureInfo("ru-RU")
            };
        }

        protected override async Task HandleThen(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            var upd = notification.Update;
            var lang = upd.Message.From.LanguageCode;
            var culture = _supportedCultures.FirstOrDefault(it => it.Name.Contains(lang));
            
            if (culture != null)
            {
                UserSettings.Language = lang;
                await UserSettingsTable.SetSettings(UserSettings);
                Res.Culture = culture;
            }
            await ClientWrapper.Client.SendTextMessageAsync(upd.Message.Chat.Id,
                string.Format(Res.Greeting, upd.Message.From?.FirstName ?? "Gachi Brother"));
        }

        protected override bool When(TelegramUpdateNotification notification, CancellationToken cancellationToken)
        {
            return notification.Update.Type == UpdateType.Message
                && notification.Update.Message.Type == MessageType.Text
                && notification.Update.Message.Text == "/start";
        }
    }
}
