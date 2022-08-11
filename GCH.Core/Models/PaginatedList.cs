using GCH.Core.TelegramLogic.Handlers.Basic;

namespace GCH.Core.Models
{
    public class PaginatedList<TItem>
    {
        public IEnumerable<TItem> Items { get; set; } = new List<TItem>();

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = Constants.DefaultPageSize;

        public bool CanLoadNext { get; set; }

        public bool CanLoadPrevious => Offset > 0;
    }
}
