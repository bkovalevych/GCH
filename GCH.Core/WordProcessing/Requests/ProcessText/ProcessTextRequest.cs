using LanguageExt.Common;
using MediatR;

namespace GCH.Core.WordProcessing.Requests.ProcessText
{
    public class ProcessTextRequest : IRequest<Result<string>>
    {
        public string Language { get; set; }

        public string Text { get; set; }
    }
}
