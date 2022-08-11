using Azure.Storage.Blobs;

namespace GCH.Core.Interfaces.BlobContainers
{
    public interface IBaseBlobContainer
    {
        BlobContainerClient BlobContainer { get; }
    }
}
