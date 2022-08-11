using Azure.Data.Tables;

namespace GCH.Infrastructure.Tables
{
    public abstract class BaseTable
    {
        public TableClient UserSettingsTable { get; set; }

        public BaseTable(TableClient userSettingsTable)
        {
            UserSettingsTable = userSettingsTable;
        }
    }
}
