using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCH.Core.Interfaces.FfmpegHelpers
{
    public interface IOggReaderService
    {
        Task<Stream> Concat(Stream srcOne, Stream srcTwo);

        Task<Stream> ProcessVoiceMem(Stream stream);

        Task<TimeSpan> GetDuration(Uri uri);

    }
}
