using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("File")]
    public class File : IEntity
    {
        [Key]
        public long Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long? Subgroup { get; set; }
        public long? NormalGroup { get; set; }
    }
}
