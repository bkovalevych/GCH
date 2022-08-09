using NVorbis;

namespace GCH.Infrastructure.OggReader
{
    public class OggReaderService
    {
        public Stream ReadFromStream(Stream stream)
        {
            using var vorbis = new VorbisReader(stream);
            vorbis.FindNextStream();
            var output = new MemoryStream();
            var buffer = new float[4096];
            var readedBytes = 0;
            var readNow = 0;
            do
            {
                readNow = vorbis.ReadSamples(buffer, readedBytes, buffer.Length);
                readedBytes += readNow;
                foreach (var sample in buffer)
                {
                    byte[] vOut = BitConverter.GetBytes(sample);
                    output.Write(vOut, 0, 2);
                }
            } while (readNow > 0);

            return output;
        }
    }
}
