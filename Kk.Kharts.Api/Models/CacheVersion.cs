using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kk.Kharts.Api.Models
{
    [Table("cache_versions", Schema = "kropKharts")]
    public class CacheVersion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("local_storage_version")]
        public uint LocalStorageVersion { get; set; }

        [Column("indexed_db_version")]
        public uint IndexedDbVersion { get; set; }

        [Column("cache_version")]
        public uint CacheStorageVersion { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("updated_by")]
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
