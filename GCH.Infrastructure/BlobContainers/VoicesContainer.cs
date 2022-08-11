using Azure.Storage.Blobs;
using GCH.Core.Interfaces.BlobContainers;

namespace GCH.Infrastructure.BlobContainers
{
    public class VoicesContainer : AbstractContainer, IVoicesContainer
    {
        public VoicesContainer(BlobContainerClient blobContainer) : base(blobContainer)
        {
        }
    }
}
