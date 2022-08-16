using FFMpegCore;
using FFMpegCore.Pipes;
using GCH.Core.Interfaces.FfmpegHelpers;
using GCH.Core.LoggerWrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GCH.Infrastructure.OggReader
{
    public class OggReaderService : IOggReaderService
    {
        private readonly LoggerWrapperService _loggerWrapper;

        public OggReaderService(IConfiguration configuration, LoggerWrapperService loggerWrapper)
        {
            string binPath = configuration["FfmpegBin"];
            string tempPath = configuration["TempFolder"];
            if (!string.IsNullOrWhiteSpace(binPath))
            {
                GlobalFFOptions.Configure(new FFOptions
                {
                    BinaryFolder = binPath,
                    TemporaryFilesFolder = tempPath,
                    WorkingDirectory = tempPath
                });

            }
            _loggerWrapper = loggerWrapper;
        }

        public async Task<Stream> Concat(Stream srcOne, Stream srcTwo)
        {
            var memoryStream = new MemoryStream();
            _loggerWrapper.Logger.LogDebug("start processing {}", srcOne);
            var tempFiles = new List<string>()
            {
                Path.Combine(GlobalFFOptions.Current.WorkingDirectory,  Guid.NewGuid().ToString() + ".ogg"),
                Path.Combine(GlobalFFOptions.Current.WorkingDirectory,  Guid.NewGuid().ToString() + ".ogg")
            };
            try
            {

                var firstCall = FFMpegArguments.FromPipeInput(new StreamPipeSource(srcOne), args => args.ForceFormat("ogg"))
                    .OutputToFile(tempFiles[0]).ProcessAsynchronously();
                var secondCall = FFMpegArguments.FromPipeInput(new StreamPipeSource(srcTwo), args => args.ForceFormat("ogg"))
                    .OutputToFile(tempFiles[1]).ProcessAsynchronously();
                await firstCall;
                await secondCall;
                await FFMpegArguments.FromConcatInput(tempFiles)
                    .OutputToPipe(new StreamPipeSink(memoryStream), args => args.ForceFormat("ogg"))
                    .ProcessAsynchronously();
                memoryStream.Position = 0;
                _loggerWrapper.Logger.LogInformation("Successful concat, Voice size {}", memoryStream.Length);
                return memoryStream;
            }
            catch (Exception ex)
            {
                _loggerWrapper.Logger.LogError(ex, "Ogg error. Message {}", ex.Message);
                throw;
            }
            finally
            {
                File.Delete(tempFiles[0]);
                File.Delete(tempFiles[1]);
            }
        }

        public async Task<Stream> ProcessVoiceMem(Stream stream)
        {
            var memStr = new MemoryStream();
            await FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
                .OutputToPipe(new StreamPipeSink(memStr), args => args.ForceFormat("ogg"))
                .ProcessAsynchronously();
            memStr.Position = 0;
            return memStr;
        }

        public async Task<TimeSpan> GetDuration(Uri uri)
        {
            var fileName = "./temp.ogg";

            try
            {
                var analysis = await FFProbe.AnalyseAsync(uri);
                return analysis.Duration;
            }
            finally 
            {
                File.Delete(fileName);
            }

        }
    }
}
