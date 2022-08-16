using Azure;
using Azure.Data.Tables;
using GCH.Core.Interfaces.Tables;
using GCH.Core.LoggerWrapper;
using GCH.Core.Models;
using Microsoft.Extensions.Logging;

namespace GCH.Infrastructure.Tables
{
    public class UserSettingsTable : BaseTable, IUserSettingsTable
    {
        private const string PartitionKeyForSettings = "userSettings";
        private readonly LoggerWrapperService _loggerWrapper;

        public UserSettingsTable(TableClient userSettingsTable, 
            LoggerWrapperService logger) : base(userSettingsTable)
        {
            _loggerWrapper = logger;
        }

        public async Task<UserSettings> GetByChatId(long chatId)
        {
            _loggerWrapper.Logger.LogDebug("start getting settings. ChatId = {}", chatId);
            await UserSettingsTable.CreateIfNotExistsAsync();
            var settings = new UserSettings()
            {
                ChatId = chatId,
                Language = "en",
                LastVoiceId = ""
            };
            
            var queryEnumerator = UserSettingsTable
                .QueryAsync<UserSettingsEntity>(settings =>
                settings.PartitionKey == PartitionKeyForSettings &&
                settings.RowKey == chatId.ToString())
                .GetAsyncEnumerator();
            var exists = await queryEnumerator.MoveNextAsync();
            if (exists)
            {
                settings = queryEnumerator.Current;
            }
            else
            {
                await UserSettingsTable.AddEntityAsync<UserSettingsEntity>(settings);
            }
            _loggerWrapper.Logger.LogInformation("Got settings. ChatId = {}, LastVoiceId = {}", 
                settings.ChatId, settings.LastVoiceId);    

            return settings;
        }

        public async Task SetSettings(UserSettings settings)
        {
            _loggerWrapper.Logger.LogDebug("Start set settings");
            await UserSettingsTable.UpdateEntityAsync<UserSettingsEntity>(settings, ETag.All);
        }

        private class UserSettingsEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Language { get; set; }
            public string LastVoiceId { get; set; }

            public static implicit operator UserSettingsEntity(UserSettings settings)
            {
                return new UserSettingsEntity()
                {
                    Language = settings.Language,
                    LastVoiceId = settings.LastVoiceId,
                    RowKey = settings.ChatId.ToString(),
                    PartitionKey = PartitionKeyForSettings
                };
            }

            public static implicit operator UserSettings(UserSettingsEntity entity)
            {
                return new UserSettings()
                {
                    ChatId = long.Parse(entity.RowKey),
                    Language = entity.Language,
                    LastVoiceId = entity.LastVoiceId
                };
            }

            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }
    }
}
