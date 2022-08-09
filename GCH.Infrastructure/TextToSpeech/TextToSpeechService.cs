using Microsoft.CognitiveServices.Speech;
using System;

namespace GCH.Infrastructure.TextToSpeech
{
    public class TextToSpeechService
    {
        private readonly string _region;
        private readonly string _speechKey;
        
        public TextToSpeechService(string speechKey, string region)
        {
            _region = region;
            
            _speechKey = speechKey;
        }

        public async Task<Stream> FromText(string text)
        {
            var config = SpeechConfig.FromSubscription(_speechKey, _region);
            config.SpeechSynthesisVoiceName = "uk-UA-OstapNeural";
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz192KBitRateMonoMp3);
            using var synthesizer = new SpeechSynthesizer(config, null);

            var resultStream = new MemoryStream();
            var result = await synthesizer.SpeakTextAsync(text);
            using var stream = AudioDataStream.FromResult(result);
            uint startPosition = 0;
            uint readedBytes = 0;
            byte[] buffer = new byte[4096];
            do
            {
                readedBytes = stream.ReadData(startPosition, buffer);
                await resultStream.WriteAsync(buffer.AsMemory(0, (int)readedBytes));
                startPosition += readedBytes;
            } while (readedBytes != 0);
            resultStream.Position = 0;
            return resultStream;
        }
    }
}
