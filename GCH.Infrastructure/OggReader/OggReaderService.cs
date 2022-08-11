using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;

namespace GCH.Infrastructure.OggReader
{
    public class OggReaderService
    {
        private readonly ILogger<OggReaderService> _logger;

        public OggReaderService(string binPath, string tempPath, ILogger<OggReaderService> logger)
        {
            if (!string.IsNullOrWhiteSpace(binPath))
            {
                GlobalFFOptions.Configure(new FFOptions
                {
                    BinaryFolder = binPath,
                    TemporaryFilesFolder = tempPath
                });
            }
            _logger = logger;
        }
        public async Task<Stream> ConcatStreams(Stream streamOne, Stream streamTwo)
        {
            var fileNameOne = Guid.NewGuid().ToString() + ".ogg";
            var fileNameTwo = Guid.NewGuid().ToString() + ".ogg";
            var memoryStream = new MemoryStream();
            _logger.LogWarning("start processing {0}", "message");
            try
            {
                var processed = await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(streamOne))
                .OutputToFile(fileNameOne, true, options => options
                    .ForceFormat("ogg"))
                .ProcessAsynchronously();
                processed = await FFMpegArguments
                    .FromPipeInput(new StreamPipeSource(streamTwo))
                    .OutputToFile(fileNameTwo, true, options => options
                        .ForceFormat("ogg"))
                    .ProcessAsynchronously();
                processed = await FFMpegArguments.FromConcatInput(new string[] { fileNameOne, fileNameTwo })
                    .OutputToPipe(new StreamPipeSink(memoryStream), 
                    args => args.ForceFormat("ogg"))
                    .ProcessAsynchronously();
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ogg error");
                throw;
            }
            finally
            {
                File.Delete(fileNameOne);
                File.Delete(fileNameTwo);
            }
        }

        public async Task<TimeSpan> GetDuration(Stream stream)
        {
            var bitRate = 8000;
            var duration = TimeSpan.FromSeconds(stream.Length / bitRate);
            return duration;
        }
    }
}
