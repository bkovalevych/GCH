using Azure;
using Azure.Data.Tables;
using GCH.Core.Interfaces.Tables;
using GCH.Core.Models;

namespace GCH.Infrastructure.Tables
{
    public class UserSettingsTable : BaseTable, IUserSettingsTable
    {
        private const string PartitionKeyForSettings = "userSettings";
        public UserSettingsTable(TableClient userSettingsTable) : base(userSettingsTable)
        {
        }

        public async Task<UserSettings> GetByChatId(long chatId)
        {
            await UserSettingsTable.CreateIfNotExistsAsync();
            UserSettings settings = new UserSettings()
            {
                ChatId = chatId,
                Language = "en",
                LastVoiceId = ""
            };
            try
            {
                var settingsResponse = await UserSettingsTable
                    .GetEntityAsync<UserSettingsEntity>(
                    PartitionKeyForSettings, 
                    chatId.ToString());
                settings = settingsResponse.Value;
            } 
            catch (Exception e)
            {
                await UserSettingsTable.AddEntityAsync<UserSettingsEntity>(settings);
            }
            return settings;
        }

        public async Task SetSettings(UserSettings settings)
        {
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
