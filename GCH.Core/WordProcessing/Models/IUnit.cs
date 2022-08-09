namespace GCH.Core.WordProcessing.Models
{
    public interface IUnit
    {
        Task<Stream> Compose();
    }
}
