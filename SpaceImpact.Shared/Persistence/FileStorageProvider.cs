using System.IO;

namespace SpaceImpact.Persistence
{
    /// <summary>Plain file-system storage; both platforms just supply a directory.</summary>
    public sealed class FileStorageProvider : IStorageProvider
    {
        private readonly string _path;

        public FileStorageProvider(string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            _path = Path.Combine(directory, fileName);
        }

        public bool Exists => File.Exists(_path);

        public Stream OpenRead() => File.OpenRead(_path);

        public Stream OpenWrite() =>
            new FileStream(_path, FileMode.Create, FileAccess.Write);
    }
}
