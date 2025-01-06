using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task UploadFileAsync(string containerName, string blobName, byte[] fileContent);
        Task<byte[]> ReadFileAsync(string containerName, string blobName);
    }

}
