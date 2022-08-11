using GCH.Core.Models;

namespace GCH.Core.Interfaces.Tables
{
    public interface IUserSettingsTable
    {
        Task<UserSettings> GetByChatId(long chatId);
        Task SetSettings(UserSettings settings);
    }
}
