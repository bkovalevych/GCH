using Azure.Storage.Blobs;
using GCH.Core.Interfaces.BlobContainers;

namespace GCH.Infrastructure.BlobContainers
{
    public class UserVoicesContainer : AbstractContainer, IUserVoicesContainer
    {
        public UserVoicesContainer(BlobContainerClient blobContainer) : base(blobContainer)
        {
        }
    }
}
