namespace Kk.Kharts.Shared.DTOs
{
    public class CacheVersionDTO
    {
        public uint LocalStorageVersion { get; set; }
        public uint IndexedDbVersion { get; set; }
        public uint CacheStorageVersion { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateCacheVersionDTO
    {
        public bool? IncrementLocalStorage { get; set; }
        public bool? IncrementIndexedDb { get; set; }
        public bool? IncrementCache { get; set; }
    }

    public class SetCacheVersionDTO
    {
        public uint? LocalStorageVersion { get; set; }
        public uint? IndexedDbVersion { get; set; }
        public uint? CacheStorageVersion { get; set; }
    }
}
