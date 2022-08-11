namespace GCH.Core.TelegramLogic.Handlers.Basic
{
    public static class Constants
    {
        public const int DefaultPageSize = 10;
        public static TimeSpan MaxDuration { get => TimeSpan.FromMinutes(2); }
        public static class SettingsButtons
        {
            public const string Settings = "settings";

            public const string Language = "language";

            public const string LanguageEn = "language/en";

            public const string LanguageUa = "language/ua";

            public const string LanguageRu = "language/ru";
        }

        public static class CreateVoiceButtons
        {
            public const string GetVoice = "createVoice/getVoice";

            public const string State = "createVoice/state";

            public const string Next = "createVoice/next";

            public const string Previous = "createVoice/prev";

            public const string Cancel = "createVoice/cancel";

            public const string ContnetPrefix = "createVoice/_";
        }
    }
}
