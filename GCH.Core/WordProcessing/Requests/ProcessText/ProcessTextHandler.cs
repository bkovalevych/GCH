using LanguageExt.Common;
using MediatR;

namespace GCH.Core.WordProcessing.Requests.ProcessText
{
    public class ProcessTextHandler : IRequestHandler<ProcessTextRequest, Result<string>>
    {
        public async Task<Result<string>> Handle(ProcessTextRequest request, CancellationToken cancellationToken)
        {
            return "I am working";
        }
    }
}
