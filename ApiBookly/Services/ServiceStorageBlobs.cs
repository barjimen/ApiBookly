using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BooklyNugget.Models;

namespace ApiBookly.Services
{
    public class ServiceStorageBlobs
    {
        public BlobServiceClient client;

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

        public async Task<List<BlobModel>> GetBlobsAsync(string containerName)
        {
            List<BlobModel> blobs = new List<BlobModel>();
            BlobContainerClient container = this.client.GetBlobContainerClient(containerName);

            await foreach (BlobItem item in container.GetBlobsAsync())
            {
                BlobClient blob = container.GetBlobClient(item.Name);
                string url;
                string base64Image = string.Empty;

                if (container.GetProperties().Value.PublicAccess == PublicAccessType.None)
                {
                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = containerName,
                        BlobName = item.Name,
                        Resource = "b",
                        ExpiresOn = DateTime.UtcNow.AddHours(1) // Expira en 1 hora
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    Uri sasUri = blob.GenerateSasUri(sasBuilder);
                    url = sasUri.ToString();
                }
                else
                {
                    // Si el contenedor es público, usamos la URL directa
                    url = blob.Uri.AbsoluteUri;
                }

                blobs.Add(new BlobModel()
                {
                    Nombre = item.Name,
                    Url = url,
                    Container = containerName
                });
            }
            return blobs;
        }

        public BlobContainerClient GetContainerClient(string containerName)
        {
            return this.client.GetBlobContainerClient(containerName);
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);
            await containerClient.DeleteBlobAsync(blobName);
        }

        public async Task UploadBlobAsync(string containerName, string blobNane, Stream stream)
        {
            BlobContainerClient containerClient =
           this.client.GetBlobContainerClient(containerName);
            await containerClient.UploadBlobAsync
                (blobNane, stream);
        }
        public async Task UpdateBlobAsync(string containerName, string oldBlobName, string newBlobName, Stream stream)
        {
            BlobContainerClient containerClient = this.client.GetBlobContainerClient(containerName);

            await containerClient.UploadBlobAsync(newBlobName, stream);

            if (!string.IsNullOrEmpty(oldBlobName) && oldBlobName != newBlobName)
            {
                BlobClient oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                if (await oldBlobClient.ExistsAsync())
                {
                    await oldBlobClient.DeleteAsync();
                }
            }
        }

    }
}
