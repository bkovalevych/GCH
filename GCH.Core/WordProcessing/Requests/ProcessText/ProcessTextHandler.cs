using GCH.Core.WordProcessing.Models;
using LanguageExt.Common;
using MediatR;
using System.Text.RegularExpressions;

namespace GCH.Core.WordProcessing.Requests.ProcessText
{
    public class ProcessTextHandler : IRequestHandler<ProcessTextRequest, Result<List<IUnit>>>
    {
        private static char _sepChar = '\"';
        private int _lastIndex;
        private string _text;

        public async Task<Result<List<IUnit>>> Handle(ProcessTextRequest request, CancellationToken cancellationToken)
        {
            _text = request.Text.Trim();
            _lastIndex = 0;
            var matches = Regex.Matches(_text, $"{_sepChar}(.*?){_sepChar}", RegexOptions.Multiline);
            return matches.Aggregate(new List<IUnit>(),
                AggregateHandler,
                AggregateResultHandler);
        }
        private List<IUnit> AggregateHandler(List<IUnit> accumulate, Match match)
        {
            bool needText = _lastIndex < match.Index;
            if (needText)
            {
                accumulate.Add(new TextUnit()
                {
                    Text = _text[_lastIndex..match.Index]
                });
            }
            accumulate.Add(new GCHLabelUnit()
            {
                ShortName = match.Groups[1].Value
            });
            _lastIndex = match.Index + match.Length;
            return accumulate;
        }

        private List<IUnit> AggregateResultHandler(List<IUnit> accumulate)
        {
            bool needText = _lastIndex < _text.Length;
            if (needText)
            {
                accumulate.Add(new TextUnit()
                {
                    Text = _text[_lastIndex..]
                });
            }
            return accumulate;
        }
    }
}
