using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace SegundoExamenAzure.Services
{
    public class ServiceStorageBlobs
    {
        private BlobServiceClient client;

        public ServiceStorageBlobs(BlobServiceClient client)
        {
            this.client = client;
        }

        public async Task<List<string>> GetContainers()
        {
            List<string> container = new List<string>();
            await foreach (BlobContainerItem item in this.client.GetBlobContainersAsync())
            {
                container.Add(item.Name);
            }
            return container;
        }

        public string GetContainerUrl(string containerName)
        {
            BlobContainerClient container = this.client.GetBlobContainerClient(containerName);
            return container.Uri.AbsoluteUri;
        }

        public async Task<string> GetBlob(string containerName, string blobName)
        {
            BlobContainerClient container = this.client.GetBlobContainerClient(containerName);
            BlobClient blob = container.GetBlobClient(blobName);
            BlobProperties properties = await blob.GetPropertiesAsync();
            return blob.Uri.AbsoluteUri;
        }

        public BlobContainerClient GetContainerClient(string containerName)
        {
            return this.client.GetBlobContainerClient(containerName);
        }

    }
}
