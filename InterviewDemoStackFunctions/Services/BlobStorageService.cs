using Azure.Storage.Blobs;
using InterviewDemoStackFunctions.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(string blobConnectionString)
        {
            _blobServiceClient = new BlobServiceClient(blobConnectionString);
        }

        public async Task UploadFileAsync(string containerName, string blobName, byte[] fileContent)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            using (var stream = new MemoryStream(fileContent))
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.UploadAsync(stream, overwrite: true);
                //TODO: Add message for event grid to show upload
            }
        }

        public async Task<byte[]> ReadFileAsync(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(memoryStream);
                    //TODO: Add message for event grid to show save
                    return memoryStream.ToArray();
                }
            }

            //TODO: Add message for event grid to record error
            throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");
        }
    }

}
