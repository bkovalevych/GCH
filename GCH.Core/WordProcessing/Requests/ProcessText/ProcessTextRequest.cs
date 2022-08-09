using GCH.Core.WordProcessing.Models;
using LanguageExt.Common;
using MediatR;

namespace GCH.Core.WordProcessing.Requests.ProcessText
{
    public class ProcessTextRequest : IRequest<Result<List<IUnit>>>
    {
        public string Language { get; set; }

        public string Text { get; set; }
    }
}
