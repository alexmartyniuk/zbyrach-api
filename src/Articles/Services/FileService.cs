using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Zbyrach.Api.Articles
{
    public class FileService
    {
        private readonly string _rootDirectory;

        public FileService(IConfiguration config)
        {
            _rootDirectory = config["FileStorageRootDirectory"];

            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
        }

        public bool IsFileExists(string fileName)
        {
            return File.Exists(GetFullPath(fileName));
        }

        public async Task PutFile(string fileName, Stream stream)
        {
            using var fileStream = File.Create(GetFullPath(fileName));

            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(fileStream);
        }

        public async Task<Stream> GetFile(string fileName)
        {
            using var fileStream = File.OpenRead(GetFullPath(fileName));

            var stream = new MemoryStream();
            fileStream.Seek(0, SeekOrigin.Begin);
            await fileStream.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public async Task DeleteFile(string fileName)
        {
            File.Delete(GetFullPath(fileName));
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(_rootDirectory, fileName);
        }
    }

}