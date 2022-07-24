﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureStorageApi.Helpers
{
    public class BlobStorage : IBlobStorage
    {
        public async Task<List<string>> GetAllDocuments(string connectionString, string containerName)
        {
            var container = BlobExtensions.GetContainer(connectionString, containerName);

            if (!await container.ExistsAsync())
            {
                return new List<string>();
            }

            List<string> blobs = new();

            await foreach (BlobItem blobItem in container.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }
            return blobs;
        }

        public async Task UploadDocument(string connectionString, string containerName, string fileName, Stream fileContent)
        {
            var container = BlobExtensions.GetContainer(connectionString, containerName);
            if (!await container.ExistsAsync())
            {
                BlobServiceClient blobServiceClient = new(connectionString);
                await blobServiceClient.CreateBlobContainerAsync(containerName);
                container = blobServiceClient.GetBlobContainerClient(containerName);
            }

            var bobclient = container.GetBlobClient(fileName);
            if (!bobclient.Exists())
            {
                fileContent.Position = 0;
                await container.UploadBlobAsync(fileName, fileContent);
            }
            else
            {
                fileContent.Position = 0;
                await bobclient.UploadAsync(fileContent, overwrite: true);
            }
        }
        public async Task<Stream> GetDocument(string connectionString, string containerName, string fileName)
        {
            var container = BlobExtensions.GetContainer(connectionString, containerName);
            if (await container.ExistsAsync())
            {
                var blobClient = container.GetBlobClient(fileName);
                if (blobClient.Exists())
                {
                    var content = await blobClient.DownloadStreamingAsync();
                    return content.Value.Content;
                }
                else
                {
                    throw new FileNotFoundException();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

        }

        public async Task<bool> DeleteDocument(string connectionString, string containerName, string fileName)
        {
            var container = BlobExtensions.GetContainer(connectionString, containerName);
            if (!await container.ExistsAsync())
            {
                return false;
            }

            var blobClient = container.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class BlobExtensions
    {
        public static BlobContainerClient GetContainer(string connectionString, string containerName)
        {
            BlobServiceClient blobServiceClient = new(connectionString);
            return blobServiceClient.GetBlobContainerClient(containerName);
        }
    }
}
