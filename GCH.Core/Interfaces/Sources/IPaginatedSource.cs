using GCH.Core.Models;
using GCH.Core.TelegramLogic.Handlers.Basic;

namespace GCH.Core.Interfaces.Sources
{
    public interface IPaginatedSource<TItem>
    {
        Task<PaginatedList<TItem>> LoadAsync(int offset = 0, int count = Constants.DefaultPageSize);
    }
}
