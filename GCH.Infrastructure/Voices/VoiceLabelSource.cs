using Azure;
using Azure.Data.Tables;
using GCH.Core.Interfaces.Sources;
using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;

namespace GCH.Infrastructure.Voices
{
    public class VoiceLabelSource : IVoiceLabelSource
    {
        private readonly TableClient _tableClient;

        public VoiceLabelSource(TableClient blobClient)
        {
            _tableClient = blobClient;
        }

        public async Task<PaginatedList<VoiceLabel>> LoadAsync(int offset = 0, int count = Constants.DefaultPageSize)
        {
            var voiceLabels = _tableClient.QueryAsync<VoiceLabelTableEntity>($"Id gt {offset}").AsPages(pageSizeHint: count);
            var items = new List<VoiceLabel>();
            await foreach (var blobPage in voiceLabels)
            {
                foreach(var blob in blobPage.Values.OrderBy(it => it.Id))
                {
                    items.Add(blob);
                }
                break;
            }
            var canLoadNext = await _tableClient.QueryAsync<VoiceLabelTableEntity>($"Id gt {offset + count}", 1)
                .GetAsyncEnumerator().MoveNextAsync();

            return new PaginatedList<VoiceLabel>()
            {
                Items = items,
                CanLoadNext = canLoadNext,
                Count = items.Count,
                Offset = offset
            };
        }
    }

    public class VoiceLabelTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public int Id { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string Blob { get; set; }

        public static implicit operator VoiceLabel(VoiceLabelTableEntity entity)
        {
            return new VoiceLabel()
            {
                Id = entity.Id,
                Blob = entity.Blob,
                Description = entity.Description,
                PartitionKey = entity.PartitionKey,
                Name = entity.Name,
                RowKey = entity.RowKey,
            };
        }
    }
}
