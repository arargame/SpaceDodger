using System.IO;

namespace SpaceDodger.Persistence
{
    /// <summary>
    /// Abstraction over where save data physically lives.
    /// Keeps <see cref="JsonSaveGameService"/> testable and platform-free.
    /// </summary>
    public interface IStorageProvider
    {
        bool Exists { get; }
        Stream OpenRead();
        Stream OpenWrite();
    }
}
