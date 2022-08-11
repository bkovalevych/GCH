using Azure.Storage.Blobs;

namespace GCH.Infrastructure.BlobContainers
{
    public abstract class AbstractContainer
    {
        public AbstractContainer(BlobContainerClient blobContainer)
        {
            BlobContainer = blobContainer;
        }

        public BlobContainerClient BlobContainer { get; set; }
    }
}
